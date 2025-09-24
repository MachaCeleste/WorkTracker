using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using WorkTracker.Utils;

public class Database
{
    public static Database Singleton;

    private SqliteConnection sqlConn;

    public Database()
    {
        FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        var DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileInfo.CompanyName, fileInfo.ProductName);
        if (!Directory.Exists(DataPath))
            Directory.CreateDirectory(DataPath);
        sqlConn = new SqliteConnection($"Data Source={Path.Combine(DataPath, "workdata.db")}");
        sqlConn.Open();

        CreateDatabase();
        Database.Singleton = this;
    }

    /// <summary>
    /// Login to a User with a Username and Passhash
    /// </summary>
    /// <param name="username"></param>
    /// <param name="passhash"></param>
    /// <returns>The given user or null</returns>
    public User? Login(string username, string passhash)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users Where Username=@Username AND Passhash=@Passhash";
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@Passhash", passhash);

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return PullUser(reader);
        }
        return null;
    }

    /// <summary>
    /// Get a User by their UserID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>User Object or null</returns>
    public User? GetUserById(Guid userId)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE ID=@ID";
        cmd.Parameters.AddWithValue("@ID", userId.ToString());

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
            return PullUser(reader);
        return null;
    }

    /// <summary>
    /// Get a User by their Username
    /// </summary>
    /// <param name="username"></param>
    /// <returns>User Object or null</returns>
    public User? GetUserByUsername(string username)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE Username=@Username";
        cmd.Parameters.AddWithValue("@Username", username);

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
            return PullUser(reader);
        return null;
    }

    /// <summary>
    /// Adds a new User
    /// </summary>
    /// <param name="user"></param>
    public void AddUser(User user)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (ID, Username, Passhash, Status, Created, LastLogin) VALUES (@ID, @Username, @Passhash, @Status, @Created, @LastLogin)";
        PushUser(user, cmd);
    }

    /// <summary>
    /// Updates an existing User
    /// </summary>
    /// <param name="user"></param>
    public void UpdateUser(User user)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET ID=@ID, Username=@Username, Passhash=@Passhash, Status=@Status, Created=@Created, LastLogin=@LastLogin WHERE ID=@ID";
        PushUser(user, cmd);
    }

    /// <summary>
    /// Delete a User entry
    /// </summary>
    /// <param name="userId"></param>
    public void DeleteUser(Guid userId)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE ID=@ID";
        cmd.Parameters.AddWithValue("@ID", userId.ToString());
        cmd.ExecuteNonQuery();
    }


    /// <summary>
    /// Get a Job by its JobID
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns>Job Object or null</returns>
    public Job? GetJobByJobId(Guid jobId)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Jobs WHERE ID=@ID";
        cmd.Parameters.AddWithValue("@ID", jobId.ToString());

        using SqliteDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
            return PullJob(reader);
        return null;
    }

    /// <summary>
    /// Get all Jobs
    /// </summary>
    /// <returns>A List of all Job Objects</returns>
    public List<Job> GetAllJobs()
    {
        List<Job> jobs = new List<Job>();
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Jobs";

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Job job = PullJob(reader);
            jobs.Add(job);
        }

        return jobs;
    }

    /// <summary>
    /// Get all Jobs the User is allowed to see
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>A List of all Job Objects</returns>
    public List<Job> GetAllJobsAllowed(Guid userId)
    {
        List<Job> jobs = new List<Job>();
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Jobs WHERE IsPublic=@Public OR Creator=@UserID";
        cmd.Parameters.AddWithValue("@Public", true);
        cmd.Parameters.AddWithValue("@UserID", userId.ToString());

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Job job = PullJob(reader);
            jobs.Add(job);
        }

        return jobs;
    }

    /// <summary>
    /// Get Jobs made by a Creator UserID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>A List of all Job Objects from Creator or null</returns>
    public List<Job>? GetJobsByCreator(Guid userId)
    {
        List<Job> jobs = new List<Job>();
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Jobs WHERE Creator=@UserId";
        cmd.Parameters.AddWithValue("@UserId", userId);

        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Job job = PullJob(reader);
            jobs.Add(job);
        }
        if (jobs.Count > 0)
            return jobs;
        return null;
    }

    /// <summary>
    /// Add a new Job
    /// </summary>
    /// <param name="job"></param>
    public void AddJob(Job job)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "INSERT INTO Jobs (ID, Name, Description, Creator, Created, IsPublic, Sessions) VALUES (@ID, @Name, @Description, @Creator, @Created, @IsPublic, @Sessions)";
        PushJob(job, cmd);
    }

    /// <summary>
    /// Update an existing Job
    /// </summary>
    /// <param name="job"></param>
    public void UpdateJob(Job job)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "UPDATE Jobs SET ID=@ID, Name=@Name, Description=@Description, Creator=@Creator, Created=@Created, IsPublic=@IsPublic, Sessions=@Sessions WHERE ID=@ID";
        PushJob(job, cmd);
    }

    /// <summary>
    /// Delete a Job entry
    /// </summary>
    /// <param name="jobId"></param>
    public void DeleteJob(Guid jobId)
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = "DELETE FROM Jobs WHERE ID=@ID";
        cmd.Parameters.AddWithValue("@ID", jobId.ToString());
        cmd.ExecuteNonQuery();
    }


    private void CreateDatabase()
    {
        using var cmd = sqlConn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                ID TEXT PRIMARY KEY,
                Username TEXT NOT NULL UNIQUE,
                Passhash TEXT NOT NULL,
                Status INTEGER NOT NULL,
                Created TEXT NOT NULL,
                LastLogin TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Jobs (
                ID TEXT PRIMARY KEY,
                Name TEXT NOT NULL UNIQUE,
                Description TEXT NOT NULL,
                Creator TEXT NOT NULL,
                Created TEXT NOT NULL,
                IsPublic INTEGER NOT NULL,
                Sessions TEXT NOT NULL
            )";
        cmd.ExecuteNonQuery();
    }

    private void PushUser(User user, SqliteCommand cmd)
    {
        cmd.Parameters.AddWithValue("@ID", user.ID.ToString());
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@Passhash", user.Passhash);
        cmd.Parameters.AddWithValue("@Status", user.Status);
        cmd.Parameters.AddWithValue("@Created", user.Created.ToString(Formats.FormatDateTime));
        cmd.Parameters.AddWithValue("@LastLogin", user.LastLogin.ToString(Formats.FormatDateTime));
        cmd.ExecuteNonQuery();
    }

    private User PullUser(SqliteDataReader reader)
    {
        return new User
        {
            ID = Guid.Parse(reader.GetString(0).ToString()),
            Username = reader.GetString(1),
            Passhash = reader.GetString(2),
            Status = (OnlineStatus)reader.GetInt32(3),
            Created = DateTime.Parse(reader.GetString(4)),
            LastLogin = DateTime.Parse(reader.GetString(5))
        };
    }

    private void PushJob(Job job, SqliteCommand cmd)
    {
        cmd.Parameters.AddWithValue("@ID", job.ID.ToString());
        cmd.Parameters.AddWithValue("@Name", job.Name);
        cmd.Parameters.AddWithValue("@Description", job.Description);
        cmd.Parameters.AddWithValue("@Creator", job.Creator.ToString());
        cmd.Parameters.AddWithValue("@Created", job.Created.ToString(Formats.FormatDateTime));
        cmd.Parameters.AddWithValue("@IsPublic", job.IsPublic ? 1 : 0);
        var jobs = JsonConvert.SerializeObject(job.Sessions);
        cmd.Parameters.AddWithValue("@Sessions", jobs);
        cmd.ExecuteNonQuery();
    }

    private Job PullJob(SqliteDataReader reader)
    {
        Job job = new Job
        {
            ID = Guid.Parse(reader.GetString(0)),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            Creator = Guid.Parse(reader.GetString(3)),
            Created = DateTime.Parse(reader.GetString(4)),
            IsPublic = reader.GetInt32(5) == 1 ? true : false,
            Sessions = new List<Session>()
        };
        var sessions = JsonConvert.DeserializeObject<List<Session>>(reader.GetString(6));
        if (sessions != null)
            job.Sessions = sessions;
        return job;
    }
}
