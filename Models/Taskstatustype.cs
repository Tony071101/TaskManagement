namespace TaskManagement.Models;

public partial class Taskstatustype
{
    public int Taskstatustypeid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
