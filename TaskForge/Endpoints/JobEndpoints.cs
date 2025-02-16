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
            app.MapGet("/user/jobs", async (int userId) =>
            {
                var repo = new JobRepository(dbConnection);
                var userJobs = await repo.GetUserJobsAsync(userId);

                if (userJobs == null)
                {
                    return Results.NotFound($"No jobs found for user with ID: {userId}");
                }

                return Results.Ok(userJobs);
            });

            // Get a job by its id
            app.MapGet("/jobs", async (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var job = await repo.GetJobByIdAsync(jobId);

                if (job == null)
                {
                    return Results.NotFound($"No jobs found with ID: {jobId}");
                }
                return Results.Ok(job);
            });

            // Get all jobs
            app.MapGet("/jobs/all-jobs", async () =>
            {
                var repo = new JobRepository(dbConnection);
                var jobs = await repo.GetAllJobsAsync();

                if (jobs == null)
                {
                    return Results.NotFound("No jobs found.");
                }

                return Results.Ok(jobs);
            });

            // Create job item in job table; return job object
            app.MapPost("/jobs/create-job", async (Job job) =>
            {
                var repo = new JobRepository(dbConnection);
                var newJob = await repo.CreateJobAsync(job.Name, job.Status, job.Location);

                if (newJob == null)
                {
                    return Results.BadRequest("Failed to create job.");
                }
                return Results.Created($"/jobs?jobId={newJob.Id}", newJob);
            });

            // Create job with user id; return job object
            app.MapPost("/user/create-job", async (UserJobRequest userJobRequest) =>
            {
                var repo = new JobRepository(dbConnection);
                var newJob = await repo.UserCreateJobAsync(userJobRequest.UserId, userJobRequest.Name, userJobRequest.Status, userJobRequest.Location);

                if (newJob == null)
                {
                    return Results.BadRequest("Failed to create job.");
                }

                return Results.Created($"/jobs?jobId={newJob.Id}", newJob);
            });

            // Assign an existing job to a user; 
            app.MapPost("/jobs/assign", async (int jobId, int userId) =>
            {
                var repo = new JobRepository(dbConnection);
                bool assigned = await repo.AssignJobAsync(jobId, userId);

                if (!assigned)
                {
                    return Results.NotFound("Failed to assign job to user.");
                }

                return Results.Ok();
                
            });

            app.MapPatch("/jobs/update-job", async (UpdateJobRequest jobRequest, int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var updatedJob = await repo.UpdateJobAsync(jobRequest, jobId);

                if (updatedJob == null)
                { 
                    return Results.BadRequest($"Failed to update job with ID: {jobId}"); 
                }

                return Results.Created($"jobs?jobId={jobId}" ,updatedJob);

            });

            app.MapPatch("/jobs/clear-job", async (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                var clearedJob = await repo.ClearJobAsync(jobId);

                if (clearedJob == null)
                {
                    return Results.NotFound($"Failed to find job with ID: {jobId}");
                }
                return Results.Ok(clearedJob);
            });

            // Delete job with jobId
            app.MapDelete("/jobs/delete-job", async (int jobId) =>
            {
                var repo = new JobRepository(dbConnection);
                bool deleted = await repo.DeleteJobAsync(jobId);
                if(!deleted)
                {
                    return Results.NotFound($"Failed to delete job with ID: {jobId}");
                }

                return Results.Ok();
            });
        }
    }
}
