using DevIO.API.Configuration;
using DevIO.API.Extensions;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevIO.API
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
            services.AddDbContext<MeuDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentityConfiguration(Configuration);

            services.AddControllers()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            //Dependency Injection - AutoMapper
            services.AddAutoMapper(typeof(Startup));

            //Criado em Configuration/ApiConfig.cs
            services.WebApiConfig();

            //Dependency Injection
            services.ResolveDependencies();

            //Movido para SwaggerConfig.cs
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "APICompleta", Version = "v1" });
            //});

            //Swagger LoogerConfig.cs
            services.AddSwaggerConfig();

            //Logger elmah.io - LoogerConfig.cs
            services.AddLoggingConfiguration();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider /*para nova config no swagger*/)
        {
            if (env.IsDevelopment())
            {
                app.UseCors("Development");

                app.UseDeveloperExceptionPage();

                //Movido para SwaggerConfig.cs
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }
            else
            {
                app.UseCors("Production");
                app.UseHsts();
            }

            //Authentication Identity
            app.UseAuthentication();

            //Middleware criada para pegar os erros sem tratamento - config para enviar pro elmah.io
            app.UseMiddleware<ExceptionMiddleware>();

            //Criado em Configuration/ApiConfig.cs
            app.UseMvcConfiguration();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //Swagger SwaggerConfig.cs
            app.UseSwaggerConfig(provider);

            //Logger elmah.io - LoogerConfig.cs
            app.UseLoggingConfiguration();
        }
    }
}
