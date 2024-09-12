using System.Windows;
using PMC_Eight_Mount_Control.ViewModels;

namespace PMC_Eight_Mount_Control.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(); // Bind to the ViewModel
        }
    }
}
