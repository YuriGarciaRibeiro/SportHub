using Api.Document;
using Api.Extensions;
using Infrastructure.Middleware;
using Infrastructure.Services;
using Scalar.AspNetCore;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.AddAuthentication()
        .AddServices()
        .AddRepositories()
        .AddCustomExceptionHandler()
        .AddDatabase(builder.Configuration)
        .AddMediatR()
        .AddSettings()
        .AddSeeders()
        .AddSerilogLogging()
        .AddCaching()
        .AddCors();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    // Migra apenas o schema public (TenantDbContext)
    // Schemas de tenant são migrados durante o provisioning
    app.ExecuteMigrations();
}

app.UseRouting();
app.UseCors();

// Deve vir ANTES de UseAuthentication para que o schema já esteja definido
// quando o ApplicationDbContext for resolvido na request
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints();

// Seed do SuperAdmin (schema public)
using (var scope = app.Services.CreateScope())
{
    var superAdminSeeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
    await superAdminSeeder.SeedAsync();
}

app.Run();
