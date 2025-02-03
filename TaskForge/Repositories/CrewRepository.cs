using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge.Repositories
{
    public class CrewRepository : DbRepository
    {
        private string _databaseConnection;

        public CrewRepository(string databaseConnection) : base(databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public Crew CreateCrew(string name)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentTime = DateTime.UtcNow;

            var newId = db.ExecuteScalar<int>(@"
                                                insert into dbo.Crews (name, created_date, updated_date)
                                                output inserted.id
                                                values (@Name, @CurrentTime, @CurrentTime)",
                                                new { Name = name, CurrentTime = currentTime });

            return new Crew { Id = newId, Name = name, CreatedDate = currentTime, UpdatedDate = currentTime };
        }

        public Crew GetCrew(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            var crew = db.QueryFirstOrDefault<Crew>(@"select * from dbo.Crews
                                                      where id = @CrewId", new { CrewId = crewId});
            if (crew == null)
            {
                throw new Exception($"Could not find crew for id: {crewId}");
            }
            return crew;
        }

        public Crew UpdateCrew(UpdateCrewRequest updateCrewRequest, int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            var query = "update Crews set";
            var parameters = new DynamicParameters();
            bool firstField = true;

            if (string.IsNullOrEmpty(updateCrewRequest.Name))
            {
                query += $"{(firstField ? "" : ",")}name = @Name";
                parameters.Add("Name", updateCrewRequest.Name);
                firstField = false;
            }

            if (updateCrewRequest.SupervisorId != 0)
            {
                query += $"{(firstField ? "" : ",")}supervisor_id = @SupervisorId";
                parameters.Add("SupervisorId", updateCrewRequest.SupervisorId);
                firstField = false;
            }

            query += " where Id = @Id";
            parameters.Add("Id", crewId);

            int rowsAffected = db.Execute(query, parameters, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                throw new Exception("Error updating Crew");
            }

            transaction.Commit();

            UpdateDateModified("Crews", crewId);

            var crew = GetCrew(crewId);
            return crew;
        }

        public void DeleteCrew(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute("@delete from dbo.Crews where id = @CrewId", new { CrewId = crewId }, transaction);

            if (rowsAffected != 1)
            {  
                transaction.Rollback(); throw new Exception("Could not delete crew.");
            }

            transaction.Commit();
        }
    }
}
