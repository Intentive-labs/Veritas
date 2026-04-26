using Veritas.Corpora;
using Veritas.DomainPacks;
using Veritas.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Veritas services
// [MOCK] IDocumentStore uses in-memory DataLakeDocumentStore.
// Replace with a real Azure Data Lake Gen2 implementation before production.
builder.Services.AddSingleton<IDocumentStore, DataLakeDocumentStore>();
builder.Services.AddSingleton<CorpusService>();
builder.Services.AddSingleton<DocumentIngestionService>();
builder.Services.AddSingleton<TextExtractionService>();
builder.Services.AddSingleton<IndexingService>();
builder.Services.AddSingleton<IDomainPackRuntime, DomainPackRuntime>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
