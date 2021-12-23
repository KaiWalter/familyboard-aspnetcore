using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

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