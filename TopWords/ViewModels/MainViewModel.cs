using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace TopWordsTestApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _path;
        private readonly Lazy<ICommand> _selectPathCommand;
        private readonly Lazy<ICommand> _runCommand;
        private bool _isSearchInSubfolders;

        public MainViewModel()
        {
            _selectPathCommand = new Lazy<ICommand>(() => new RelayCommand(SelectPath));
            _runCommand = new Lazy<ICommand>(() => new RelayCommand(RunSearch));
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyPropertyChanged("Path");
            }
        }

        public ICommand SelectPathCommand => _selectPathCommand.Value;

        public ICommand RunCommand => _runCommand.Value;

        public bool IsSearchInSubfolders
        {
            get => _isSearchInSubfolders;
            set
            {
                _isSearchInSubfolders = value;
                NotifyPropertyChanged("IsSearchInSubfolders");
            }
        }

        private void RunSearch(object obj)
        {
            var frequenceAnalysisWindow = new Views.FrequenceAnalysisWindow();
            frequenceAnalysisWindow.ViewModel.Run(Path, IsSearchInSubfolders);
            frequenceAnalysisWindow.ShowDialog();
        }

        private void SelectPath(object o)
        {
            var dialog = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                    Description = "Select folder with text files",
                    SelectedPath = Path,
                };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Path = dialog.SelectedPath;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}