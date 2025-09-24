using System.Windows;

namespace WorkTracker
{
    public partial class Changelog : Window
    {
        public string m_notes = string.Empty;

        public Changelog()
        {
            InitializeComponent();
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.m_notes = ChangesBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
