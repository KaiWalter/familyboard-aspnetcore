using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;

// Configure services
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTokenCache();

builder.Services.AddCookieConfiguration();

builder.Services.AddUIAndApiConfiguration(builder.Configuration);

builder.Services.AddCoreServices();


// Configure and enable middlewares
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";

            var feature = context.Features.Get<IExceptionHandlerFeature>();

            if (feature != null)
            {
                await context.Response.WriteAsync($"<h1>Custom Error Page</h1> {HtmlEncoder.Default.Encode(feature.Error.Message)}");
                await context.Response.WriteAsync($"<hr />{HtmlEncoder.Default.Encode(feature.Error.Source)}");
            }
        });
    });
    app.UseHsts();
}

app.UseStaticFiles();

app.UseCookiePolicy();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();