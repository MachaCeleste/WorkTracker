using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using WorkTracker.Utils;

namespace WorkTracker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Job> jobs;

        public ObservableCollection<Job> Jobs
        {
            get { return jobs; }
            set
            {
                jobs = value;
                RaisePropertyChanged("Jobs");
            }
        }

        public string Username { get; set; }

        private TimeSpan sessionTotal;
        public TimeSpan SessionTotal
        {
            get { return sessionTotal; }
            set
            {
                sessionTotal = value;
                RaisePropertyChanged("SessionTotal");
            }
        }
        
        private User m_user { get; set; }
        private DateTime m_lastUpdate { get; set; }

        public MainWindow(User user)
        {
            m_user = user;
            Username = m_user.Username;
            UpdateJobs();

            InitializeComponent();
            DataContext = this;

            UpdateSessionTime();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateSessionTime()
        {
            SessionTotal = TimeSpan.Zero;
            foreach (Job job in Jobs)
            {
                foreach (Session session in job.Sessions.Where(x => x.UserID == m_user.ID))
                {
                    SessionTotal += session.EndTime - session.StartTime;
                }
            }
        }

        private void UpdateJobs()
        {
            Jobs = new ObservableCollection<Job>(Database.Singleton.GetAllJobsAllowed(m_user.ID));
            m_lastUpdate = DateTime.Now;
        }

        private Job? SelectJob()
        {
            if (JobsViewer.SelectedIndex > Jobs.Count - 1 || JobsViewer.SelectedIndex == -1)
                return null;
            return Jobs[JobsViewer.SelectedIndex];
        }

        private void RefreshAll()
        {
            UpdateJobs();
            UpdateSessionTime();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshAll();
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to delete your User data?\n\nThis action cannot be reversed!", "Delete User?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            this.Hide();
            Database.Singleton.DeleteUser(m_user.ID);
        }

        private void NewJob_Click(object sender, RoutedEventArgs e)
        {
            var res = new NewJobWindow();
            res.Owner = this;
            this.IsEnabled = false;
            res.ShowDialog();
            this.IsEnabled = true;
            if (res.DialogResult.HasValue && res.DialogResult.Value)
            {
                var job = new Job(m_user.ID, res.m_name, res.m_description, res.m_isPublic);
                Jobs.Add(job);
                Database.Singleton.AddJob(job);
            }
            UpdateJobs();
        }

        private void ViewJob_Click(object sender, RoutedEventArgs e)
        {
            var job = SelectJob();
            if (job == null) return;
            var res = new JobViewer(m_user, job);
            res.Owner = this;
            this.IsEnabled = false;
            res.ShowDialog();
            this.IsEnabled = true;
        }

        private void DeleteJob_Click(Object sender, RoutedEventArgs e)
        {
            var job = SelectJob();
            if (job == null) return;
            var res = MessageBox.Show($"Are you sure you want to permenantly delete {job.Name}?\n\nThis action cannot be reversed!", "Delete Job?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            Database.Singleton.DeleteJob(job.ID);
            RefreshAll();
        }
    }
}
