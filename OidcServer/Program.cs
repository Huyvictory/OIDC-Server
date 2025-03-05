namespace OidcServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        // Show OIDC configuration
        app.MapGet("/.well-known/openid-configuration", () => Results.File(
            Path.Combine(builder.Environment.ContentRootPath, "OIDCDiscovery", "openid-configuration.json"),
            contentType: "application/json"));

        // Show JWKS
        app.MapGet("/.well-known/jwks.json", () => Results.File(
            Path.Combine(builder.Environment.ContentRootPath, "OIDCDiscovery", "jwks.json"),
            contentType: "application/json"));

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}