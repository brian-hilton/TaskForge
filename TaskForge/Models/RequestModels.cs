namespace TaskForge.Models
{
    public class AssignJobRequest 
    {
        public int JobId { get; set; }
        public int UserId { get; set; } 
    }

    public class CreateRoleRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; } = 0;
        /*
        1 = supervisor
        2 = worker
        3 = project manager
        */
    }
}
