using PESOapps.Shared.Services;
using PESOapps.Web.Components;
using PESOapps.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5167/api/Tupad/getall") });
builder.Services.AddScoped<ApiService>();


// Add device-specific services used by the PESOapps.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();
app.UsePathBase("/pesobeta");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(PESOapps.Shared._Imports).Assembly);

app.Run();
