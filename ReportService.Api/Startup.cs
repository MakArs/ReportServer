using Domain0.Auth.AspNet;
using Domain0.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


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
            services.AddSingleton<Worker>();
            services.AddControllers();

            services.AddDomain0Auth(new TokenValidationSettings
            {
                Audience = Configuration["TokenValidationSettings:Token_Audience"],
                Issuer = Configuration["TokenValidationSettings:Token_Issuer"],
                Keys = new[]
                {
                    new KeyInfo
                    {
                        Key=Configuration["TokenValidationSettings:Token_Secret"],
                        Alg=Configuration["TokenValidationSettings:Token_Alg"]
                    }
                }
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Worker worker)
        {
            //TODO: worker.StartAsync();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //exceptions
            //Https disabling

            //app.UseHttpsRedirection(); //TODO: client redirection

            app.UseRouting();

            app.UseStatusCodePages("text/plain", "Error. Status code : {0}");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}