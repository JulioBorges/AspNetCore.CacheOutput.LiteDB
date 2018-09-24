using AspNetCore.CacheOutput.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.CacheOutput.LiteDB.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();
            
            // Define provider to generate cache database at default location
            services.AddSingleton<IApiOutputCache, LiteDBOutputCacheProvider>();

            // OR Define The path of cache database
            //services.AddSingleton<IApiOutputCache, LiteDBOutputCacheProvider>(provider =>
            //{
            //    return new LiteDBOutputCacheProvider("newFile.db");
            //});

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCacheOutput();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
