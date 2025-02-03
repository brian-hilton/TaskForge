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

            app.MapGet("crews/{crewId}/get-all-members", (int crewId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var crewMembers = repo.GetAllCrewMembersByCrew(crewId);
                return crewMembers;
            });

            app.MapGet("crews/get-all-crews-for-user", (int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var crewList = repo.GetAllCrewMembersByCrew(userId);
                return crewList;
            });

            app.MapPost("crews/{crewId}/add-member", (int crewId, int userId, string role) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                var newCrewMember = repo.CreateCrewMember(crewId, userId, role);
                return newCrewMember;
            });

            app.MapDelete("crews/{crewId}/delete-member", (int crewId, int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                repo.DeleteCrewMember(crewId, userId);
            });

            app.MapDelete("crews/delete-all-user-memberships", (int userId) =>
            {
                var repo = new CrewMemberRepository(dbConnection);
                repo.DeleteAllUserCrewMemberships(userId);
            });
        }            
    }
}