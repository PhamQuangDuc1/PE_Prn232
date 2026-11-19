var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Doctor"));
app.Run();