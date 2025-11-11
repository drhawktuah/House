using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.HTTP;

public class CoomerPost
{
    public string ID { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string? Text { get; set; }

    public DateTime Published { get; set; }

    public bool IsDirectMessage { get; set; }

    public List<CoomerFile> Attachments { get; set; } = [];
    public List<string> Labels { get; set; } = [];

    public string GetPostUrl(string service, string creatorID) => $"https://coomer.st//post/{ID}";
}
