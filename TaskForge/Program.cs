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
            string dbConnection = builder.Configuration.GetConnectionString("DbConnection")!;
            var app = builder.Build(); 
            
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
