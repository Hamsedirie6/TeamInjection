using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.DTOs.Posts;

public class CreatePostRequest
{
    public int ToUserId { get; set; }
    public string Message { get; set; } = "";
}
