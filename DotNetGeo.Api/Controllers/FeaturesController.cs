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
        return new JsonResult(new LandingPage()
        {
            Title = "My first Geo API.",
            Description = "An example API for example people.",
            Links = new()
            {
                new()
                {
                    HRef = "http://data.example.org/", // Replace with the truth.
                    Rel = "self",
                    Type = "application/json",
                    Title = "this document",
                },
                new()
                {
                    HRef = "http://data.example.org/api", // Replace with the truth.
                    Rel = "service-desc",
                    Type = "application/vnd.oai.openapi+json;version=3.0",
                    Title = "the API definition",
                },
                new()
                {
                    HRef = "http://data.example.org/conformance", // Replace with the truth.
                    Rel = "conformance",
                    Type = "application/json",
                    Title = "OGC API conformance classes implemented by this server",
                },
                new()
                {
                    HRef = "http://data.example.org/collections", // Replace with the truth.
                    Rel = "data",
                    Type = "application/json",
                    Title = "Information about the feature collections",
                },
            }
        });
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
        [FromQuery(Name = "limit")] int? limit,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "datetime")] string? interval
        )
    {
        var request = new SearchRequest
        {
            collectionID = collectionID,
            bbox = BoundingBox.FromString(bbox),
            limit = limit,
            page = page,
            interval = (interval is not null) ? new(interval) : null,
        };

        var result = Central.GetFeatures(request);

        return new JsonResult(result);
    }

    [HttpGet("/collections/{collectionId}/items/{featureId}")]
    public ActionResult GetFeature(
        [FromRoute(Name = "collectionId")] string collectionID,
        [FromRoute(Name = "featureId")] string featureID
        )
    {
        return new JsonResult(Central.GetFeature(collectionID, featureID));
    }
}
