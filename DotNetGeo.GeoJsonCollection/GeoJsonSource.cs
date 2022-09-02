using DotNetGeo.Core;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace DotNetGeo.GeoJsonSource;

public class GeoJsonSource : IFeatureSource
{
    public Collection Collection { get; init; }

    private IList<ExtendedFeature> Features { get; init; }

    public GeoJsonSource(string file)
    {
        Collection = new()
        {
            ID = "Example.geo.json",
            Title = "Title",
            Description = "Description",
            Extent = new()
            {
                Spatial = new()
                {
                    BoundingBox = new()
                {
                    new()
                    {
                        BottomLeft = new()
                        {
                            X = -180,
                            Y = -90,
                        },
                        TopRight = new()
                        {
                            X = 180,
                            Y = -90,
                        }
                    }
                }
                },
                Temporal = new()
                {
                    Interval = new()
                {
                    new()
                    {
                        Interval = new("2022-09-01T00:00:00Z"),
                        TemporalReferenceSystem = "http://www.opengis.net/def/uom/ISO-8601/0/Gregorian",
                    }
                }
                }
            }
        };

        var raw = File.ReadAllText(file);
        var data = JsonSerializer.Deserialize<ExtendedFeatureCollection>(raw);

        Features = data.Features;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        var features = Features.Where(f => (
            true//f.BoundingBox.Intersects(new Envelope(request.bbox.BottomLeft, request.bbox.TopRight))
        )).ToList();

        var collection = new ExtendedFeatureCollection()
        {
            Features = Features,
            NumberMatched = features.Count,
            NumberReturned = features.Count,
        };

        return collection;
    }

    public ExtendedFeature GetFeature(string id)
    {
        var feature = Features.Single(f => f.ID == id);
        if (feature is not null)
        {
            return feature;
        }
        else
        {
            return null;
        }
    }
}
