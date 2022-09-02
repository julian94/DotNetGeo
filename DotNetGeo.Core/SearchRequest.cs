namespace DotNetGeo.Core;

public class SearchRequest
{
    public string collectionID;
    public BoundingBox bbox;
    public TimeInterval? interval;
    public int limit;
    public int page;

    public Range FetchRange()
    {
        int start = page * limit;
        int end = start + limit - 1;
        return new Range(start, end);
    }
}
