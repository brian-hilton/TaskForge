using Microsoft.Identity.Client;
using TaskForge.Endpoints;
using TaskForge.Models;

namespace TaskForge
{
    
    public class Program
    {
        public static void Main(string[] args)
        {

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOpenApi();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); 
                options.Cookie.HttpOnly = true; 
                options.Cookie.IsEssential = true; 
            });

            var MyAllowSpecificOrigins = "AllowFrontend";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    policy => policy.WithOrigins("http://localhost:5173", "http://192.168.68.110:5173")
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials()); 
            });
            builder.Services.AddAuthentication().AddCookie();
            builder.Services.AddAuthorization(); 
            

            string dbConnection = builder.Configuration.GetConnectionString("DbConnection")!;
            var app = builder.Build();

            app.UseCors("AllowFrontend");
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"An internal server error occurred.\"}");
                });
            });
            app.UseSession();
            app.UseAuthorization();
            app.MapOpenApi();
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");
                await next();
            });
            app.MapUserEndpoints();
            app.MapJobEndpoints();
            app.MapCrewEndpoints();
            app.MapCrewMemberEndpoints();

            app.Run();
        }
    }
}
