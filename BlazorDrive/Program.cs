using System.Collections.Immutable;
using BlazorDrive.App.Configuration;
using BlazorDrive.App.Database;
using BlazorDrive.App.Helpers;
using BlazorDrive.App.Repository;
using BlazorDrive.App.Services.Sessions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Logging.Net;

Logger.UseSBLogger();


ConfigHelper configHelper = new();

await configHelper.Perform();

ConfigService configService = new();

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Services
builder.Services.AddSingleton<ConfigService>();

Logger.Info("Successfully initialised the configuration");

// Database

DatabaseCheckup databaseCheckup = new (configService);
await databaseCheckup.Perform();

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped(typeof(Repository<>));

// Identity
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<IdentityService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Services.GetRequiredService<ConfigService>();

app.Run();

