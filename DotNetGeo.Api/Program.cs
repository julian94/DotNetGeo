using DotNetGeo.Core;

var builder = WebApplication.CreateBuilder(args);

var dataCentral = new DataCentral(new List<IFeatureSource>()
{
    new DotNetGeo.GeoJsonSource.GeoJsonSource("./Data/Example.geo.json"),
});
builder.Services.AddSingleton(dataCentral);

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
