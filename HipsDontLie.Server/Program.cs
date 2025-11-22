using HipsDontLie.Client;
using HipsDontLie.Database;
using HipsDontLie.Models;
using HipsDontLie.Repository;
using HipsDontLie.Server.Database;
using HipsDontLie.Server.Repository;
using HipsDontLie.Server.Settings;
using HipsDontLie.Services;
using HipsDontLie.WebSockets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;

namespace HipsDontLie {
    public class Program {
        private static async Task Main(string[] args) {
            
            var builder = WebApplication.CreateBuilder(args);
            if(builder.Configuration == null) {
                throw new Exception("Configuration does not exist. Check your appsettings.json file.");
            }
            
            // Data access setup
            ConfigureDataAccess(builder);
            ConfigureSecurity(builder);
            ConfigureApplicationServices(builder);
            ConfigureHealthChecks(builder);
            ConfigureSwagger(builder);
            ConfigureCors(builder);
            builder.Services.AddControllers();

            var app = builder.Build();
            
            UseSwaggerUI(app);
            ConfigureWebSockets(app);
            MigrateDatabase(app);
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/healthz");

            app.Run();
        }

        // --- Helpers ---

        private static async void MigrateDatabase(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            }
        }

        private static void ConfigureDataAccess(WebApplicationBuilder builder) {
            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));

            if (builder.Configuration.GetSection("MongoChat") == null) {
                throw new Exception("MongoChat section is missing in configuration.");
            }

            // Bind Mongo chat settings
            builder.Services.Configure<MongoChatSettings>(
                builder.Configuration.GetSection("MongoChat"));

            // Register IMongoClient as singleton
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoChatSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });
            builder.Services.AddMemoryCache();


        }

        private static void ConfigureSecurity(WebApplicationBuilder builder) {
            // Identity
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

            // JWT
            if (builder.Configuration.GetSection("JwtSettings") == null) {
                throw new Exception("JwtSettings section is missing in configuration.");
            }
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
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;

                options.SaveTokens = true;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.ClaimActions.MapJsonKey("email_verified", "email_verified");
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
                options.ClaimActions.MapJsonKey("urn:google:locale", "locale");
                options.ClaimActions.MapJsonKey("urn:google:profile", "profile");
            });

            builder.Services.ConfigureExternalCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });


            builder.Services.AddAuthorization();
        }

        private static void ConfigureApplicationServices(WebApplicationBuilder builder) {
            // Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGroupService, GroupService>();
            builder.Services.AddScoped<IChatService, ChatService>();

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            //builder.Services.AddScoped<IChatRepository, ChatRepository>(); // sql based chat repository using ef core
            builder.Services.AddScoped<IChatRepository, MongoChatRepository>(); // mongo based chat repository

            // WebSocket helpers
            builder.Services.AddSingleton<WebSocketConnectionManager>();
            builder.Services.AddSingleton<WebSocketEventHandler>();
        }

        private static void ConfigureHealthChecks(WebApplicationBuilder builder) {
            builder.Services.AddHealthChecks()
                .AddCheck<HealthCheck>("Database_Health_Check");
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder) {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "HipsDontLie API",
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
        }

        private static void ConfigureCors(WebApplicationBuilder builder) {
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
                    Console.WriteLine($"starting with env: {env}");
                });
            });
        }

        private static void UseSwaggerUI(WebApplication app) {
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging() && !app.Environment.IsProduction()) {
                app.UseSwagger();
                app.UseSwaggerUI(options => {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HipsDontLie API v1");
                    options.RoutePrefix = "swagger";
                });
            }
        }

        private static void ConfigureWebSockets(WebApplication app) {
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
        }
    }
}
