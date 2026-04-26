using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Veritas.Corpora;
using Veritas.DomainPacks;
using Veritas.Rag.Contracts;
using Veritas.Rag.Implementation;
using Veritas.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// [MOCK] JWT Bearer authentication stub.
// Real implementation: configure Azure Active Directory (AAD) as identity provider.
// Set Veritas:AadTenantId and Veritas:AadClientId in appsettings or Key Vault.
// See infrastructure/main.bicep for the AAD app registration placeholders.
// NuGet: Microsoft.AspNetCore.Authentication.JwtBearer (already in Microsoft.AspNetCore.App)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // [MOCK] Replace with real AAD tenant + audience before production
        options.Authority = builder.Configuration["Veritas:AadAuthority"]
            ?? "https://login.microsoftonline.com/[MOCK-TENANT-ID]";
        options.Audience = builder.Configuration["Veritas:AadClientId"]
            ?? "[MOCK-CLIENT-ID]";
        // In development, skip token validation so the API works without AAD
        if (builder.Environment.IsDevelopment())
            options.TokenValidationParameters = new() { ValidateAudience = false, ValidateIssuer = false };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VeritasWeb", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Register Veritas services
// [MOCK] IDocumentStore uses in-memory DataLakeDocumentStore.
// Replace with a real Azure Data Lake Gen2 implementation before production.
builder.Services.AddSingleton<IDocumentStore, DataLakeDocumentStore>();
builder.Services.AddSingleton<CorpusService>();
builder.Services.AddSingleton<DocumentIngestionService>();
builder.Services.AddSingleton<TextExtractionService>();
builder.Services.AddSingleton<IndexingService>();
builder.Services.AddSingleton<IDomainPackRuntime, DomainPackRuntime>();

// RAG pipeline — [MOCK] LenrCorpusConnector uses placeholder corpus ID.
// Replace with corpus ID resolved from the authenticated user's active corpus.
// See rag-plugin-contract.skill for the full contract.
builder.Services.AddSingleton<ICorpusConnector>(_ =>
    LenrCorpusConnector.CreateMock("default-corpus"));
builder.Services.AddSingleton<IIndexBackend, MockIndexBackend>();
builder.Services.AddSingleton<IRetriever, RagRetriever>();
builder.Services.AddSingleton<IAnswerGenerator, GroundedAnswerGenerator>();
builder.Services.AddSingleton<ICitationValidator, CitationValidator>();
builder.Services.AddSingleton<IRagPlugin, RagPipeline>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Veritas API";
        options.Theme = ScalarTheme.Purple;
    });
}

app.UseHttpsRedirection();
app.UseCors("VeritasWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
