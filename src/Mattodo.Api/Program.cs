using FluentValidation;
using Mattodo.Api;
using Mattodo.Api.Auth;
using Mattodo.Api.Data;
using Mattodo.Api.Endpoints.Internal;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
//    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
//builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddEndpoints<Program>(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseEndpoints<Program>();
//app.UseAuthorization();

app.MapGet("/", () => "Hello World!")
.WithTags("ForTheLulz");

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
