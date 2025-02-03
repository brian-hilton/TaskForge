using Microsoft.Identity.Client;

namespace TaskForge.Models
{
    public class Crew
    {
        public int Id { get; set; }
        public int? SupervisorId { get; set; }
        public string? Name { get; set; } = "work crew";
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
