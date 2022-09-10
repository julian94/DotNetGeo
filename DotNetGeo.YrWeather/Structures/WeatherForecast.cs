using System.Text.Json.Serialization;

namespace DotNetGeo.Yr.Structures;

public class WeatherForecast
{
    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    [JsonPropertyName("update")]
    public DateTimeOffset Update { get; set; }

    [JsonPropertyName("dayIntervals")]
    public List<DayInterval> DayIntervals { get; set; }

    [JsonPropertyName("longIntervals")]
    public List<LongInterval> LongIntervals { get; set; }

    [JsonPropertyName("shortIntervals")]
    public List<ShortInterval> ShortIntervals { get; set; }
}

public class DayInterval
{
    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("end")]
    public DateTimeOffset End { get; set; }

    // These strings are enums.
    [JsonPropertyName("twentyFourHourSymbol")]
    public string TwentyFourHourSymbol { get; set; }

    [JsonPropertyName("twelveHourSymbols")]
    public List<string> TwelveHourSymbols { get; set; }

    [JsonPropertyName("sixHourSymbols")]
    public List<string> SixHourSymbols { get; set; }

    [JsonPropertyName("symbolConfidence")]
    public string SymbolConfidence { get; set; }

    [JsonPropertyName("precipitation")]
    public Precipitation Precipitation { get; set; }

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; }
}

public class LongInterval
{
    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("end")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("nominalStart")]
    public DateTimeOffset NominalStart { get; set; }

    [JsonPropertyName("nominalEnd")]
    public DateTimeOffset NominalEnd { get; set; }

    [JsonPropertyName("symbol")]
    public Symbol Symbol { get; set; }

    [JsonPropertyName("symbolCode")]
    public SymbolCode SymbolCode { get; set; }

    [JsonPropertyName("symbolConfidence")]
    public string SymbolConfidence { get; set; }

    [JsonPropertyName("precipitation")]
    public DetailedPrecipitation Precipitation { get; set; }

    [JsonPropertyName("temperature")]
    public FullyDetailedTemperature Temperature { get; set; }

    [JsonPropertyName("wind")]
    public DetailedWind Wind { get; set; }

    [JsonPropertyName("feelsLike")]
    public OneValue FeelsLike { get; set; }

    [JsonPropertyName("pressure")]
    public OneValue Pressure { get; set; }

    [JsonPropertyName("cloudCover")]
    public CloudCover CloudCover { get; set; }

    [JsonPropertyName("humidity")]
    public OneValue Humidity { get; set; }

    [JsonPropertyName("dewPoint")]
    public OneValue DewPoint { get; set; }
}

public class ShortInterval
{
    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("end")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("symbol")]
    public Symbol Symbol { get; set; }

    [JsonPropertyName("symbolCode")]
    public SymbolCode SymbolCode { get; set; }

    [JsonPropertyName("symbolConfidence")]
    public string SymbolConfidence { get; set; }

    [JsonPropertyName("precipitation")]
    public DetailedPrecipitation Precipitation { get; set; }

    [JsonPropertyName("temperature")]
    public DetailedTemperature Temperature { get; set; }

    [JsonPropertyName("wind")]
    public DetailedWind Wind { get; set; }

    [JsonPropertyName("feelsLike")]
    public OneValue FeelsLike { get; set; }

    [JsonPropertyName("pressure")]
    public OneValue Pressure { get; set; }

    [JsonPropertyName("uvIndex")]
    public OneValue UltraVioletIndex { get; set; }

    [JsonPropertyName("cloudCover")]
    public CloudCover CloudCover { get; set; }

    [JsonPropertyName("humidity")]
    public OneValue Humidity { get; set; }

    [JsonPropertyName("dewPoint")]
    public OneValue DewPoint { get; set; }
}

public class Symbol
{
    [JsonPropertyName("sunup")]
    public bool SunIsUp { get; set; }

    [JsonPropertyName("n")]
    public int N { get; set; } // What does N mean?

    [JsonPropertyName("clouds")]
    public int Clouds { get; set; }

    [JsonPropertyName("precip")]
    public int Precipitation { get; set; }

    [JsonPropertyName("var")]
    public string Var { get; set; } // Sun and Moon are possibilities at least.
}

public class SymbolCode
{
    [JsonPropertyName("next1Hour")]
    public string NextHour { get; set; }

    [JsonPropertyName("next6Hours")]
    public string NextSixHours { get; set; }

    [JsonPropertyName("next12Hours")]
    public string NextTwelveHours { get; set; }
}

public class Precipitation
{
    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("probability")]
    public int Probability { get; set; }
}

public class DetailedPrecipitation
{
    [JsonPropertyName("min")]
    public double Minimum { get; set; }

    [JsonPropertyName("max")]
    public double Maximum { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("pop")]
    public int Pop { get; set; } // The heck is Pop?

    [JsonPropertyName("probability")]
    public int Probability { get; set; }
}

public class DetailedProbability
{
    [JsonPropertyName("tenPercentile")]
    public double TenPercentile { get; set; }

    [JsonPropertyName("ninetyPercentile")]
    public double NinetyPercintile { get; set; }
}

public class OneValue
{
    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class Temperature
{
    [JsonPropertyName("min")]
    public double Minimum { get; set; }

    [JsonPropertyName("max")]
    public double Maximum { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class DetailedTemperature
{
    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("probability")]
    public DetailedProbability Probability { get; set; }
}

public class FullyDetailedTemperature
{
    [JsonPropertyName("min")]
    public double Minimum { get; set; }

    [JsonPropertyName("max")]
    public double Maximum { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("probability")]
    public DetailedProbability Probability { get; set; }
}

public class Wind
{
    [JsonPropertyName("min")]
    public double Minimum { get; set; }

    [JsonPropertyName("max")]
    public double Maximum { get; set; }

    [JsonPropertyName("maxGust")]
    public double MaximumGust { get; set; }
}

public class DetailedWind
{
    [JsonPropertyName("direction")]
    public int Direction { get; set; }

    [JsonPropertyName("gust")]
    public double Gust { get; set; }

    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("probability")]
    public DetailedProbability Probability { get; set; }
}

public class CloudCover
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("low")]
    public int Low { get; set; }

    [JsonPropertyName("middle")]
    public int Middle { get; set; }

    [JsonPropertyName("high")]
    public int High { get; set; }

    [JsonPropertyName("fog")]
    public int Fog { get; set; }
}
