namespace WorkTracker.Utils
{
    public class User
    {
        public Guid ID { get; set; }
        public string? Username { get; set; }
        public string? Passhash { get; set; }
        public OnlineStatus Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }

        public User() { }
        public User(string username, string passhash)
        {
            this.ID = Guid.NewGuid();
            this.Username = username;
            this.Passhash = passhash;
            this.Status = OnlineStatus.Offline;
            this.Created = DateTime.Now;
            this.LastLogin = DateTime.Now;
        }
    }

    public class Job
    {
        public Guid ID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid Creator { get; set; }
        public DateTime Created { get; set; }
        public bool IsPublic { get; set; }
        public List<Session>? Sessions { get; set; }

        public Job() { }
        public Job(Guid userId, string name, string description, bool isPublic)
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
            this.Description = description;
            this.Creator = userId;
            this.Created = DateTime.Now;
            this.IsPublic = isPublic;
            this.Sessions = new List<Session>();
        }
    }

    public class Session
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Session() { }
        public Session(Guid userId)
        {
            this.ID = Guid.NewGuid();
            this.UserID = userId;
            this.StartTime = DateTime.Now;
        }
    }

    public enum OnlineStatus
    {
        Offline,
        Idle,
        InSession
    }

    public static class Formats
    {
        public static readonly string FormatDateTime = "yyyy-MM-ddTHH:mm:ss.fffffff";
    }
}
