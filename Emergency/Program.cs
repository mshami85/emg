using Emergency.Classes;
using Emergency.Data;
using Emergency.Hubs;
using Emergency.Models;
using Emergency.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Emergency
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Add services to the container.
            builder.Configuration.AddJsonFile("appsettings.json", true, true).AddJsonFile($"appsettings.{env}.json", true, true);
            builder.Services.Configure<AppSettings>(builder.Configuration);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddIdentity<User, IdentityRole<int>>().AddEntityFrameworkStores<DBContext>().AddDefaultTokenProviders();
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Arabic }));
            builder.Services.AddSingleton<IConnectedUserService, ConnectedUserService>();
            builder.Services.AddCors(options => options.AddPolicy("_allowAll", policy => policy.SetIsOriginAllowed(orig => true)));
            builder.Services.AddAuthentication()
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = true,
                        //ValidAudience = builder.Configuration["JWT:Audience"],
                        //ValidIssuer = builder.Configuration["JWT:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var access_token = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(access_token) && path.HasValue && path.StartsWithSegments("/MessageHub", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = access_token;
                                context.HttpContext.Request.Headers.Authorization = $"{JwtBearerDefaults.AuthenticationScheme} {access_token}";
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(IdentityConstants.ApplicationScheme,
                                                                       JwtBearerDefaults.AuthenticationScheme,
                                                                       CookieAuthenticationDefaults.AuthenticationScheme)
                                            .RequireAuthenticatedUser()
                                            .Build();
            });
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR().AddJsonProtocol();
            builder.Services.AddWebOptimizer(pipe =>
            {
                pipe.AddCssBundle("/css/bootstrap.css", "/lib/bootstrap/dist/css/bootstrap.rtl.css", "/lib/bootstrap/dist/css/bootstrap-icons.css", "/css/PagedList.css");
                pipe.AddCssBundle("/css/leafleft.css", "/lib/leafleft/leaflet.css");
                pipe.AddJavaScriptBundle("/js/jquery.js", "/lib/jquery/dist/jquery.js", "/lib/jquery-validation/dist/jquery.validate.js", "/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js");
                pipe.AddJavaScriptBundle("/js/maps.js", "/lib/leafleft/leaflet.js", "/js/map.js");
                pipe.MinifyCssFiles();
                pipe.MinifyJsFiles();
            }, op =>
            {
                op.EnableCaching = true;
                op.EnableDiskCache = false;
                op.EnableMemoryCache = true;
                op.AllowEmptyBundle = false;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("_allowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<MessageHub>("/MessageHub");
            app.Run();

        }
    }

}