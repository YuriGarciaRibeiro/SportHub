using Scalar.AspNetCore;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.AddAuthentication()
        .AddServices()
        .AddRepositories()
        .AddCustomExecptionHanlder()
        .AddDatabase(builder.Configuration)
        .AddMediatR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ExecuteMigrations();
}

app.UseEndpoints();




app.Run();
