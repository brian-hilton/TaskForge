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

        public async Task<CrewMember> GetCrewMemberByUserIdAsync(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            var crewMember = await db.QueryFirstOrDefaultAsync<CrewMember>(@"select * from dbo.CrewMembers
                                                 where user_id = @UserId", new { UserId = userId });
            return crewMember;
        }
        public async Task<CrewMember> CreateCrewMemberAsync(int crewId, int userId, string role)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            using var transaction = await db.BeginTransactionAsync();

            try
            {
                // Validate userId
                var user = await db.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM dbo.users WHERE id = @Id", new { Id = userId }, transaction);
                if (user == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // Validate role
                var thisRole = await db.QueryFirstOrDefaultAsync<Role>(
                    "SELECT * FROM dbo.roles WHERE name = @RoleName", new { RoleName = role }, transaction);
                if (thisRole == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // Validate crew
                var crew = await db.QueryFirstOrDefaultAsync<Crew>(
                    "SELECT * FROM dbo.crews WHERE id = @Id", new { Id = crewId }, transaction);
                if (crew == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                // Insert new crew member
                int rowsAffected = await db.ExecuteAsync(@"
                                                        INSERT INTO dbo.CrewMembers (crew_id, user_id, role)
                                                        VALUES (@CrewId, @UserId, @Role)",
                    new { CrewId = crewId, UserId = userId, Role = role }, transaction);

                if (rowsAffected != 1)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await transaction.CommitAsync();
                return new CrewMember { CrewId = crewId, UserId = userId, Role = role };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error inserting crew member: {ex.Message}");
                return null;
            }
        }

        public async Task<CrewMember> UpdateCrewMemberRoleAsync(int userId, int roleId)
        {
            var role = await GetRoleByIdAsync(roleId);
            if (role == null)
            {
                return null;
            }
            var roleName = role.Name;

            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"update CrewMembers 
                                            set role = @RoleName
                                            where user_id = @UserId", new { RoleName = roleName, UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return null;
            }

            await transaction.CommitAsync();
            return await GetCrewMemberByUserIdAsync(userId);

        }

        // Get all crew members for a crew
        public async Task<List<CrewMember>?> GetAllCrewMembersByCrewAsync(int crewId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var crewMembers = await db.QueryAsync<CrewMember>(@"select * from dbo.CrewMembers where crew_id = @CrewId", new { CrewId = crewId });
            if (crewMembers == null) { return null; }

            return crewMembers.ToList();

        }

        // Return list of all crew_id's a user is part of
        public async Task<List<string>?> GetAllCrewsByUserAsync(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            var crewIdList = await db.QueryAsync<string>(@"select crew_id from dbo.CrewMembers where user_id = @UserId", new { UserId = userId });

            if (crewIdList == null || crewIdList?.Count() < 1)
            {
                return null;
            }

            return (List<string>?)crewIdList;
        }

        // Delete user from specific crew
        public async Task<bool> DeleteCrewMemberAsync(int crewId, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"delete from dbo.CrewMembers
                                            where crew_id = @CrewId and user_id = @UserId", new { CrewId = crewId, UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }

        // Remove user from any crews they are in
        public async Task<bool> DeleteAllUserCrewMembershipsAsync(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"delete from dbo.CrewMembers
                                            where user_id = @UserId", new { UserId = userId }, transaction);

            if (rowsAffected < 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }
    }
}