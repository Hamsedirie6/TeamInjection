using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.DTO
{

    public class DirectMessageRequest
    {
        public int ReceiverId { get; set; }
        [MaxLength(500)]
        public string Message { get; set; } = "";
    }
}
