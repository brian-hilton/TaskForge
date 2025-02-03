using Microsoft.Identity.Client;
using TaskForge.Endpoints;
using TaskForge.Models;

namespace TaskForge
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);
            string dbConnection = builder.Configuration.GetConnectionString("DbConnection")!;
            var app = builder.Build();            

            app.MapUserEndpoints();
            app.MapJobEndpoints();
            //app.MapCrewEndpoints();

            //app.MapGet("/", () => "Task Forge.");

            app.Run();
        }
    }
}
