namespace TaskManagement.Models;

public partial class Roletype
{
    public int Roletypeid { get; set; }

    public string Rolename { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
