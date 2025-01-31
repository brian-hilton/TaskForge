﻿using TaskForge.Models;

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

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class UpdateUserRequest 
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class UpdateJobRequest
    {
        public string? Name {  get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; } = DateTime.UtcNow;
    }
}
