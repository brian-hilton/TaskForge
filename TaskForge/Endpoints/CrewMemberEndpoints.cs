using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Endpoints
{
    public static class CrewMemberEndpoints
    {
        public static void MapCrewMemberEndpoints(this WebApplication app)
        {
            string dbConnection = app.Configuration.GetConnectionString("DbConnection")!;

            app.MapGet("crews/get-all-members", async (int crewId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var crewMembers = await repo.GetAllCrewMembersByCrewAsync(crewId);
                if (crewMembers == null) 
                {
                    return Results.NotFound($"Crewmembers for crew ID {crewId} not found.");
                }
                return Results.Ok(crewMembers);
            });

            app.MapGet("crews/get-member", async (int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var crewMember = await repo.GetCrewMemberByUserIdAsync(userId);

                if (crewMember == null)
                {
                    return Results.NotFound($"Crewmember with User ID: {userId} not found.");
                }
                return Results.Ok(crewMember);
            });

            app.MapGet("crews/get-all-crews-for-user", async (int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var crewList = await repo.GetAllCrewsByUserAsync(userId);
                if (crewList == null)
                {
                    return Results.NotFound($"Crews for user {userId} not found.");
                }
                return Results.Ok(crewList);
            });

            app.MapPost("crews/add-member", async (int crewId, int userId, string role) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var newCrewMember = await repo.CreateCrewMemberAsync(crewId, userId, role);
                if (newCrewMember == null)
                {
                    return Results.BadRequest($"Failed to create crew member.");
                }
                return Results.Ok(newCrewMember);
            });

            app.MapPatch("crews/update-crew-member-role", async (int userId, int roleId) => 
            {
                var repo = new CrewMemberRepository(dbConnection);
                var updatedCrewMember = await repo.UpdateCrewMemberRoleAsync(userId, roleId);
                if (updatedCrewMember == null)
                {
                    return Results.BadRequest($"Failed to update crewmember role for user with ID: {userId}");
                }
                return Results.Ok(updatedCrewMember);
            });

            // Remove user from crew
            app.MapDelete("crews/delete-member", async (int crewId, int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                bool deleted = await repo.DeleteCrewMemberAsync(crewId, userId);
                if (!deleted)
                {
                    return Results.NotFound($"Failed to delete user with ID: {userId} from crew with ID: {crewId}");
                }
                return Results.Ok(deleted);
            });

            // Delete all crew memberships that belong to a single user
            app.MapDelete("crews/delete-all-user-memberships", async (int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                bool deleted = await repo.DeleteAllUserCrewMembershipsAsync(userId);
                if (!deleted) 
                {
                    return Results.NotFound($"Failed to delete all crew-memberships belonging to user with ID: {userId}");
                }
                return Results.Ok(deleted);
            });
        }            
    }
}