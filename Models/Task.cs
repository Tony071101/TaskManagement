using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace TaskManagement.Models;

public partial class Task
{
    public int Taskid { get; set; }

    public string Taskname { get; set; } = null!;

    public DateTime Datetimetaskcreated { get; set; }

    public DateTime Datetimeassign { get; set; }

    public DateTime Datetimedue { get; set; }
    
    //Thiếu DateTime Completed

    public int Categoryid { get; set; }

    public int Statusid { get; set; }

    public int Assigner { get; set; }

    public int? Assignedto { get; set; }

    public int? Taskchecker { get; set; }

    public NpgsqlTsVector? SearchVector { get; set; }

    public virtual User? AssignedtoNavigation { get; set; }

    public virtual User? AssignerNavigation { get; set; } = null!;

    public virtual Taskcategory? Category { get; set; } = null!;

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual Taskstatustype? Status { get; set; } = null!;

    public virtual User? TaskcheckerNavigation { get; set; }

    public virtual ICollection<Taskcomment> Taskcomments { get; set; } = new List<Taskcomment>();

    public virtual ICollection<Tasktimetracking> Tasktimetrackings { get; set; } = new List<Tasktimetracking>();

    public string? description { get; set; } 
    
    public string? attachmenturl { get; set; }

    public string? githubprurl { get; set; }
}
