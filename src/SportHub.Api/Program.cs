using Api.Document;
using Application.Settings;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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
        .AddCustomExecptionHanlder()
        .AddDatabase(builder.Configuration)
        .AddMediatR()
        .AddSettings()
        .AddSeeders()
        .AddSerilogLogging();
        
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ExecuteMigrations();
}

app.UseEndpoints();
app.UseAuthentication();
app.UseAuthorization();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await RoleSeeder.SeedAsync(roleManager, logger);

    var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
    await userSeeder.SeedAdminAsync();
}

app.Run();
