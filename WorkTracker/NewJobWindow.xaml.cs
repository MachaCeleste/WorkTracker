using System.Windows;

namespace WorkTracker
{
    public partial class NewJobWindow : Window
    {
        public string m_name = "";
        public string m_description = "";
        public bool m_isPublic = false;

        public NewJobWindow()
        {
            InitializeComponent();
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            this.m_name = NameBox.Text;
            this.m_description = DescriptionBox.Text;
            this.m_isPublic = IsPublicCheckbox.IsChecked.GetValueOrDefault();
            this.DialogResult = true;
            this.Close();
        }
    }
}
