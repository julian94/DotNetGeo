using DotNetGeo.Core;
using System.Text.Json.Serialization;

namespace DotNetGeo.Api.Structures;

public class LandingPage
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("links")]
    public List<Link>? Links { get; set; }
}