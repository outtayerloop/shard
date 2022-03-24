using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shard.WiemEtBrunelle.Web.Services.Problem;
using Shard.WiemEtBrunelle.Web.Services;
using Shard.WiemEtBrunelle.Web.Services.Users;
using Shard.WiemEtBrunelle.Web.Services.RequestValidators;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Repositories.Users;
using Shard.WiemEtBrunelle.Web.Repositories;
using Shard.WiemEtBrunelle.Web.Repositories.Buildings;
using Shard.WiemEtBrunelle.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using SystemClock = Shard.Shared.Core.SystemClock;
using Shard.WiemEtBrunelle.Web.Database.Services;
using Shard.WiemEtBrunelle.Web.Database.Options;
using Shard.WiemEtBrunelle.Web.Repositories.Universe;

namespace Shard.WiemEtBrunelle.Web
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
            services.AddSingleton<ISectorService, SectorService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IProblemDescriptionService, ProblemDescriptionService>();
            services.AddSingleton<IRequestBodyValidationService, RequestBodyValidationService>();
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IBuildingRepository, BuildingRepository>();
            services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
            services.AddHttpClient();
            services.AddSingleton<MongoDbConnection>();
            services.Configure<MongoDbConnectionOptions>(Configuration.GetSection("MongoDb"));
            services.AddSingleton<ISectorRepository, SectorRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
