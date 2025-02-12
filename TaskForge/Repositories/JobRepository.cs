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

        public async Task<Job> CreateJobAsync(string name, string status, string location)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            var newJob = new Job() { Name = name, Status = status, Location = location, CreatedDate = currentDate, UpdatedDate = currentDate };

            var newId = await db.ExecuteScalarAsync<int>(@"
            insert into dbo.Jobs (name, status, location, created_date, updated_date)
            output inserted.id
            values (@Name, @Status, @Location, @CreatedDate, @CreatedDate)", newJob);

            newJob.Id = newId;
            return newJob;
        }

        public async Task<Job> UserCreateJobAsync(int userId, string name, string status, string location)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            // Validate userId
            var user = await db.QueryFirstOrDefaultAsync<User>("select * from users where id = @Id", new { Id = userId });
            if (user == null)
            {
                return null;
            }

            var newJob = new Job() { Name = name, Status = status, Location = location, UserId = userId, CreatedDate = currentDate, UpdatedDate = currentDate };

            var newId = await db.ExecuteScalarAsync<int>(@"
            insert into dbo.Jobs (name, status, location, user_id, created_date, updated_date)
            output inserted.id
            values (@Name, @Status, @Location, @UserId, @CreatedDate, @CreatedDate)", newJob);

            newJob.Id = newId;
            return newJob;
        }
        public async Task<bool> AssignJobAsync(int jobId, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = await db.ExecuteAsync(@"
                        update dbo.Jobs SET user_id = @UserId
                        where id = @JobId",
                        new { JobId = jobId, UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;

        }

        public async Task<List<Job>> GetUserJobsAsync(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var userJobs = await db.QueryAsync<Job>(@" select * from dbo.Jobs where user_id = @UserId", new { UserId = userId });
            if (userJobs == null) { return null; }
            return userJobs.ToList();
        }

        public async Task<Job> GetJobByIdAsync(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var job = await db.QueryFirstOrDefaultAsync<Job>(@"
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
            return job;
        }

        public async Task<Job> ClearJobAsync(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = await db.ExecuteAsync(@"UPDATE Jobs set 
                                            name = '',
                                            status = '',
                                            location = '',
                                            updated_date = @CurrentDate,
                                            due_date = NULL
                                            where id = @JobId", new { CurrentDate = currentDate, JobId = jobId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return null;
            }

            transaction.Commit();
            return await GetJobByIdAsync(jobId);
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

            var job = await GetJobByIdAsync(jobId);

            return job;
        }

        public async Task<bool> DeleteJobAsync(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"delete from dbo.Jobs 
                                            where id = @JobId", new { @JobId = jobId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }
    }
}
