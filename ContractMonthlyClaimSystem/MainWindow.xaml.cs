using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContractMonthlyClaimSystem.Views;

namespace ContractMonthlyClaimSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // --- View Launch Methods ---
        // Part 2: NEW Codes
        private void LaunchLecturerView_Click(object sender, RoutedEventArgs e)
        {
            LecturerView lecturerView = new LecturerView();
            lecturerView.Show();
            this.Close();
        }

        private void LaunchAdminView_Click(object sender, RoutedEventArgs e)
        {
            AdminView adminView = new AdminView();
            adminView.Show();
            this.Close();
        }

        private void LaunchManagerView_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the ManagerView
            ManagerView managerView = new ManagerView();
            managerView.Show();
            this.Close(); // Close the main selection window
        }

        private void LaunchHRView_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the HRView
            HRView hrView = new HRView();
            hrView.Show();
            this.Close(); // Close the main selection window
        }
    }
}
