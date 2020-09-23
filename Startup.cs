using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace SaraReportAPI {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        readonly string AllowSpecificOrigins = "AllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            // Get 'GlobalData' data from 'appsettings.json' and store it in a ingleton class
            Configuration.GetSection("GlobalData").Bind(GlobalData.Current);

            services.AddDbContext<Models.Entities.SaraReportDBContext>(options =>
                 options.UseSqlServer(Configuration.GetConnectionString(GlobalData.Current.UseConnection))
            );
            services.AddControllers()
                    .AddNewtonsoftJson(options =>
                        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
                    );

            // Configure the JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new TokenValidationParameters {
                            NameClaimType = "NoEmployee",
                            RoleClaimType = "Roles",
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = GlobalData.Current.Jwt["Issuer"],
                            ValidAudience = GlobalData.Current.Jwt["Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(GlobalData.Current.Jwt["Key"]))
                        };
                    });

            // Enable CORS
            services.AddCors(options => {
                options.AddPolicy(AllowSpecificOrigins,
                builder => {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseCors(AllowSpecificOrigins);
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
