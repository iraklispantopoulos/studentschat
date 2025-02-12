using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Serilog;
using Tcp;
using WebApiGateway.Configuration;
using Shared;
using Repository;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using WebApiGateway.Helpers;
using Microsoft.Extensions.Logging;

namespace WebApiGateway
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var envVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? builder.Configuration["EnvironmentName"]
                  ?? "Development";
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            builder.Environment.EnvironmentName = environment;
            builder.Services.AddHttpContextAccessor();
            var key = Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"]!); // Replace with a secure key
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "chatbotforstudents", // Replace with your issuer
                        ValidAudience = "chatbotforstudents", // Replace with your audience
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("DevelopmentIgnore", policy =>
                    policy.RequireAssertion(context => context.User.Identity?.IsAuthenticated == true ||
                                                       builder.Environment.IsDevelopment()));
            });
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), p => p.MigrationsAssembly("Repository"));
            }, ServiceLifetime.Singleton);

            builder.Services.AddDistributedMemoryCache(); // In-memory cache for session storage
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(70); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Host.UseSerilog((context, configuration) =>
                                configuration.ReadFrom.Configuration(context.Configuration));
            builder.Services.AddScoped<Client>();
            builder.Services.AddScoped<IChatGPTClient>(p => new ChatGptClient.ChatGPTClient(builder.Configuration["ChatGPTKey"]!, "gpt-4o"));
            builder.Services.AddScoped<WebApiGateway.Session.User>();
            builder.Services.AddScoped<Helpers.ChatFactory>();
            builder.Services.AddScoped<SpeechGeneratorFactory>();            
            builder.Services.AddScoped<WindowsNarratorSpeechGenerator>();
            builder.Services.AddScoped<GoogleTextToSpeech.SpeechGenerator>(p =>
            {
                var logger=p.GetService<ILogger<GoogleTextToSpeech.SpeechGenerator>>();
                var user = p.GetService<WebApiGateway.Session.User>();
                var outputDir = builder.Configuration["AudioOutputDir"];
                //get the dir of the executable
                var jsonKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "google-default-account-key.json");
                return new GoogleTextToSpeech.SpeechGenerator(logger!, outputDir!, jsonKeyPath!, user?.GetAvatarGenderType().Result!);
            });                
            builder.Services.AddSingleton<Shared.PromptConfiguration.Configuration>(p =>
            {
                var jsonString = File.ReadAllText("Content/chat-bot-for-students-sample-everyday-configuration.json");
                return JsonConvert.DeserializeObject<Shared.PromptConfiguration.Configuration>(jsonString);
            });
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
            builder.Services.AddScoped<IChatHistoryRepository,ChatHistoryRepository>();
            builder.Services.AddScoped<IPromptUnitRepository, PromptUnitRepository>();
            builder.Services.AddSingleton(x => new ServerConfiguration(builder.Configuration["SpeechServer:Ip"], int.Parse(builder.Configuration["SpeechServer:Port"]!)));
            builder.Services.AddSingleton(x => new AppConfig()
            {
                LipSyncOutputDir = builder.Configuration["LipSyncOutputDir"]!
            });


            var app = builder.Build();
            ServiceProviderWrapper.Initialize(app.Services);
            var isProd=app.Environment.IsProduction() ? "yes" : "no";
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"EnvVar:{envVar}.{app.Environment.EnvironmentName}. Is production:{isProd}");
            app.UseSwagger();
            app.UseSwaggerUI();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                logger.LogInformation("development mode");
                app.UseCors(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            }
            else
            {
                logger.LogInformation("Production mode");
                app.UseCors(policy =>
                        policy.SetIsOriginAllowed(origin =>
                        {
                            var allowed = origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost") || origin.Contains("https://students-chat.org");                            
                            if (!allowed)
                            {
                                logger.LogInformation($"Origin not allowed:{origin}");
                            }                            
                            return (allowed);
                        })                        
                        .AllowAnyHeader()
                        .AllowAnyMethod());


            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllers();
            // Apply pending migrations automatically
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();  // Creates DB if missing
                dbContext.Database.Migrate();//auto migrates
            }
            app.Run();
        }
    }
}
