using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KestrelWebServer.Data;
using Serilog;
using Microsoft.Extensions.FileProviders;
using Serilog.Events;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace KestrelWebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            Log.Warning("Configure: Current dir is " + System.IO.Directory.GetCurrentDirectory());

            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-3.1&tabs=visual-studio
            app.UseHttpsRedirection();

            // https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
            // https://github.com/serilog/serilog-aspnetcore
            //app.UseSerilogRequestLogging();
            app.UseSerilogRequestLogging(options =>
            {
                // Customize the message template
                options.MessageTemplate = "{RequestPath} {StatusCode} {Elapsed}";

                // Emit debug-level events instead of the defaults
                options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });

            // ...or maybe before previous row
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-3.1
            app.UseStaticFiles();

            string rootDir = @"E:\SimpleWebServer\root";
            app.UseStaticFiles(new StaticFileOptions
            { FileProvider = new PhysicalFileProvider(rootDir), RequestPath = "" });
            Log.Information("Read static files in " + rootDir);

            List<string> staticDirs = System.IO.File.ReadLines(rootDir + @"\" + "allowedStaticDirectories.txt").ToList();

			foreach (string dir in staticDirs)
			{
                if(dir.IndexOf(':')<0)  // Not an absolute path (like E:\mydir)
                {
                    app.UseStaticFiles(new StaticFileOptions
                    { FileProvider = new PhysicalFileProvider(rootDir + @"\" + dir), RequestPath = "/" + dir });
                    Log.Information("Read static files in " + rootDir + @"\" + dir);
                }
                else // It is an absolute path. 
				{
                    var dirs = dir.Split(@"\");
                    app.UseStaticFiles(new StaticFileOptions
                    { FileProvider = new PhysicalFileProvider(dir), RequestPath = "/" + dirs.Last() });
                    Log.Information("Read static files in " + dir);
                }
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
