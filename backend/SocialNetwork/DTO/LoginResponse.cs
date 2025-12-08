using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.DTO
{
    public class LoginResponse
    {
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";     // JWT-token som klienten använder
        public int UserId { get; set; }             // Identifierar användaren
        public string Username { get; set; } = "";  // Visningsnamn eller login-namn
    }
}