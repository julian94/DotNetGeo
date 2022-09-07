using DotNetGeo.Core;
using DotNetGeo.GeoJsonSource;
using DotNetGeo.GeoPackage;
using DotNetGeo.Mock;

var builder = WebApplication.CreateBuilder(args);

var featureSources = new List<IFeatureSource>()
{
    //new GeoJsonSource("./Data/Example.geo.json"),
    new DummySource(),
};
featureSources.AddRange(
    await GeoPackageProvider.GetSourcesFromDB(
        @"C:\Users\Julian\Downloads\opmplc_gpkg_gb\data\opmplc_gb.gpkg"));

var dataCentral = new DataCentral(featureSources);
builder.Services.AddSingleton(dataCentral);

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("Features", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DotNetGeo Feature Server",
        Description = "A simple server for hosting simple features using the OGC Feature API.",
        Contact = new()
        {
            Email = "langlo94@gmail.com",
            Name = "Julian Silden Langlo",
            Url = new("https://github.com/julian94/DotNetGeo"),
        },
        Version = "v1",
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/Features/swagger.json", "OGC Features API");
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
