using Microsoft.Identity.Client;
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

            app.MapGet("/", () => "Hello World!");

            app.MapPost("/jobs", (Job job) =>
            {
                var repo = new DbRepository(dbConnection);
                var newJob = repo.CreateJob(job.Name, job.Status, job.Location);
                return newJob;
            });

            app.MapPost("/user/add-role", (CreateRoleRequest roleRequest) =>
            {
                var repo = new DbRepository(dbConnection);
                var newRole = repo.CreateUserRole(roleRequest.UserId, roleRequest.RoleId);
                return newRole;
            });



            
            app.Run();
        }
    }
}
