namespace DotNetGeo.Core;

public interface IGeoCollection
{
    public string ID { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public object Extent { get; init; }
    public List<Link> Links { get; init; }
}
