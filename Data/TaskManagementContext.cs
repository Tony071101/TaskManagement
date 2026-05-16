using Microsoft.EntityFrameworkCore;
using TaskManagement.Models;
using Task = TaskManagement.Models.Task;
namespace TaskManagement.Datas;

public partial class TaskManagementContext : DbContext
{
    public TaskManagementContext()
    {
    }

    public TaskManagementContext(DbContextOptions<TaskManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Roletype> Roletypes { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Taskcategory> Taskcategories { get; set; }

    public virtual DbSet<Taskcomment> Taskcomments { get; set; }

    public virtual DbSet<Taskstatustype> Taskstatustypes { get; set; }

    public virtual DbSet<Tasktimetracking> Tasktimetrackings { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<TaskDependency> TaskDependencies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Lấy cấu hình từ Program.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Conversationid).HasName("conversation_pkey");

            entity.ToTable("conversation");

            entity.Property(e => e.Conversationid).HasColumnName("conversationid");
            entity.Property(e => e.Conversationtype)
                .HasMaxLength(50)
                .HasColumnName("conversationtype");
            entity.Property(e => e.Taskid).HasColumnName("taskid");

            entity.HasOne(d => d.Task).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.Taskid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("conversation_taskid_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Messageid).HasName("message_pkey");

            entity.ToTable("message");

            entity.HasIndex(e => e.Conversationid, "idx_message_conversationid");

            entity.HasIndex(e => e.Senderid, "idx_message_senderid");

            entity.Property(e => e.Messageid).HasColumnName("messageid");
            entity.Property(e => e.Conversationid).HasColumnName("conversationid");
            entity.Property(e => e.Datetimesent)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datetimesent");
            entity.Property(e => e.Messagecontent).HasColumnName("messagecontent");
            entity.Property(e => e.Senderid).HasColumnName("senderid");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Conversationid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("message_conversationid_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Senderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("message_senderid_fkey");
        });

        modelBuilder.Entity<Roletype>(entity =>
        {
            entity.HasKey(e => e.Roletypeid).HasName("roletype_pkey");

            entity.ToTable("roletype");

            entity.HasIndex(e => e.Rolename, "roletype_rolename_key").IsUnique();

            entity.Property(e => e.Roletypeid).HasColumnName("roletypeid");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Taskid).HasName("task_pkey");

            entity.ToTable("task");

            entity.HasIndex(e => e.Assignedto, "idx_task_assignedto");

            entity.HasIndex(e => e.Assigner, "idx_task_assigner");

            entity.HasIndex(e => e.SearchVector, "idx_task_search_vector").HasMethod("gin");

            entity.HasIndex(e => e.Statusid, "idx_task_statusid");

            entity.HasIndex(e => e.Taskchecker, "idx_task_taskchecker");

            entity.Property(e => e.Taskid).HasColumnName("taskid");
            entity.Property(e => e.Assignedto).HasColumnName("assignedto");
            entity.Property(e => e.Assigner).HasColumnName("assigner");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Datetimeassign)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datetimeassign");
            entity.Property(e => e.Datetimedue)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datetimedue");
            entity.Property(e => e.Datetimetaskcreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datetimetaskcreated");
            entity.Property(e => e.SearchVector).HasColumnName("search_vector");
            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Taskchecker).HasColumnName("taskchecker");
            entity.Property(e => e.Taskname)
                .HasMaxLength(255)
                .HasColumnName("taskname");

            entity.HasOne(d => d.AssignedtoNavigation).WithMany(p => p.TaskAssignedtoNavigations)
                .HasForeignKey(d => d.Assignedto)
                .HasConstraintName("task_assignedto_fkey");

            entity.HasOne(d => d.AssignerNavigation).WithMany(p => p.TaskAssignerNavigations)
                .HasForeignKey(d => d.Assigner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_assigner_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_categoryid_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.Statusid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_statusid_fkey");

            entity.HasOne(d => d.TaskcheckerNavigation).WithMany(p => p.TaskTaskcheckerNavigations)
                .HasForeignKey(d => d.Taskchecker)
                .HasConstraintName("task_taskchecker_fkey");

            entity.Property(e => e.githubprurl)
                .HasColumnName("githubprurl")
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Taskcategory>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("taskcategory_pkey");

            entity.ToTable("taskcategory");

            entity.HasIndex(e => e.Categoryname, "taskcategory_categoryname_key").IsUnique();

            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(100)
                .HasColumnName("categoryname");
        });

        modelBuilder.Entity<Taskcomment>(entity =>
        {
            entity.HasKey(e => e.Taskcommentid).HasName("taskcomment_pkey");

            entity.ToTable("taskcomment");

            entity.HasIndex(e => e.Taskid, "idx_comment_taskid");

            entity.HasIndex(e => e.Userid, "idx_comment_userid");

            entity.Property(e => e.Taskcommentid).HasColumnName("taskcommentid");
            entity.Property(e => e.Commentcontent).HasColumnName("commentcontent");
            entity.Property(e => e.Datetimecomment)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datetimecomment");
            entity.Property(e => e.Taskid).HasColumnName("taskid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Task).WithMany(p => p.Taskcomments)
                .HasForeignKey(d => d.Taskid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("taskcomment_taskid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Taskcomments)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("taskcomment_userid_fkey");
        });

        modelBuilder.Entity<Taskstatustype>(entity =>
        {
            entity.HasKey(e => e.Taskstatustypeid).HasName("taskstatustype_pkey");

            entity.ToTable("taskstatustype");

            entity.HasIndex(e => e.Statusname, "taskstatustype_statusname_key").IsUnique();

            entity.Property(e => e.Taskstatustypeid).HasColumnName("taskstatustypeid");
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasColumnName("statusname");
        });

        modelBuilder.Entity<Tasktimetracking>(entity =>
        {
            entity.HasKey(e => e.Tasktimetrackingid).HasName("tasktimetracking_pkey");

            entity.ToTable("tasktimetracking");

            entity.HasIndex(e => e.Taskid, "idx_timetrack_taskid");

            entity.HasIndex(e => e.Userid, "idx_timetrack_userid");

            entity.Property(e => e.Tasktimetrackingid).HasColumnName("tasktimetrackingid");
            entity.Property(e => e.Checkintime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("checkintime");
            entity.Property(e => e.Checkouttime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("checkouttime");
            entity.Property(e => e.Taskid).HasColumnName("taskid");
            entity.Property(e => e.Totaltime).HasColumnName("totaltime");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Task).WithMany(p => p.Tasktimetrackings)
                .HasForeignKey(d => d.Taskid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasktimetracking_taskid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Tasktimetrackings)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasktimetracking_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "User_username_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Totaltaskcompleted)
                .HasDefaultValue(0)
                .HasColumnName("totaltaskcompleted");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_roleid_fkey");
        });

        modelBuilder.Entity<TaskDependency>()
            .HasKey(td => new { td.PredecessorTaskId, td.SuccessorTaskId });

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.PredecessorTask)
            .WithMany()
            .HasForeignKey(td => td.PredecessorTaskId);

        modelBuilder.Entity<TaskDependency>()
            .HasOne(td => td.SuccessorTask)
            .WithMany()
            .HasForeignKey(td => td.SuccessorTaskId);

            OnModelCreatingPartial(modelBuilder);
    }
        

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
