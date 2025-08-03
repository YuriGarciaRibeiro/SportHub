using Api.Document;
using Api.Extensions;
using Infrastructure.Services;
using Scalar.AspNetCore;
using WebAPI.Extensions;

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
          .AllowAnyOrigin()      // ou .WithOrigins("https://seu-domínio")
          .AllowAnyMethod()      // ou .WithMethods("GET","POST",…)
          .AllowAnyHeader();     // ou .WithHeaders("content-type",…)
    });
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
        .AddCaching();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ExecuteMigrations();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints();
app.UseCors("AllowAll");



using (var scope = app.Services.CreateScope())
{
    var userSeeder = scope.ServiceProvider.GetRequiredService<CustomUserSeeder>();
    await userSeeder.SeedAsync();
    
    var sportSeeder = scope.ServiceProvider.GetRequiredService<SportSeeder>();
    await sportSeeder.SeedAsync();
}

app.Run();
