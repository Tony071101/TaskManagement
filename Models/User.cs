namespace TaskManagement.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Roleid { get; set; }

    public string? Email { get; set; }

    public int? Totaltaskcompleted { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Roletype Role { get; set; } = null!;

    public virtual ICollection<Task> TaskAssignedtoNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskAssignerNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskTaskcheckerNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Taskcomment> Taskcomments { get; set; } = new List<Taskcomment>();

    public virtual ICollection<Tasktimetracking> Tasktimetrackings { get; set; } = new List<Tasktimetracking>();
}
