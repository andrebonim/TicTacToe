using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Data;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Model.Extensions;
using TicTacToe.Repository;
using TicTacToe.Util.Extensions;
using TicTacToe.Business.Interfaces;
using TicTacToe.Business;
using TicTacToe.Hubs;
using System.Diagnostics;

namespace TicTacToe.API
{
    public class Startup
    {

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        public void ConfigureDI(IServiceCollection services)
        {

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IGameRepository, GameRepository>();

            services.AddScoped<IPlayerBusiness, PlayerBusiness>();
            services.AddScoped<IGameBusiness, GameBusiness>();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            // === JWT Authentication ===
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!))
                    };
                });


            // === Controllers + Swagger ===
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddDbContext<TicTacToeContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b=> b.MigrationsAssembly("TicTacToe.Data"));
                // Mostra SQL no console (útil em desenvolvimento)
                options.EnableSensitiveDataLogging();          // mostra valores dos parâmetros
                options.EnableDetailedErrors();                // erros mais detalhados

                // Logging completo (escolha um ou mais)
                options.LogTo(Console.WriteLine, LogLevel.Information);  // loga SQL no console
                                                                         // ou
                options.LogTo(message => Debug.WriteLine(message), LogLevel.Information); // Visual Studio Output
            });



            services.AddControllers();


            services.AddDistributedMemoryCache();  // ← cache em memória (bom para dev/teste)

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);     // tempo de inatividade antes de expirar
                options.Cookie.HttpOnly = true;                     // segurança: não acessível via JS
                options.Cookie.IsEssential = true;                  // ignora consentimento GDPR (para funcionar sem banner de cookies)
                                                                    // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // se quiser forçar HTTPS
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhostDev", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowReact", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            services.AddSignalR();


            services.AddAutoMapper(cfg =>
            {
                // Evita mapeamentos automáticos que causam loop 
                cfg.AllowNullCollections = true;                  
                cfg.ShouldMapProperty = p => p.GetGetMethod() != null;  // só props com getter

            }, typeof(MappingProfile));


            services.AddHttpClient();


            ConfigureDI(services);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TicTacToe API", Version = "v1" });

                //Adiciona suporte a JWT no Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;  // ← desabilita o 400 automático
            });
        }

        public void Configure(IApplicationBuilder app
            , IWebHostEnvironment env
            , IMapper mapper
            )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();                
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IVM API v1");
                    c.RoutePrefix = string.Empty;     // ← página do swagger na raiz
                });
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }
            

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowLocalhostDev");
            app.UseCors("AllowReact");

            app.UseAuthentication();  // <--- ANTES do UseAuthorization
            app.UseAuthorization();

            app.UseSession();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/gameHub");
            });

            // CONFIGURAÇÃO OBRIGATÓRIA PARA implicit operator
            AutoMapperExtensions.Configure(mapper);
        }
    }
}
