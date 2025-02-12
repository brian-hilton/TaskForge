using System.Data.Common;
using System.Runtime.CompilerServices;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Endpoints
{
    public static class JobEndpoints
    {
        public static void MapJobEndpoints(this WebApplication app)
        {
            string dbConnection = app.Configuration.GetConnectionString("DbConnection")!;

            // Get all jobs for a user; return list of jobs
            app.MapGet("/user/jobs", (int userId) =>
            {
                var repo = new JobRepository(dbConnection);
                var userJobs = repo.GetUserJobs(userId);
                return userJobs;
            });

            app.MapGet("/jobs", (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var job = repo.GetJobById(jobId);
                return job;
            });

            // Create job item in job table; return job object
            app.MapPost("/jobs/create-job", (Job job) =>
            {
                var repo = new JobRepository(dbConnection);
                var newJob = repo.CreateJob(job.Name, job.Status, job.Location);
                return newJob;
            });

            // Create job and assign to user id; return job object
            app.MapPost("/user/new-job", (Job job, int userId) =>
            {
                var repo = new JobRepository(dbConnection);
                var newJob = repo.CreateJob(job.Name, job.Status, job.Location);
                repo.AssignJob(newJob.Id, userId);
                return newJob;
            });

            // Assign an existing job to a user; 
            app.MapPost("/jobs", (int jobId, int userId) =>
            {
                var repo = new JobRepository(dbConnection);
                repo.AssignJob(jobId, userId);
                return ($"Assigned job {jobId} to user {userId}");
            });

            app.MapPatch("/jobs/update-job", async (UpdateJobRequest jobRequest, int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var updatedJob = await repo.UpdateJobAsync(jobRequest, jobId);
                return updatedJob;

            });

            app.MapPatch("/jobs/clear-job", (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var clearedJob = repo.ClearJob(jobId);
                return clearedJob;
            });

            // Delete job with jobId
            app.MapDelete("/jobs/delete-job", (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                repo.DeleteJob(jobId);
            });
        }
    }
}
