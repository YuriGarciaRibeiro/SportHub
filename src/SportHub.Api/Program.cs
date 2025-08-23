using Api.Document;
using Api.Extensions;
using Api.Extensions.Results;
using Infrastructure.Services;
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

// Seed dos dados
using (var scope = app.Services.CreateScope())
{
    var userSeeder = scope.ServiceProvider.GetRequiredService<CustomUserSeeder>();
    await userSeeder.SeedAsync();
    
    var sportSeeder = scope.ServiceProvider.GetRequiredService<SportSeeder>();
    await sportSeeder.SeedAsync();
}

app.Run();
