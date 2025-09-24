using System.Windows;
using FoxCrypto;
using WorkTracker.Utils;

namespace WorkTracker
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            new Database();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UserBox.Text;
            var passhash = Crypto.GetPasskey(PassBox.Password);
            if (Database.Singleton.GetUserByUsername(username) == null)
            {
                var res = MessageBox.Show($"{username} does not yet exist.\n\nWould you like to register?", "Register?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
                User? registrant = null;
                do
                {
                    registrant = new User(username, passhash);
                }
                while (Database.Singleton.GetUserById(registrant.ID) != null);
                Database.Singleton.AddUser(registrant);
                if (Database.Singleton.GetUserByUsername(username) == null)
                {
                    MessageBox.Show("Something went wrong during creation!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var user = Database.Singleton.Login(username, passhash);
            if (user == null)
            {
                MessageBox.Show("Password incorrect!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var window = new MainWindow(user);
            window.Owner = this;
            user.Status = OnlineStatus.Idle;
            Database.Singleton.UpdateUser(user);
            this.Hide();
            UserBox.Text = string.Empty;
            PassBox.Password = string.Empty;
            window.ShowDialog();
            this.Close();
        }
    }
}