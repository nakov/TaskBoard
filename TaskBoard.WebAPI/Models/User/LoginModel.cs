using System.ComponentModel.DataAnnotations;

namespace TaskBoard.WebAPI.Models.User
{
    public class LoginModel
    {
        [Required]
        public string Username { get; init; }

        [Required]
        public string Password { get; init; }
    }
}