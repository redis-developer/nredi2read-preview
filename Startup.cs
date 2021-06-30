using NRedi2Read.Providers;
using NRedi2Read.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace NRedi2Read
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private const string SecretName = "CacheConnection";

        public const string COOKIE_AUTH_SCHEME = "CookieAuthentication";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            NReJSON.NReJSONSerializer.SerializerProxy = new NewtonsoftSeralizeProxy();
            services.AddControllers();
            services.AddSpaStaticFiles(configuration: options => { options.RootPath = "clientapp/dist"; });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "NRedi2Read-preview", Version = "v1"});
            });

            //Add Redis healthcheck
            services.AddHealthChecks()
                .AddRedis(Configuration[SecretName]);
            
            //services.Configure<Redis>(Configuration);
            services.AddSingleton<RedisProvider>();
            services.AddTransient<BookService>();
            services.AddTransient<CartService>();
            services.AddTransient<UserService>();
            services.AddTransient<BookRatingService>();

            services.AddAuthentication(COOKIE_AUTH_SCHEME)
                .AddCookie(COOKIE_AUTH_SCHEME, options =>{
                    options.Cookie.Name = "redis.Authcookie";
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = redirectContext =>
                        {
                            redirectContext.HttpContext.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }
                    };
                    options.ForwardDefaultSelector = ctx => COOKIE_AUTH_SCHEME;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NRedi2Read-preview v1"));
            }
            app.ApplicationServices.GetService<CartService>().CreateCartIndex();
            app.ApplicationServices.GetService<UserService>().CreateUserIndex();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.Map(new PathString(""), client =>
            {
                var clientPath = Path.Combine(Directory.GetCurrentDirectory(), "clientapp/dist");
                StaticFileOptions clientAppDist = new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(clientPath)
                };
                client.UseSpaStaticFiles(clientAppDist);
                client.UseSpa(spa => spa.Options.DefaultPageStaticFileOptions = clientAppDist);

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            ThreadPool.QueueUserWorkItem(async (state) =>
            {
                await SeedScript.SeedDatabase(
                    app.ApplicationServices.GetService<BookService>(),
                    app.ApplicationServices.GetService<UserService>(),
                    app.ApplicationServices.GetService<CartService>());
            });
        }
    }
}