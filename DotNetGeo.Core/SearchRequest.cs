namespace DotNetGeo.Core;

public class SearchRequest
{
    public string collectionID;
    public BoundingBox bbox;
    public TimeInterval? interval;
    public int limit;
    public int offset;

    public Range FetchRange()
    {
        int start = offset;
        int end = offset + limit - 1;
        return new Range(start, end);
    }
}
