using DotNetGeo.Core;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace DotNetGeo.Mock;

public class DummySource : IFeatureSource
{
    public Collection Collection { get; init; }

    private List<ExtendedFeature> Features { get; init; }

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
            },
            Links = new(),
        };

        Features = new()
        {
            new()
            {
                ID = "1",
                Properties = new() 
                {
                    { "ItemType", "Boat" }
                },
                Geometry = new Point(2, 55),
                BoundingBox = new()
                {
                    BottomLeft = new()
                    {
                        X = 2,
                        Y = 55,
                    },
                    TopRight = new()
                    {
                        X = 2,
                        Y = 55,
                    },
                },
                Links = new(),
            },
            new()
            {
                ID = "2",
                Properties = new()
                {
                    { "ItemType", "Plane" }
                },
                Geometry = new Point(3, 56),
                BoundingBox = new()
                {
                    BottomLeft = new()
                    {
                        X = 3,
                        Y = 56,
                    },
                    TopRight = new()
                    {
                        X = 3,
                        Y = 56,
                    },
                },
                Links = new(),
            }
        };

    }

    public ExtendedFeature GetFeature(string id)
    {
        var feature = Features.First(f => f.ID.ToString() == id);

        return feature;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        var features = Features.Where(f => request.bbox.AsEnvelope().Intersects(f.BoundingBox.AsEnvelope()));

        var span = features.Take(request.FetchRange()).ToList();

        var copies = new List<ExtendedFeature>();

        foreach (var feature in features) copies.Add(new()
        {
            ID = feature.ID,
            BoundingBox = feature.BoundingBox,
            Geometry = feature.Geometry,
            Properties = feature.Properties,
            Links = new(),
        });

        var result = new ExtendedFeatureCollection()
        {
            Features = copies,
            NumberMatched = features.Count(),
            NumberReturned = span.Count,
            Links = new(),
        };

        return result;
    }
}
