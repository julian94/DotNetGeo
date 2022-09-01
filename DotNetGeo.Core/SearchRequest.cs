namespace DotNetGeo.Core;

public class SearchRequest
{
    public string collectionID;
    public BoundingBox bbox;
    public TimeInterval? interval;
    public int? limit;
    public int? page;
}
