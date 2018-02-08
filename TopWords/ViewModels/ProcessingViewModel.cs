using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TopWordsTestApp.ViewModels
{
    public class ProcessingViewModel : INotifyPropertyChanged
    {
        private ProgressInfo _progressInfo;
        private Task<IEnumerable<KeyValuePair<string, int>>> _task;
        private CancellationTokenSource _cancallationToken;
        private readonly ObservableCollection<KeyValuePair<string, int>> _wordsList;
        private int _maxCount;
        private string _description;

        public ProcessingViewModel()
        {
            _wordsList = new ObservableCollection<KeyValuePair<string, int>>();
            _progressInfo = new ProgressInfo();
            _progressInfo.Update += ProgressInfoOnUpdate;
            CancelProcessingCommand = new RelayCommand(CancelProcessing,
                                                       o =>
                                                       _cancallationToken != null && _task != null &&
                                                       _task.Status == TaskStatus.Running);
        }

        private void CancelProcessing(object o)
        {
            if (_cancallationToken != null && _task != null && _task.Status == TaskStatus.Running)
            {
                _cancallationToken.Cancel(false);
            }
        }

        private void ProgressInfoOnUpdate()
        {
            Description = $"Processed {ProcessedCount} of {FilesCount} files";
            NotifyPropertyChanged(string.Empty);
            OnLog(ProgressInfo.LastFileName);
        }

        public ProgressInfo ProgressInfo
        {
            get => _progressInfo;
            set
            {
                _progressInfo = value;
                NotifyPropertyChanged(string.Empty);
            }
        }

        public int FilesCount => ProgressInfo.FilesCount;

        public int ProcessedCount => ProgressInfo.ProcessedCount;

        public int FauledCount => ProgressInfo.FauledCount;

        public int WordsCount => ProgressInfo.WordsCount;

        public int MaxCount
        {
            get => _maxCount;
            set
            {
                _maxCount = value;
                NotifyPropertyChanged(string.Empty);
            }
        }

        public string Status
        {
            get
            {
                if (_task == null)
                    return string.Empty;

                switch (_task.Status)
                {
                    case TaskStatus.Canceled:
                        return "Canceled";
                    case TaskStatus.Running:
                        return $"{(FilesCount > 0 ? Math.Truncate(((double) ProcessedCount / FilesCount) * 100) : 0)}% Completed";
                    case TaskStatus.Faulted:
                        return "Error";
                    case TaskStatus.RanToCompletion:
                        return "Completed";
                    default:
                        return string.Empty;
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public bool IsRunning => _task != null && _task.Status == TaskStatus.Running;

        public ObservableCollection<KeyValuePair<string, int>> WordsList => _wordsList;

        public event Action<string> Log;

        protected virtual void OnLog(string obj)
        {
            Log?.Invoke(obj);
        }

        public void Run(string path, bool isSearchInSubfolders)
        {
            if (_cancallationToken != null)
            {
                throw new OperationCanceledException();
            }

            _cancallationToken = new CancellationTokenSource();
            _task = FrequencyAnalisator.GetTopWords(_cancallationToken, ProgressInfo, path, isSearchInSubfolders);
            _task.Start();
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            
            _task.ContinueWith(GetResult, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion,
                               scheduler);
            //on error
            _task.ContinueWith(ProcessingCompleated, scheduler);

            OnLog("Starting search in folder " + path);
            NotifyPropertyChanged(string.Empty);

        }

        public TaskStatus TaskStatus => _task?.Status ?? TaskStatus.Created;

        private void ProcessingCompleated(Task<IEnumerable<KeyValuePair<string, int>>> task)
        {
            _cancallationToken = null;
            OnLog($"Finished with status '{task.Status}'");
            if (task.Status == TaskStatus.Faulted)
            {
                if (task.Exception != null && task.Exception.InnerExceptions.Any())
                {
                    Description = task.Exception.InnerExceptions[0].Message;
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        OnLog(ex.Message);
                    }
                }
            }
            NotifyPropertyChanged(string.Empty);
        }

        private void GetResult(Task<IEnumerable<KeyValuePair<string, int>>> task)
        {
            _wordsList.Clear();
            MaxCount = task.Result.Max(o => o.Value);
            foreach (var keyValuePair in task.Result)
            {
                _wordsList.Add(keyValuePair);
            }
        }

        public ICommand CancelProcessingCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}