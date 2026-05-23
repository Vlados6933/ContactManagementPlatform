using ContactsManager.Infrastructure.DbContext;
using ContactsManager.UI.Middleware;
using ContactsManager.UI.StartupExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ContactsManager.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Serilog
            builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider service, LoggerConfiguration loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(service);
            });

            builder.Services.ConfigureSrevise(builder.Configuration);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                    await DbInitializer.SeedDataAsync(context, env);
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred whilst seeding the database");
                }
            }

            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseExceptionHandlingMiddleware();
            }

            if (builder.Environment.IsEnvironment("Test") == false)
            {
                Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
            }

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseHttpLogging();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}");
            });

            app.Run();
        }
    }
}
