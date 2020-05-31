using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControl.Models;
using FlightControlWeb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlightControl
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new Exception("Problem with null configuration");
            }

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IFlightManager, FlightManager>();
            services.AddSingleton<IFlightPlanManager, FlightPlanManager>();
            services.AddSingleton<IServersManager, ServersManager>();
            services.AddSingleton<IDataBase, SqliteDataBase>();



        }

        /* public void ConfigureServicesDataBase(IDataBase db)
         {
             services.AddSingleton<IDataBase, db>();
         }*/
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}