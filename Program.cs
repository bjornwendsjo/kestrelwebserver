using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.AspnetcoreHttpcontext;

// Documentation
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1#host-versus-app-configuration-1
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-3.1
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#welog
// https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
// https://benfoster.io/blog/serilog-best-practices/
// https://kimsereyblog.blogspot.com/2018/02/logging-in-asp-net-core-with-serilog.html
// https://github.com/trenoncourt/serilog-enrichers-aspnetcore-httpcontext
// https://github.com/serilog/serilog-aspnetcore
// https://www.humankode.com/asp-net-core/develop-locally-with-https-self-signed-certificates-and-asp-net-core

namespace KestrelWebServer
{
	public class Program
	{
		static string logPath = @"E:\KestrelWebServer\Logs\log.txt"; /* <----------here---------- */

		static string logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} {RequestScheme} {RemoteIpAddress} {RequestMethod} {Message:lj}{NewLine}{Exception}";

		public static int Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
			// Filter out ASP.NET Core infrastructre logs that are Information and below
			.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
			.Enrich.FromLogContext()
			.WriteTo.Console(
				outputTemplate: logTemplate)
			.WriteTo.File(
				logPath,
				outputTemplate: logTemplate,
				rollingInterval: RollingInterval.Day,
				rollOnFileSizeLimit: true)
			.CreateLogger();

			Log.Warning("At entry point --------------------");
			Log.Warning("Log file path is " + logPath);

			try
			{
				Log.Information("Starting web host");
				CreateHostBuilder(args).Build().Run();
				return 0;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			}
			finally
			{
				Log.Warning("exit");
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseSerilog() // <-- Add this line

				.ConfigureWebHostDefaults(webBuilder =>
				{
					// https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-3.1&tabs=visual-studio#kestrel
					webBuilder.UseKestrel(serverOptions =>
					{
						// Configure the Url and ports to bind to
						// This overrides calls to UseUrls and the ASPNETCORE_URLS environment variable, but will be 
						// overridden if you call UseIisIntegration() and host behind IIS/IIS Express
						serverOptions.Listen(System.Net.IPAddress.Any, 5000);
						serverOptions.Listen(System.Net.IPAddress.Any, 5001, listenOptions =>
						{
							listenOptions.UseHttps("qgis-xyz_wend_se.pfx", "kestreltestcert");
						});
					});

					webBuilder.UseStartup<Startup>();
				});
	}
}
