using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using Tests.Helpers;

namespace Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddOpenTelemetryTracing(options => options
            .AddSource(ActivityHelper.SourceName)
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = filter =>
                {
                    bool framework = filter.Request.Path.Value.Contains("_framework", StringComparison.OrdinalIgnoreCase);
                    bool swagger = filter.Request.Path.Value.Contains("swagger", StringComparison.OrdinalIgnoreCase);
                    bool result = framework | swagger;
                    return !result; // hide accessing to swagger or refresh page
                };
                options.Enrich = (activity, eventName, rawObject) =>
                {
                    activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;
                    activity.AddTag("AspNet.Enviroment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                };
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_COLLECTOR_ENDPOINT"));
                options.Protocol = OtlpExportProtocol.Grpc;
            })
            .SetResourceBuilder(
                ResourceBuilder
                .CreateDefault()
                .AddService(Environment.GetEnvironmentVariable("APPLICATIONNAME"))));

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
