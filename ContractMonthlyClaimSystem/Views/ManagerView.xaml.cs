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

            // 1. Instantiate the ClaimService dependency
            // NOTE: You must ensure ClaimService is in the ContractMonthlyClaimSystem.Services namespace.
            var claimService = new ClaimService();

            // 2. Instantiate the ManagerViewModel
            var viewModel = new ManagerViewModel();

            // 3. Set the ViewModel as the DataContext
            this.DataContext = viewModel;

            // 4. Subscribe to the RequestClose event from the ViewModelBase.
            // This allows the ViewModel (e.g., GoHomeCommand) to trigger the View to close itself.
            viewModel.RequestClose += (sender, e) =>
            {
                // 1. Open the main selection window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // 2. Close the current ManagerView window
                this.Close();
            };
        }
    }
}
