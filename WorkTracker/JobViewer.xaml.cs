using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;
using WorkTracker.Utils;
using System.ComponentModel;

namespace WorkTracker
{
    public partial class JobViewer : Window, INotifyPropertyChanged
    {
        private string jobName;
        public string JobName
        {
            get { return jobName; }
            set
            {
                jobName = value;
                RaisePropertyChanged("JobName");
            }
        }
        private string iD;
        public string ID
        {
            get { return iD; }
            set
            {
                iD = value;
                RaisePropertyChanged("ID");
            }
        }
        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }
        private string creator;
        public string Creator
        {
            get { return creator; }
            set
            {
                creator = value;
                RaisePropertyChanged("Creator");
            }
        }
        private string created;
        public string Created
        {
            get { return created; }
            set
            {
                created = value;
                RaisePropertyChanged("Created");
            }
        }
        private bool isPublic;
        public bool IsPublic
        {
            get { return isPublic; }
            set
            {
                isPublic = value;
                RaisePropertyChanged("IsPublic");
            }
        }
        private ObservableCollection<Session> sessions;
        public ObservableCollection<Session> Sessions
        {
            get { return sessions; }
            set
            {
                sessions = value;
                RaisePropertyChanged("Sessions");
            }
        }

        private User m_user;
        private Job m_job;
        private Session m_curSession;
        private Stopwatch m_stopwatch;
        private bool m_running;

        public JobViewer(User user, Job job)
        {
            m_user = user;
            m_job = job;
            m_running = true;
            UpdateLoop();
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task UpdateLoop()
        {
            while (m_running)
            {
                UpdateJob();
                await Task.Delay(1000);
            }
        }

        private void UpdateJob()
        {
            var job = Database.Singleton.GetJobByJobId(m_job.ID);
            if (job != null)
            {
                m_job = job;
                var username = Database.Singleton.GetUserById(m_job.Creator)?.Username;
                this.JobName = m_job.Name;
                this.ID = m_job.ID.ToString();
                this.Description = m_job.Description;
                this.Creator = username == null ? m_job.Creator.ToString() : username;
                this.Created = m_job.Created.ToString(Formats.FormatDateTime);
                this.IsPublic = m_job.IsPublic;
                this.Sessions = new ObservableCollection<Session>(m_job.Sessions);
            }
        }


        private void StartSession_Click(object sender, RoutedEventArgs e)
        {
            if (m_user.Status == OnlineStatus.InSession)
                return;
            m_user.Status = OnlineStatus.InSession;
            Database.Singleton.UpdateUser(m_user);

            StartSessionButton.IsEnabled = false;
            EndSessionButton.IsEnabled = true;

            m_curSession = new Session(m_user.ID);
            m_stopwatch = Stopwatch.StartNew();
        }

        private void EndSession_Click(Object sender, RoutedEventArgs e)
        {
            if (m_user.Status != OnlineStatus.InSession)
                return;
            m_user.Status = OnlineStatus.Idle;
            Database.Singleton.UpdateUser(m_user);

            StartSessionButton.IsEnabled = true;
            EndSessionButton.IsEnabled = false;

            m_stopwatch.Stop();
            m_curSession.Duration = TimeSpan.FromMilliseconds(m_stopwatch.ElapsedMilliseconds);
            m_curSession.EndTime = DateTime.Now;

            var res = new Changelog();
            res.Owner = this;
            this.IsEnabled = false;
            res.ShowDialog();
            this.IsEnabled = true;
            if (res.DialogResult.HasValue && res.DialogResult.Value)
            {
                m_curSession.Notes = res.m_notes;
            }

            m_job.Sessions.Add(m_curSession);
            Database.Singleton.UpdateJob(m_job);
        }
    }
}
