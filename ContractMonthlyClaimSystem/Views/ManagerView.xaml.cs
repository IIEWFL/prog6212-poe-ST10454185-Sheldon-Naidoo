using ContractMonthlyClaimSystem.Services;
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
    /// Interaction logic for ManagerView.xaml
    /// </summary>
    public partial class ManagerView : Window
    {
        public ManagerView()
        {
            InitializeComponent();

            // Instantiates the ClaimService dependency
            var claimService = new ClaimService();

            // Instantiates the ManagerViewModel
            var viewModel = new ManagerViewModel();

            // Sets the ViewModel as the DataContext
            this.DataContext = viewModel;

            // Subscribes to the RequestClose event from the ViewModelBase.
            // This allows the ViewModel to trigger the View to close itself.
            viewModel.RequestClose += (sender, e) =>
            {
                // Opens the main selection window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Closes the current ManagerView window
                this.Close();
            };
        }
    }
}
