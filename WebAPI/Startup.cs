using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;
using System.Drawing;
using System.Text;
using WebAPI.Mappings;
using WebAPI.Middleware;
using WebAPI.Models.Database;
using WebAPI.Models.Generic;
using WebAPI.Services;


namespace WebAPI
{

    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            configRoot = configuration;
        }
        public IConfiguration configRoot { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            #region Db &App Settings
            var appSettingsSection = configRoot.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            //services.Configure<AppSettings>(configRoot.GetSection("AppSettings"));

            #endregion

            services.AddDbContext<Models.Database.ApplicationContext>((p, o) =>
                o.UseNpgsql(configRoot.GetConnectionString("AppDB"))
            );

            services.AddControllers();

            services.AddMvc(); // Add this line to enable MVC services

            #region Service Registration
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient(typeof(IUploadDocumentService), typeof(UploadDocumentService));
            //services.AddScoped<IUploadDocumentService, UploadDocumentService>();

            services.AddSwaggerGen(c => {
                c.CustomSchemaIds(t => t.ToString());
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
            });

            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configRoot["JWTSettings:ValidAudience"],
                    ValidIssuer = configRoot["JWTSettings:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configRoot.GetSection("JWTSettings:SecretKey").Value)),
                };
            });

            #endregion

            services.AddCors(options => {
                options.AddPolicy("AllowOrigin",
                    builder => {
                        builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                    });
            });

            services.AddRazorPages();

            var mappingConfig = new MapperConfiguration(mc => {
                mc.AddProfile(new MappingProfiles());
            });

            IMapper mapper = mappingConfig.CreateMapper();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "WebAPI v1"));
            }
            app.UseAuthentication();

            app.UseCors("AllowOrigin");

            app.UseRouting();
            
            app.UseMvc();
            app.UseHttpsRedirection();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "Test API V1");
            });
            app.UseDeveloperExceptionPage();
            app.UseMiddleware(typeof(ExceptionHandler));
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}