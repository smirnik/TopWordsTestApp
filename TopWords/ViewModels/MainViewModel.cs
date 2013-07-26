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

        /// <summary>
        /// ���� �� ���������� � ���������� �������
        /// </summary>
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyPropertyChanged("Path");
            }
        }

        /// <summary>
        /// ������� ������ ����������
        /// </summary>
        public ICommand SelectPathCommand
        {
            get { return _selectPathCommand.Value; }
        }

        /// <summary>
        /// ������� ������� ������ ����� ����� ����������� ����
        /// </summary>
        public ICommand RunCommand
        {
            get { return _runCommand.Value; }
        }

        /// <summary>
        /// ������ �� ��������� �����������
        /// </summary>
        public bool IsSearchInSubfolders
        {
            get { return _isSearchInSubfolders; }
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
                    Description = "�������� ���������� ��� ������ ��������� ������",
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}