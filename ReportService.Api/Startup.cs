using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ReportService.Api
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
            services.AddHostedService<Worker>();
            services.AddControllers();
            services.AddSingleton<Worker>();        
        }

        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Worker worker)
        {
            //worker.StartAsync();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //port
            //exceptions
            //Https disabling

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStatusCodePages("text/plain", "Error. Status code : {0}");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var x = 2;



            app.Run(async (context) =>
            {
                x = x * 2;
                await context.Response.WriteAsync($"Result: {x}");

            });

        }
    }
}
