﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetGeo.Core;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SQLitePCL;


namespace DotNetGeo.GeoPackage;

internal class GeoPackageReader
{
    private SqliteConnection Connection { get; init; }

    public GeoPackageReader(string dbFile)
    {
        var connectionString = new SqliteConnectionStringBuilder($"Data Source={dbFile}")
        {
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        Connection = new(connectionString);
    }

    /* Things we need:
     * 
     * Get list of Spatial reference systems (gpkg_spatial_ref_sys).
     * Get table of tables (gpkg_contents) 
     * Get N matching features from feature table X in SRS Y
     * Get specific feature from table X in SRS Y
     * 
     */

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        var bbox = request.bbox.AsPolygon();
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(bbox);

        var matchCountCommandText =
            "SELECT COUNT(*) FROM ($table)" +
            "WHERE (($table).geom && " +
            "Transform(Extent(GeomFromGPB(($boundingBox))), ($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS)))";
        var matchCountCommand = new SqliteCommand(matchCountCommandText, Connection);
        matchCountCommand.Parameters.AddWithValue("table", request.collectionID);
        matchCountCommand.Parameters.AddWithValue("bbox", bboxBytes);
        matchCountCommand.Parameters.AddWithValue("originSRS", "EPSG:4326");
        matchCountCommand.Parameters.AddWithValue("destinationSRSnumber", GetSRS(request.collectionID).Split(":")[1]);
        matchCountCommand.Parameters.AddWithValue("destinationSRS", GetSRS(request.collectionID));


        var matches = matchCountCommand.ExecuteReader().GetInt32(0);

        var commandText =
            "SELECT COUNT(*) FROM ($table)" +
            "WHERE (($table).geom && " +
            "Transform(Extent(GeomFromGPB(($boundingBox))), ($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS)))" +
            "LIMIT ($limit) OFFSET ($offset)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("table", request.collectionID);
        searchCommand.Parameters.AddWithValue("bbox", bboxBytes);
        searchCommand.Parameters.AddWithValue("originSRS", "EPSG:4326");
        searchCommand.Parameters.AddWithValue("destinationSRSnumber", GetSRS(request.collectionID).Split(":")[1]);
        searchCommand.Parameters.AddWithValue("destinationSRS", GetSRS(request.collectionID));
        searchCommand.Parameters.AddWithValue("limit", request.limit);
        searchCommand.Parameters.AddWithValue("offset", request.offset);
        var results = searchCommand.ExecuteReader();


        var features = GetFeatures(results);

        var featureCollection = new ExtendedFeatureCollection()
        {
            Features = features,
            NumberMatched = matches,
            NumberReturned = features.Count,
            Links = new(),
        };

        return featureCollection;
    }

    public ExtendedFeature GetFeature(string table, string id, string spatialReferenceSystem)
    {
        var originSRS = GetSRS(table);

        var commandText =
            "SELECT AsGPB(Transform(" + 
            "GeomFromGPB(geom), ($destinationSRSnumber), GeomFromGPB(($boundingBox)), ($originSRS), ($destinationSRS))" +
            "FROM ($table) WHERE id=($id)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("table", table);
        searchCommand.Parameters.AddWithValue("id", id);

        // The bounding box of the table, this makes the conversion more accurate.
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(GetBoundingBox(table));
        searchCommand.Parameters.AddWithValue("boundingBox", bboxBytes);

        searchCommand.Parameters.AddWithValue("originSRS", originSRS);
        searchCommand.Parameters.AddWithValue("destinationSRSnumber", spatialReferenceSystem.Split(":")[1]);
        searchCommand.Parameters.AddWithValue("destinationSRS", spatialReferenceSystem);

        var reader = searchCommand.ExecuteReader();

        var columns = reader.FieldCount;

        var geomColumn = reader.GetOrdinal("geom");

        var features = GetFeatures(reader);

        return features.First();
    }

    private List<ExtendedFeature> GetFeatures(SqliteDataReader reader)
    {
        var columns = reader.FieldCount;

        var geomColumn = reader.GetOrdinal("geom");

        var features = new List<ExtendedFeature>();
        var geoPackageReader = new GeoPackageGeoReader();

        while (reader.Read())
        {
            var geometryStream = reader.GetStream(geomColumn);

            var properties = new Dictionary<string, object>();

            for (var i = 0; i < columns; i++)
            {
                // We really don't want to add the geometry as a property.
                if (i != geomColumn)
                {
                    properties.Add(reader.GetName(i), reader.GetValue(i));
                }
            }

            features.Add(new()
            {
                ID = (string)properties["id"],
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = properties,
                Links = new(),
            });
        }

        return features;
    }

    private Geometry GetBoundingBox(string table)
    {
        var srs = GetSRS(table);

        var bboxCommandText = 
            "SELECT AsGPB(Transform(Extent(" + 
            "MakeLine(MakePoint(min_x, min_y), MakePoint(max_x, max_y))), " +
            "($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS))) " +
            "FROM gpkg_contents WHERE table_name=($table)";

        var bboxCommand = new SqliteCommand(bboxCommandText, Connection);
        bboxCommand.Parameters.AddWithValue("originSRS", srs);
        bboxCommand.Parameters.AddWithValue("destinationSRSnumber", "4326");
        bboxCommand.Parameters.AddWithValue("destinationSRS", "EPSG:4326");

        var reader = bboxCommand.ExecuteReader();
        var bboxStream = reader.GetStream(0);

        var geoPackageReader = new GeoPackageGeoReader();
        var bbox = geoPackageReader.Read(bboxStream);

        return bbox;
    }

    private string GetSRS(string table)
    {
        var srsCommandText = "SELECT srs_id FROM gpkg_contents WHERE table_name=($table)";
        var srsCommand = new SqliteCommand(srsCommandText, Connection);
        srsCommand.Parameters.AddWithValue("table", table);

        var srsReader = srsCommand.ExecuteReader();
        var srsColumn = srsReader.GetOrdinal("srs_id");
        var srsNumber = srsReader.GetString(srsColumn);


        var orgCommandText = "SELECT organization FROM gpkg_spatial_ref_sys WHERE srs_id=($srs)";
        var orgCommand = new SqliteCommand(orgCommandText, Connection);
        orgCommand.Parameters.AddWithValue("srs", srsNumber);

        var orgReader = orgCommand.ExecuteReader();
        var orgColumn = orgReader.GetOrdinal("organization");
        var orgName = orgReader.GetString(orgColumn);


        return $"{orgName}:{srsNumber}";
    }
}