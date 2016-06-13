using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CoreDNX.Autofac;
using CoreDNX.Providers;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Web.Middleware;
using Glimpse;

namespace Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddGlimpse();
            services.AddMvc();

            var builder = new ContainerBuilder();
            builder.Populate(services);

            builder.RegisterType<RequestDispatcherService>().As<IRequestDispatcherService>();
            builder.RegisterType<FeatureActionDispatcher>().Keyed<IRequestDispatcher>("/act");
            builder.RegisterType<FeatureDescriptorDispatcher>().Keyed<IRequestDispatcher>("/info");
            builder.RegisterType<RazorPageDispatcher>().Keyed<IRequestDispatcher>("/");

            builder.RegisterModule(new SwitchModule());

            var container = builder.Build();

            var featureProvider = container.Resolve<IFeatureProvider>();
            featureProvider.EnableFeature("test1feature");

            return container.Resolve<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseGlimpse();

            app.UseFeatureMiddleware();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute("spa-fallback",
                                "{*anything}",
                                new { controller = "Home", action = "Index" });

                routes.MapWebApiRoute("defaultApi",
                                      "api/{controller}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
