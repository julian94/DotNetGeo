using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetGeo.Api.Constants;
using DotNetGeo.Api.Structures;
using DotNetGeo.Core;

namespace DotNetGeo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeaturesController : ControllerBase
{
    private DataCentral Central { get; init; }

    public FeaturesController(DataCentral central)
    {
        Central = central;
    }

    [HttpGet("/")]
    public ActionResult GetLandingPage()
    {
        var requestAddress = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        var landingPage = new LandingPage()
        {
            Title = "My first Geo API.",
            Description = "An example API for example people.",
            Links = new()
            {
                new()
                {
                    HRef = $"{requestAddress}/",
                    Rel = "self",
                    Type = "application/json",
                    Title = "this document",
                },
                new()
                {
                    HRef = $"{requestAddress}/api",
                    Rel = "service-desc",
                    Type = "application/vnd.oai.openapi+json;version=3.0",
                    Title = "the API definition",
                },
                new()
                {
                    HRef = $"{requestAddress}/conformance",
                    Rel = "conformance",
                    Type = "application/json",
                    Title = "OGC API conformance classes implemented by this server",
                },
                new()
                {
                    HRef = $"{requestAddress}/collections",
                    Rel = "data",
                    Type = "application/json",
                    Title = "Information about the feature collections",
                },
            }
        };
        return new JsonResult(landingPage);
    }

    [HttpGet("/api")]
    public ActionResult GetApiDefinition()
    {
        // Return the OpenAPI Specification.
        throw new NotImplementedException();
    }

    [HttpGet("/conformance")]
    public ActionResult GetConformance()
    {
        return new JsonResult(new List<string>() 
        {
            Conformances.V1.Core,
            Conformances.V1.OpenApi30,
            Conformances.V1.GeoJson,
        });
    }

    [HttpGet("/collections")]
    public ActionResult GetCollections()
    {
        throw new NotImplementedException();
    }

    [HttpGet("/collections/{collectionId}")]
    public ActionResult GetCollection(
        [FromRoute(Name = "collectionId")] string collectionID)
    {
        throw new NotImplementedException();
    }

    [HttpGet("/collections/{collectionId}/items")]
    public ActionResult GetFeatures(
        [FromRoute(Name = "collectionId")] string collectionID,
        [FromQuery(Name = "bbox")] string bbox,
        [FromQuery(Name = "datetime")] string? interval,
        [FromQuery(Name = "limit")] int limit = 10,
        [FromQuery(Name = "offset")] int offset = 0
        )
    {
        var request = new SearchRequest
        {
            collectionID = collectionID,
            bbox = BoundingBox.FromString(bbox),
            interval = (interval is not null) ? new(interval) : null,
            limit = limit,
            offset = offset,
        };

        var result = Central.GetFeatures(request);

        var requestAddress = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        result.PopulateLinks(requestAddress, collectionID, request);

        return new JsonResult(result);
    }

    [HttpGet("/collections/{collectionId}/items/{featureId}")]
    public ActionResult GetFeature(
        [FromRoute(Name = "collectionId")] string collectionID,
        [FromRoute(Name = "featureId")] string featureID
        )
    {
        var feature = Central.GetFeature(collectionID, featureID);


        var requestAddress = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        feature.PopulateLinks(requestAddress, collectionID, false);

        return new JsonResult(feature);
    }
}
