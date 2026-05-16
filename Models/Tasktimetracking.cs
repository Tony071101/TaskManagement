namespace TaskManagement.Models;

public partial class Tasktimetracking
{
    public int Tasktimetrackingid { get; set; }

    public int Taskid { get; set; }

    public int Userid { get; set; }

    public DateTime Checkintime { get; set; }

    public DateTime? Checkouttime { get; set; }

    public TimeSpan? Totaltime { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
