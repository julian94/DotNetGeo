namespace DotNetGeo.Core;

public class TemporalExtent
{
    [JsonPropertyName("interval")]
    public List<TemporalInterval> Interval { get; set; }
}

[JsonConverter(typeof(TemporalIntervalJsonConverter))]
public class TemporalInterval
{
    public TimeInterval? Interval { get; set; }

    // Default is: http://www.opengis.net/def/uom/ISO-8601/0/Gregorian
    public string? TemporalReferenceSystem { get; set; }
}


[JsonConverter(typeof(TimeIntervalJsonConverter))]
public class TimeInterval
{
    private readonly string dateformat = "yyyy-MM-ddTHH:mm:ssZ";
    private readonly CultureInfo culture = CultureInfo.InvariantCulture;
    private readonly string boundedMarker = "..";

    private DateTime Parse(string data) =>
        DateTime.ParseExact(data, dateformat, culture);

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public TimeInterval(string data)
    {
        if (data is null || !data.Contains('/'))
        {
            var time = Parse(data);
            Start = time;
            End = time;
            return;
        }

        var parts = data.Split('/');

        if (boundedMarker.Equals(parts[0]))
        {
            Start = DateTime.MinValue;
        }
        else
        {
            Start = Parse(parts[0]);
        }

        if (boundedMarker.Equals(parts[0]))
        {
            End = DateTime.MaxValue;
        }
        else
        {
            End = Parse(parts[0]);
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        if (Start.Equals(End))
        {
            stringBuilder.Append(Start.ToString(dateformat, culture));
        }
        else
        {
            if (Start.Equals(DateTime.MinValue))
            {
                stringBuilder.Append(boundedMarker);
            }
            else
            {
                stringBuilder.Append(Start.ToString(dateformat, culture));
            }
            if (End.Equals(DateTime.MaxValue))
            {
                stringBuilder.Append(boundedMarker);
            }
            else
            {
                stringBuilder.Append(End.ToString(dateformat, culture));
            }
        }
        return stringBuilder.ToString();
    }
}

public class TemporalIntervalJsonConverter : JsonConverter<TemporalInterval>
{
    public override TemporalInterval Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var values = new List<string>();

        while (!reader.IsFinalBlock)
        {
            values.Add(reader.GetString());
        }

        if (values.Count == 2)
        {
            return new TemporalInterval
            {
                Interval = new TimeInterval(values[0]),
                TemporalReferenceSystem = values[1],
            };
        }
        else
        {
            throw new InvalidDataException($"Wrong amount of values in temporal extent. Must have 2 values, but had: {values.Count}.");
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        TemporalInterval temporal,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(temporal.Interval.ToString());
        writer.WriteStringValue(temporal.TemporalReferenceSystem);
        writer.WriteEndArray();
    }
}

public class TimeIntervalJsonConverter : JsonConverter<TimeInterval>
{
    private readonly string dateformat = "yyyy-MM-ddTHH:mm:ssZ";
    private readonly CultureInfo culture = CultureInfo.InvariantCulture;
    private readonly string boundedMarker = "..";

    private DateTime Parse(string data) =>
        DateTime.ParseExact(data, dateformat, culture);

    public override TimeInterval Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var data = reader.GetString();
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }
        return new TimeInterval(data);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeInterval interval,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(interval.ToString());
    }
}
