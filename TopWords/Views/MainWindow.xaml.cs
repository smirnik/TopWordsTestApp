using System.Windows;
using TopWordsTestApp.ViewModels;

namespace TopWordsTestApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        public static readonly DependencyProperty ViewModelProperty =
             DependencyProperty.Register("ViewModel", typeof(MainViewModel),
             typeof(MainWindow), null);

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }
    }
}
