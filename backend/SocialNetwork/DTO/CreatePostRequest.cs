using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.DTO
{

    public class CreatePostRequest
    {
        public int ToUserId { get; set; }
        public string Message { get; set; } = "";
    }
}