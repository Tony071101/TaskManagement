namespace TaskManagement.Models;

public partial class Conversation
{
    public int Conversationid { get; set; }

    public int Taskid { get; set; }

    public string? Conversationtype { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Task Task { get; set; } = null!;
}
