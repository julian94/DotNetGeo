using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace DotNetGeo.Core;

public class SpatialExtent
{
    [JsonPropertyName("bbox")]
    public List<BoundingBox> BoundingBox { get; set; }
}

[JsonConverter(typeof(BoundingBoxJsonConverter))]
public class BoundingBox
{
    public Coordinate BottomLeft { get; set; }
    public Coordinate TopRight { get; set; }

    public List<double> ToList()
    {
        var list = new List<double>();
        list.Add(BottomLeft.X);
        list.Add(BottomLeft.Y);
        if (BottomLeft.Z != null && !double.IsNaN(BottomLeft.Z)) list.Add(BottomLeft.Z);
        list.Add(TopRight.X);
        list.Add(TopRight.Y);
        if (TopRight.Z != null   && !double.IsNaN(TopRight.Z))   list.Add(BottomLeft.Z);
        return list;
    }

    public List<Coordinate> ToFourPosition()
    {
        return new List<Coordinate>
        {
            BottomLeft,
            TopRight,
            new Coordinate(TopRight.X, BottomLeft.Y),
            new Coordinate(BottomLeft.X, TopRight.Y),
        };
    }

    public Envelope AsEnvelope()
    {
        return new(BottomLeft, TopRight);
    }

    public Polygon AsPolygon()
    {
        return new(new LinearRing(new Coordinate[2] { BottomLeft, TopRight }));
    }

    public static BoundingBox FromString(string data)
    {
        var values = new List<double>();

        var parts = data.Split(',');

        foreach (var part in parts)
        {
            if (double.TryParse(
                part,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value))
            {
                values.Add(value);
            }
        }

        return FromDoubles(values);
    }
    public static BoundingBox FromDoubles(IList<double> values)
    {
        if (values.Count == 4)
        {
            return new BoundingBox
            {
                BottomLeft = new Coordinate()
                {
                    X = values[0],
                    Y = values[1],
                },
                TopRight = new Coordinate()
                {
                    X = values[2],
                    Y = values[3],
                },
            };
        }
        else if (values.Count == 6)
        {
            return new BoundingBox
            {
                BottomLeft = new Coordinate() 
                { 
                    X = values[0], 
                    Y = values[1], 
                    Z =values[2] 
                },
                TopRight = new Coordinate()
                {
                    X = values[3],
                    Y = values[4],
                    Z = values[5]
                },
            };
        }
        else
        {
            throw new InvalidDataException($"Wrong amount of values in bounding box. Must have 4 or 6 values, but had: {values.Count}.");
        }
    }

    public bool Contains(BoundingBox box)
    {
        var positions = box.ToFourPosition();
        foreach (var position in positions)
        {
            if (Contains(position)) return true;
        }
        return false;
    }
    public bool Contains(Coordinate p)
    {
        if (
            BottomLeft.X < p.X &&
            TopRight.X > p.X &&
            BottomLeft.Y < p.Y &&
            TopRight.Y > p.Y
        ) return true;
        else return false;
    }

    public override string ToString()
    {
        return $"{BottomLeft.X},{BottomLeft.Y},{TopRight.X},{TopRight.Y}";
    }
}
public class BoundingBoxJsonConverter : JsonConverter<BoundingBox>
{
    public override BoundingBox Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var values = new List<double>();

        while (!reader.IsFinalBlock)
        {
            if (reader.TryGetDouble(out var value)) values.Add(value);
        }

        return BoundingBox.FromDoubles(values);
    }

    public override void Write(
        Utf8JsonWriter writer,
        BoundingBox bbox,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var value in bbox.ToList())
        {
            writer.WriteNumberValue(value);
        }
        writer.WriteEndArray();
    }
}