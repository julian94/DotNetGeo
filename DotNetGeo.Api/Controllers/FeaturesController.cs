using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        throw new NotImplementedException();
    }
    [HttpGet("/conformance")]
    public ActionResult GetConformance()
    {
        throw new NotImplementedException();
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
        [FromQuery(Name = "limit")] int? limit,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "bbox")] string bbox,
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
