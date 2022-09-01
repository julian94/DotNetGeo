namespace DotNetGeo.Api.Constants;

public static class Conformances
{
    public static class V1
    {
        /// <summary>The Core specifies requirements that all Web APIs have to implement.</summary>
        /// <remarks>The Core does not mandate a specific encoding or format for representing features or feature collections.</remarks>
        public const string Core = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/core";

        /// <summary>Supports OpenAPI Specification 3.0</summary>
        /// <remarks></remarks>
        public const string OpenApi30 = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/oas30";

        /// <summary>Can return data in html format</summary>
        /// <remarks>Every 200-response of an operation of the server SHALL support the media type text/html.</remarks>
        public const string Html = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/html";

        /// <summary>Can return data in geojson format.</summary>
        /// <remarks>200-responses of the server SHALL support the following media types:
        /// application/geo+json for resources that include feature content, and
        /// application/json for all other resources.</remarks>
        public const string GeoJson = "http://www.opengis.net/spec/ogcapi-features-1/1.0/conf/geojson";
    }
}
