using System;
using System.Threading.Tasks;

namespace TopWordsTestApp
{
    /// <summary>
    /// Отселживание хода выполнения поиска слов
    /// </summary>
    public class ProgressInfo
    {
        private readonly object _lock = new object();

        /// <summary>
        /// Общее количество файлов
        /// </summary>
        public int FilesCount { get; private set; } 
        
        /// <summary>
        /// Количество обработаных файлов
        /// </summary>
        public int ProcessedCount { get; private set; }
 
        /// <summary>
        /// Количество ошибок
        /// </summary>
        public int FauledCount { get; private set; } 
        
        /// <summary>
        /// Общее количество найденых слов
        /// </summary>
        public int WordsCount { get; private set; } 
        
        /// <summary>
        /// Последний обработаный файл
        /// </summary>
        public string LastFileName { get; private set; }
        
        /// <summary>
        /// Статус процесса обрабатывающего файл
        /// </summary>
        public TaskStatus TaskStatus { get; private set; }
        
        /// <summary>
        /// Событие обновления статуса
        /// </summary>
        public event Action Update;

        /// <summary>
        /// Обновить информацию по количеству файлов
        /// </summary>
        /// <param name="filesCount">Количество файлов</param>
        public void UpdateFilesCount(int filesCount)
        {
            lock (_lock)
            {
                FilesCount = filesCount;
                OnUpdate();
            }
        }

        /// <summary>
        /// Обновить информацию о ходе обработки файлов
        /// </summary>
        /// <param name="fileName">Имя последнего обработаного файла</param>
        /// <param name="status">Статус процесса</param>
        /// <param name="wordsCount">Общее количество найденых слов</param>
        public void UpdateProgress(string fileName, TaskStatus status, int wordsCount)
        {
            lock (_lock)
            {
                if (status != TaskStatus.Canceled)
                {
                    ProcessedCount++;
                }
                WordsCount = wordsCount;
                LastFileName = fileName;
                if (status == TaskStatus.Faulted)
                {
                    FauledCount++;
                }

                OnUpdate();
            }
        }

        protected virtual void OnUpdate()
        {
            var handler = Update;
            if (handler != null) handler();
        }
    }
}