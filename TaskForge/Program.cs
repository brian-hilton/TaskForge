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
                options.IdleTimeout = TimeSpan.FromMinutes(30); 
                options.Cookie.HttpOnly = true; 
                options.Cookie.IsEssential = true; 
            });

            builder.Services.AddAuthentication().AddCookie();
            builder.Services.AddAuthorization(); 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy.WithOrigins("http://localhost:5173")
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

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
            app.MapUserEndpoints();
            app.MapJobEndpoints();
            app.MapCrewEndpoints();
            app.MapCrewMemberEndpoints();

            //app.MapGet("/", () => "Task Forge.");

            app.Run();
        }
    }
}
