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
    }
}
