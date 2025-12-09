using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.DTO
{
    public class DirectMessageRequest
    {
        public int ReceiverId { get; set; }
        [MaxLength(500)]

        [MaxLength(500)]
        public string Message { get; set; } = "";
    }
}
