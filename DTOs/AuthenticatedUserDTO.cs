namespace TaskManagement.DTOs
{
    public class AuthenticatedUserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}