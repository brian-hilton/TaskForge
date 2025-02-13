using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge.Repositories
{
    public class CrewRepository : DbRepository
    {
        private readonly string _databaseConnection;

        public CrewRepository(string databaseConnection) : base(databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }
        
        public async Task<Crew?> CreateCrewAsync(string name)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();

            var currentTime = DateTime.UtcNow;

            var newId = await db.ExecuteScalarAsync<int>(@"
            insert into dbo.Crews (name, created_date, updated_date)
            output inserted.id
            values (@Name, @CurrentTime, @CurrentTime)",
                new { Name = name, CurrentTime = currentTime });

            return newId > 0 ? new Crew { Id = newId, Name = name, CreatedDate = currentTime, UpdatedDate = currentTime } : null;
        }

        public async Task<Crew?> GetCrewAsync(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();

            return await db.QueryFirstOrDefaultAsync<Crew>(@"
            select * from dbo.Crews where id = @CrewId",
                new { CrewId = crewId });
        }

        public async Task<List<Crew>> GetAllCrewsAsync()
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();

            var crews = (await db.QueryAsync<Crew>("select * from dbo.Crews")).ToList();
            return crews;
        }

        public async Task<Crew?> UpdateCrewAsync(UpdateCrewRequest updateCrewRequest, int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            var query = "update Crews set ";
            var parameters = new DynamicParameters();
            bool firstField = true;

            if (!string.IsNullOrEmpty(updateCrewRequest.Name))
            {
                query += $"{(firstField ? "" : ", ")}name = @Name";
                parameters.Add("Name", updateCrewRequest.Name);
                firstField = false;
            }

            if (updateCrewRequest.SupervisorId != 0)
            {
                query += $"{(firstField ? "" : ", ")}supervisor_id = @SupervisorId";
                parameters.Add("SupervisorId", updateCrewRequest.SupervisorId);
                firstField = false;
            }

            query += " where Id = @Id";
            parameters.Add("Id", crewId);

            int rowsAffected = await db.ExecuteAsync(query, parameters, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return null;
            }

            await transaction.CommitAsync();

            await UpdateDateModifiedAsync("Crews", crewId);

            return await GetCrewAsync(crewId);
        }

        public async Task<bool> DeleteCrewAsync(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"
            delete from dbo.Crews where id = @CrewId",
                new { CrewId = crewId }, transaction);

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
