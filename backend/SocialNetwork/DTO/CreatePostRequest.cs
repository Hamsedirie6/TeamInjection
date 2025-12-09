
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.DTO
{
    public class CreatePostRequest
    {
        public int ToUserId { get; set; }


        [MaxLength(500)]
        public string Message { get; set; } = "";
    }
}
