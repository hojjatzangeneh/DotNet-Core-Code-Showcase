using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace WpfConfigureAwaitDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_WithContext_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading...";
            await Task.Delay(2000); // Simulates I/O work
            StatusText.Text = "Finished without ConfigureAwait(false)";
        }

        private async void Button_WithoutContext_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading...";
            await Task.Delay(2000).ConfigureAwait(false); // This will NOT return to UI thread
            try
            {
                StatusText.Text = "This will crash!"; // Cross-thread access
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
