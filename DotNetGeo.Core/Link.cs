﻿namespace DotNetGeo.Core;
public class Link
{
    /// <summary>
    /// Supplies the URI to a remote resource (or resource fragment).
    /// </summary>
    [JsonPropertyName("href")]
    public string? HRef;

    /// <summary>
    /// The type or semantics of the relation.
    /// </summary>
    [JsonPropertyName("rel")]
    public string? Rel;

    /// <summary>
    /// A hint indicating what the media type of the result of dereferencing the link should be.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type;

    /// <summary>
    /// Used to label the destination of a link such that it can be used as a human-readable identifier.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title;
}
