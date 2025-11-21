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
    /// Interaction logic for LecturerView.xaml
    /// </summary>
    public partial class LecturerView : Window
    {
        public LecturerView()
        {
            InitializeComponent();

            // Part 2
            // Ensures the DataContext is set and the ViewModel is the correct type
            if (this.DataContext is LecturerViewModel viewModel)
            {
                // Action that the ViewModel triggers when GoHomeCommand runs
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
