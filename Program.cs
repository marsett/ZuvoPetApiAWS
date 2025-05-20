using Microsoft.EntityFrameworkCore;
using ZuvoPetApiAWS.Repositories;
using ZuvoPetApiAWS.Data;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using ZuvoPetApiAWS.Helpers;
using Azure.Storage.Blobs;
using ZuvoPetApiAWS.Services;
using Microsoft.Extensions.Azure;
using Azure.Security.KeyVault.Secrets;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetRequiredService<SecretClient>();

//KeyVaultSecret secretConnectionString = await secretClient.GetSecretAsync("SqlZuvoPet");
string secretConnectionString = builder.Configuration.GetConnectionString("MySql");
KeyVaultSecret secretStorageAccount = await secretClient.GetSecretAsync("StorageAccount");

KeyVaultSecret secretAudience = await secretClient.GetSecretAsync("Audience");
KeyVaultSecret secretIssuer = await secretClient.GetSecretAsync("Issuer");
KeyVaultSecret secretIterate = await secretClient.GetSecretAsync("Iterate");
KeyVaultSecret secretKey = await secretClient.GetSecretAsync("Key");
KeyVaultSecret secretSalt = await secretClient.GetSecretAsync("Salt");
KeyVaultSecret secretSecretKey = await secretClient.GetSecretAsync("SecretKey");

HelperCriptography.Initialize(
    secretSalt.Value,
    secretIterate.Value,
    secretKey.Value
);
builder.Services.AddHttpContextAccessor();

HelperActionServicesOAuth helper = new HelperActionServicesOAuth(
    secretIssuer.Value,
    secretAudience.Value,
    secretSecretKey.Value
);
builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
builder.Services.AddSingleton<ServiceStorageBlobs>();
builder.Services.AddScoped<HelperUsuarioToken>();
builder.Services.AddAuthentication(helper.GetAuthenticateSchema())
    .AddJwtBearer(helper.GetJwtBearerOptions());
// Add services to the container.

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<ServiceStorageS3>();

string azureKeys = secretStorageAccount.Value;
BlobServiceClient blobServiceClient = new BlobServiceClient(azureKeys);
builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);

string connectionString =
    secretConnectionString;
builder.Services.AddTransient<IRepositoryZuvoPet, RepositoryZuvoPet>();
builder.Services.AddDbContext<ZuvoPetContext>
    (options => options.UseMySQL(connectionString));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}
app.MapOpenApi();
app.UseHttpsRedirection();

app.MapScalarApiReference();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context => {
    context.Response.Redirect("/scalar/v1");
    return Task.CompletedTask;
});

app.Run();