using System;
using System.Windows;
using TopWordsTestApp.ViewModels;

namespace TopWordsTestApp.Views
{
    /// <summary>
    /// Interaction logic for FrequenceAnalysisWindow.xaml
    /// </summary>
    public partial class FrequenceAnalysisWindow : Window
    {
        public FrequenceAnalysisWindow()
        {
            InitializeComponent();
            ViewModel = new ProcessingViewModel();
            ViewModel.Log += ViewModelOnLog;
        }

        private void ViewModelOnLog(string s)
        {
            Dispatcher.Invoke(() =>
                              logTextBox.AppendText(string.Format("[{0}] {1}{2}", DateTime.Now.ToString("HH:mm:ss.ff"),
                                                                  s, Environment.NewLine)));
        }

        public static readonly DependencyProperty ViewModelProperty =
         DependencyProperty.Register("ViewModel", typeof(ProcessingViewModel),
         typeof(FrequenceAnalysisWindow), null);

        public ProcessingViewModel ViewModel
        {
            get { return (ProcessingViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
