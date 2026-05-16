namespace TaskManagement.Models;

public partial class Message
{
    public int Messageid { get; set; }

    public int Conversationid { get; set; }

    public int Senderid { get; set; }

    public string Messagecontent { get; set; } = null!;

    public DateTime Datetimesent { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
