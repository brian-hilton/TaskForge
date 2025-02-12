using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge.Repositories
{
    public class JobRepository : DbRepository
    {
        private string _databaseConnection;

        public JobRepository(string databaseConnection) : base(databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public Job CreateJob(string name, string status, string location)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            var newJob = new Job() { Name = name, Status = status, Location = location, CreatedDate = currentDate, UpdatedDate = currentDate };

            var newId = db.ExecuteScalar<int>(@"
            insert into dbo.Jobs (name, status, location, created_date, updated_date)
            output inserted.id
            values (@Name, @Status, @Location, @CreatedDate, @CreatedDate)", newJob);

            newJob.Id = newId;
            return newJob;

        }

        public void AssignJob(int jobId, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = db.Execute(@"
                        update dbo.Jobs SET user_id = @UserId
                        where id = @JobId",
                        new { JobId = jobId, UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                throw new Exception("Invalid job ID");
            }

            transaction.Commit();

        }

        public List<Job> GetUserJobs(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var userJobs = db.Query<Job>(@" select * from dbo.Jobs where id = @UserId", new { UserId = userId }).ToList();

            return userJobs;
        }

        public Job GetJobById(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var job = db.QueryFirstOrDefault<Job>(@"
                                                    SELECT 
                                                        id AS Id,
                                                        name AS Name,
                                                        status AS Status,
                                                        location AS Location,
                                                        user_id AS UserId,
                                                        created_date AS CreatedDate,
                                                        updated_date AS UpdatedDate,
                                                        due_date AS DueDate
                                                    FROM dbo.Jobs
                                                    WHERE id = @JobId", new { JobId = jobId });

            if (job == null)
            {
                throw new Exception($"Job with ID {jobId} could not be found");
            }

            return job;
        }

        public Job ClearJob(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = db.Execute(@"UPDATE Jobs set 
                                            name = '',
                                            status = '',
                                            location = '',
                                            updated_date = @CurrentDate,
                                            due_date = NULL
                                            where id = @JobId", new { CurrentDate = currentDate, JobId = jobId }, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback(); throw new Exception("Could not clear job");
            }

            transaction.Commit();
            return GetJobById(jobId);
        }
        public async Task<Job> UpdateJobAsync(UpdateJobRequest updatedJob, int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            // Begin query and initialize empty parameters object
            var query = "update Jobs set ";
            var parameters = new DynamicParameters();
            bool firstField = true;

            // Check for each field and append to query & parameters
            if (!string.IsNullOrEmpty(updatedJob.Name))
            {
                query += $"{(firstField ? "" : ", ")}name = @Name";
                parameters.Add("Name", updatedJob.Name);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedJob.Location))
            {
                query += $"{(firstField ? "" : ", ")}location = @Location";
                parameters.Add("Location", updatedJob.Location);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedJob.Status))
            {
                query += $"{(firstField ? "" : ", ")}status = @Status";
                parameters.Add("Status", updatedJob.Status);
                firstField = false;
            }

            if (updatedJob.DueDate.HasValue)
            {
                query += $"{(firstField ? "" : ", ")}due_date = @DueDate";
                parameters.Add("DueDate", updatedJob.DueDate);
                firstField = false;
            }

            // If we did not find any fields in the passed job request object
            if (firstField)
            {
                //throw new Exception("Update attempted with empty job request");
            }

            // Append WHERE clause to query
            query += " where Id = @Id";
            parameters.Add("Id", jobId);

            int rowsAffected = await db.ExecuteAsync(query, parameters, transaction);

            await transaction.CommitAsync();

            await UpdateDateModifiedAsync("Jobs", jobId);

            var job = GetJobById(jobId);

            return job;
        }

        public void DeleteJob(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute(@"delete from dbo.Jobs 
                                            where id = @JobId", new { @JobId = jobId }, transaction);

            if (rowsAffected != 0)
            {
                transaction.Rollback();
                throw new Exception($"Could not delete job with id {jobId}");
            }

            transaction.Commit();
        }
    }
}
