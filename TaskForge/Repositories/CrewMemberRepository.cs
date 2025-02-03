using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge.Repositories
{
    public class CrewMemberRepository : DbRepository
    {
        private string _databaseConnection;

        public CrewMemberRepository(string databaseConnection) : base(databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public CrewMember CreateCrewMember(int crewId, int userId, string role)
        {
            using var db = new SqlConnection(_databaseConnection);

            db.ExecuteScalar(@"insert into dbo.CrewMembers (crew_id, user_id, role)
                               values (@CrewId, @UserId, @Role)", new { CrewId = crewId, UserId = userId, Role = role });

            return new CrewMember { CrewId = crewId, UserId = userId, Role = role };
        }

        // Get all crew members for a crew
        public List<CrewMember>? GetAllCrewMembersByCrew(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var crewMembers = db.Query<CrewMember>(@"select * from dbo.CrewMembers where crew_id = @CrewId", new { CrewId = crewId }).ToList();

            return crewMembers;

        }

        // Return list of all crew_id's a user is part of
        public List<String>? GetAllCrewsByUser(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            var crewIdList = db.Query<String>(@"select crew_id from dbo.CrewMembers where user_id = @UserId", new { UserId = userId });

            if (crewIdList == null || crewIdList?.Count() < 1)
            {
                throw new Exception("User is not part of any crews");
            }

            return (List<string>?)crewIdList;
        }

        // Delete user from specific crew
        public void DeleteCrewMember(int crewId, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute(@"delete from dbo.CrewMembers
                                            where crew_id = @CrewId and user_id = @UserId", new { CrewId = crewId, UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                throw new Exception("Could not delete crew member.");
            }

            transaction.Commit();
        }

        // Remove user from any crews they are in
        public void DeleteAllUserCrewMemberships(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute(@"delete from dbo.CrewMembers
                                            where user_id = @UserId", new { UserId = userId }, transaction);

            if (rowsAffected < 1)
            {
                transaction.Rollback();
                throw new Exception("Could not delete user from all crews.");
            }

            transaction.Commit();
        }
    }
}