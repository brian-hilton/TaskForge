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

            app.MapGet("/", () => "Task Forge.");

            // Return user from user table based off passed id in the url
            app.MapGet("/get-user", (int userId) =>
            {
                var repo = new DbRepository(dbConnection);
                var user = repo.GetUser(userId);
                return user;
            });

            // Get all jobs for a user; return list of jobs
            app.MapGet("/user/jobs", (int userId) =>
            {
                var repo = new DbRepository(dbConnection);
                var userJobs = repo.GetUserJobs(userId);
                return userJobs;
            });

            // Get all roles associated with user
            app.MapGet("/user/roles", (int userId) => 
            {
                var repo = new DbRepository(dbConnection);
                var userRoles = repo.GetUserRole(userId);
                return userRoles;
            });

            app.MapGet("/jobs", (int jobId) =>
            {
                var repo = new DbRepository(dbConnection);
                var job = repo.GetJobById(jobId);
                return job;
            });






            // Create job item in job table; return job object
            app.MapPost("/jobs/create-job", (Job job) =>
            {
                var repo = new DbRepository(dbConnection);
                var newJob = repo.CreateJob(job.Name, job.Status, job.Location);
                return newJob;
            });

            // Add a role into role table; return role object
            app.MapPost("/user/add-role", (CreateRoleRequest roleRequest) =>
            {
                var repo = new DbRepository(dbConnection);
                var newRole = repo.CreateUserRole(roleRequest.UserId, roleRequest.RoleId);
                return newRole;
            });

            // Create a user using request model; return user object
            app.MapPost("/create-user", (CreateUserRequest createUserRequest) =>
            {
                var repo = new DbRepository(dbConnection);
                var newUser = repo.CreateUser(createUserRequest.Username, createUserRequest.Password, createUserRequest.Email);
                return newUser;
            });

            // Create job and assign to user id; return job object
            app.MapPost("/user/new-job", (Job job, int userId) =>
            {
                var repo = new DbRepository(dbConnection);
                var newJob = repo.CreateJob(job.Name, job.Status, job.Location);
                repo.AssignJob(newJob.Id, userId);
                return newJob;
            });

            // Assign an existing job to a user; 
            app.MapPost("/jobs", (int jobId, int userId) =>
            {
                var repo = new DbRepository(dbConnection);
                repo.AssignJob(jobId, userId);
                return ($"Assigned job {jobId} to user {userId}");
            });






            app.MapPatch("/jobs/update-job", (UpdateJobRequest jobRequest, int jobId) => 
            {
                var repo = new DbRepository(dbConnection);
                var updatedJob = repo.UpdateJob(jobRequest, jobId);
                return updatedJob;

            });

            app.MapPatch("/users/update-user", (UpdateUserRequest userRequest, int userId) =>
            {
                var repo = new DbRepository(dbConnection);
                var updatedUser = repo.UpdateUser(userRequest, userId);
                return updatedUser;
            });

            app.MapPatch("/jobs/clear-job", (int jobId) =>
            {
                var repo = new DbRepository(dbConnection);
                var clearedJob = repo.ClearJob(jobId);
                return clearedJob;
            });




            // Delete user with their id
            app.MapDelete("/users/delete-user", (int userId) => 
            {
                var repo = new DbRepository(dbConnection);
                repo.DeleteUser(userId);
            });

            // Delete job with jobId
            app.MapDelete("/jobs/delete-job", (int jobId) => 
            {
                var repo = new DbRepository(dbConnection);
                repo.DeleteUser(jobId);
            });
            app.Run();
        }
    }
}
