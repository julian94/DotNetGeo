using DotNetGeo.Core;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace DotNetGeo.Mock;

public class DummySource : IFeatureSource
{
    public Collection Collection { get; init; }

    private List<Feature> Features { get; init; }

    public DummySource()
    {
        Collection = new()
        {
            ID = "DummySource",
            Title = "A collection of hardcoded features.",
            Description = "This collection is used both as a reference and to have a simple and testable source.",
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
                            X = 0,
                            Y = 54,
                        },
                        TopRight = new()
                        {
                            X = 5,
                            Y = 57,
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

        Features = new()
        {
            new()
            {
                Attributes = new AttributesTable
                {
                    { "ID", "1" }
                },
                Geometry = new Point(2, 55),
                BoundingBox = new Envelope(new Coordinate(2, 55))
            },
            new()
            {
                Attributes = new AttributesTable
                {
                    { "ID", "2" }
                },
                Geometry = new Point(3, 56),
                BoundingBox = new(new Coordinate(3, 56))
            }
        };

    }

    public IFeature GetFeature(string id)
    {
        var feature = Features.First(f => f.Attributes["ID"].ToString() == id);

        return feature;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        var features = Features.Where(f => request.bbox.AsEnvelope().Intersects(f.BoundingBox));

        var span = features.Take(request.FetchRange()).ToList();

        var result = new ExtendedFeatureCollection()
        {
            Features = span,
            NumberMatched = features.Count(),
            NumberReturned = span.Count,
        };

        return result;
    }
}
