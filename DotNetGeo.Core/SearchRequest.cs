namespace DotNetGeo.Core;

public class SearchRequest
{
    string collectionID;
    string bbox;
    TimeInterval? interval;
    int? limit;
    int? page;
}
