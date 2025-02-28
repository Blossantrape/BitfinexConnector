using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Указываем, что Razor Pages находятся в папке Components/Pages
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Components/Pages";
});
builder.Services.AddServerSideBlazor();

// Регистрируем ITestConnector с реализацией RestClientService через HttpClient
// Пример регистрации в DI
builder.Services.AddHttpClient<ITestConnector, RestClientService>(client => 
{
    client.BaseAddress = new Uri("https://api-pub.bitfinex.com/v2/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
// Если в _Host.cshtml прописан маршрут @page "/", fallback можно задать как "/_Host"
app.MapFallbackToPage("/_Host");

app.Run();