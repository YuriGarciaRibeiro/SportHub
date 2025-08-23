using Api.Document;
using Api.Extensions;
using Api.Extensions.Results;
using Infrastructure.Persistence.Seeders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();

// Adiciona todos os serviços necessários
builder.AddAllServices();

var app = builder.Build();

// Configura o ResultExtensions
ResultExtensions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAll");

// Configura a aplicação
app.ConfigureApplication();

// Seed all data using the new seeding system
using (var scope = app.Services.CreateScope())
{
    var dataSeederService = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await dataSeederService.SeedAllAsync();
}

app.Run();
