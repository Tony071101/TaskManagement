using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Viewmodels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Input your Username")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Input your Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}