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
        private ObservableCollection<KeyValuePair<string, int>> _wordsList;
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
            Description = string.Format("Обработано {0} из {1} файлов", ProcessedCount, FilesCount);
            NotifyPropertyChanged(string.Empty);
            OnLog(ProgressInfo.LastFileName);
        }

        /// <summary>
        /// Ход выполнения поиска
        /// </summary>
        public ProgressInfo ProgressInfo
        {
            get { return _progressInfo; }
            set
            {
                _progressInfo = value;
                NotifyPropertyChanged(string.Empty);
            }
        }

        /// <summary>
        /// Количество файлов
        /// </summary>
        public int FilesCount
        {
            get { return ProgressInfo.FilesCount; }
        }

        /// <summary>
        /// Количество обработаных файлов
        /// </summary>
        public int ProcessedCount
        {
            get { return ProgressInfo.ProcessedCount; }
        }

        /// <summary>
        /// Количество ошибок
        /// </summary>
        public int FauledCount
        {
            get { return ProgressInfo.FauledCount; }
        }

        /// <summary>
        /// Количество слов
        /// </summary>
        public int WordsCount
        {
            get { return ProgressInfo.WordsCount; }
        }

        /// <summary>
        /// Частота самого встречаемого слова
        /// </summary>
        public int MaxCount
        {
            get { return _maxCount; }
            set
            {
                _maxCount = value;
                NotifyPropertyChanged(string.Empty);
            }
        }

        /// <summary>
        /// Статус выполнения процесса обработки
        /// </summary>
        public string Status
        {
            get
            {
                if (_task == null)
                    return string.Empty;

                switch (_task.Status)
                {
                    case TaskStatus.Canceled:
                        return "Отменено";
                    case TaskStatus.Running:
                        return string.Format("{0}% Завершено",
                                             FilesCount > 0
                                                 ? Math.Truncate(((double) ProcessedCount/FilesCount)*100)
                                                 : 0);
                    case TaskStatus.Faulted:
                        return "Ошибка";
                    case TaskStatus.RanToCompletion:
                        return "Завершено";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Описание статуса обработки
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Обработка активна
        /// </summary>
        public bool IsRunning
        {
            get { return _task != null && _task.Status == TaskStatus.Running; }
        }

        /// <summary>
        /// Результат работы (слово - количество)
        /// </summary>
        public ObservableCollection<KeyValuePair<string, int>> WordsList
        {
            get { return _wordsList; }
        }

        /// <summary>
        /// Событие для логирования хода выполнения
        /// </summary>
        public event Action<string> Log;

        protected virtual void OnLog(string obj)
        {
            Action<string> handler = Log;
            if (handler != null) handler(obj);
        }

        /// <summary>
        /// Начать поиск в заданой папке
        /// </summary>
        /// <param name="path">Путь к папке</param>
        /// <param name="isSearchInSubfolders">Искать в подпапках</param>
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
            //при успешном завершении
            _task.ContinueWith(GetResult, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion,
                               scheduler);
            //при любом завершии
            _task.ContinueWith(ProcessingCompleated, scheduler);

            OnLog("Начало поиска в директории " + path);
            NotifyPropertyChanged(string.Empty);

        }

        public TaskStatus TaskStatus
        {
            get
            {
                return _task != null ? _task.Status : TaskStatus.Created;
            }
        }

        private void ProcessingCompleated(Task<IEnumerable<KeyValuePair<string, int>>> task)
        {
            _cancallationToken = null;
            OnLog(string.Format("Поиск завершен со статусом '{0}'", task.Status));
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

        /// <summary>
        /// Команда отмены поиска
        /// </summary>
        public ICommand CancelProcessingCommand { get; set; }

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