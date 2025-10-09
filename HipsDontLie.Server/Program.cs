using HipsDontLie.Database;
using HipsDontLie.Models;
using HipsDontLie.Repository;
using HipsDontLie.Server.Database;
using HipsDontLie.Services;
using HipsDontLie.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HipsDontLie {
    public class Program {
        private static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Database setup
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));
            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddRoles<IdentityRole<int>>()
            .AddDefaultTokenProviders();


            // JWT setup
            // JWT setup
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });


            builder.Services.AddAuthorization();

            // Service and repository setup
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGroupService, GroupService>();
            builder.Services.AddScoped<IChatService, ChatService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddSingleton<WebSocketConnectionManager>();
            builder.Services.AddSingleton<WebSocketEventHandler>();

            // Health Check setup
            builder.Services.AddHealthChecks()
                .AddCheck<HealthCheck>("Database_Health_Check");

            // Swagger setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "GameTogether API",
                    Version = "v1",
                    Description = "API for user authentication and management"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Add CORS policy
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowFrontend", policy => {
                    var env = builder.Environment.EnvironmentName;
                    if (env == "Development") {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else {
                        var frontendUrl = builder.Configuration["FRONTEND_BASE_URL"];
                        if (string.IsNullOrEmpty(frontendUrl)) {
                            throw new Exception("FRONTEND_BASE_URL is not set in configuration.");
                        }

                        // Extract the base domain without protocol and port
                        var uri = new Uri(frontendUrl);
                        var domain = uri.Host;

                        policy.SetIsOriginAllowed(origin => new Uri(origin).Host.EndsWith(domain))
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction()) {
                app.UseSwagger();
                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HipsDontLie API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            using (var scope = app.Services.CreateScope())
            {
               await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            }

            // Enable CORS before authentication
            app.UseCors("AllowFrontend");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.Use(async (context, next) => {
                if (context.Request.Path == "/ws/events")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var handler = context.RequestServices.GetRequiredService<WebSocketEventHandler>();
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await handler.HandleSocketAsync(context, socket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            // Map Controllers
            app.MapControllers();

            // Map fallback to Blazor index.html
            //app.MapFallbackToFile("index.html");

            // Map Health Check Endpoint
            app.MapHealthChecks("/healthz");

            app.Run();
        }
    }
}
