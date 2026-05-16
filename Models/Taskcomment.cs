namespace TaskManagement.Models;

public partial class Taskcomment
{
    public int Taskcommentid { get; set; }

    public int Taskid { get; set; }

    public int Userid { get; set; }

    public string Commentcontent { get; set; } = null!;

    public DateTime Datetimecomment { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
