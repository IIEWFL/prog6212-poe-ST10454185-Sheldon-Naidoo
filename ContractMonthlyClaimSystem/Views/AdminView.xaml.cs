using ContractMonthlyClaimSystem.ViewModels;
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
using System.Windows.Shapes;

namespace ContractMonthlyClaimSystem.Views
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public partial class AdminView : Window
    {
        public AdminView()
        {
            InitializeComponent();

            // Part 2
            // Retrieves a reference to the ViewModel
            if (this.DataContext is AdminViewModel viewModel)
            {
                // Action property that calls for the Close method
                viewModel.CloseWindowAction = () =>
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close(); 
                };
            }
        }
    }
}
