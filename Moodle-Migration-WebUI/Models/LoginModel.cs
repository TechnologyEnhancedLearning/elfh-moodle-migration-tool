using System.ComponentModel.DataAnnotations;

namespace Moodle_Migration_WebUI.Models
{
    public class LoginModel
    {
        [Required]
        [MinLength(4)]
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int Age { get; set; }
        public IFormFile? File { get; set; }
    }

    public class AuthUser
    {
        private string? username;

        [Required]
        [Display(Name = "Username")]
        public string Username
        {
            get => username ?? string.Empty;
            set => username = value;
        }

        public string? Password { get; set; }
    }
}
