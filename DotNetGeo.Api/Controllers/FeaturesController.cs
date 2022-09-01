using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetGeo.Api.Constants;

namespace DotNetGeo.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeaturesController : ControllerBase
{
    [HttpGet("/")]
    public ActionResult GetLandingPage()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    [HttpGet("/collections/{collectionId}/items/{featureId}")]
    public ActionResult GetFeature(
        [FromRoute(Name = "collectionId")] string collectionID,
        [FromRoute(Name = "featureId")] string featureID
        )
    {
        throw new NotImplementedException();
    }
}
