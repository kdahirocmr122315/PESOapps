using Microsoft.AspNetCore.Components.ProtectedBrowserStorage;
using Microsoft.AspNetCore.StaticFiles;
using PESOapps.Shared.Address;
using PESOapps.Shared.Services;
using PESOapps.Web.Components;
using PESOapps.Web.Services;
using System.Net.Http;
using Tewr.Blazor.FileReader;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AuthService>();
// Register HttpClient
builder.Services.AddScoped<HttpClient>(sp =>
{
    // Get the host environment
    var environment = sp.GetRequiredService<IHostEnvironment>();

    // Here we use a hardcoded base URI (use your app's base URI if needed)
    return new HttpClient { BaseAddress = new Uri("https://portal.misamisoriental.gov.ph/pesobeta/Tupadreg") };  // You can set your base URL here
});
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://portal.misamisoriental.gov.ph/peso2api/") });
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5167/")
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the PESOapps.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddProtectedBrowserStorage();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<UiStateService>();
builder.Services.AddFileReaderService(options => options.InitializeOnFirstCall = true);

// Register AddressService (Ensure it properly loads JSON)
builder.Services.AddScoped<AddressService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new AddressService(httpClient); // Make sure AddressService is using HttpClient to fetch JSON
});

var app = builder.Build();
app.UsePathBase("/pesobeta");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(PESOapps.Shared._Imports).Assembly);

app.Run();
