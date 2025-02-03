namespace TaskForge.Models
{
    public class CrewMember
    {
        public int CrewId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = "worker";
    }
}
