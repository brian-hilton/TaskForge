namespace TaskForge.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public int? UserId {  get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
