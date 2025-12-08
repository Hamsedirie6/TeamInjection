using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.DTOs;

public class DirectMessageRequest
{
    public int ReceiverId { get; set; }
    public string Message { get; set; } = "";
}