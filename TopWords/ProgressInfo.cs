using System;
using System.Threading.Tasks;

namespace TopWordsTestApp
{
    /// <summary>
    /// Progress information
    /// </summary>
    public class ProgressInfo
    {
        private readonly object _lock = new object();

        public int FilesCount { get; private set; } 
        
        public int ProcessedCount { get; private set; }
 
        public int FauledCount { get; private set; } 
        
        public int WordsCount { get; private set; } 
        
        public string LastFileName { get; private set; }
        
        public TaskStatus TaskStatus { get; private set; }
        
        public event Action Update;

        public void UpdateFilesCount(int filesCount)
        {
            lock (_lock)
            {
                FilesCount = filesCount;
                OnUpdate();
            }
        }

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
            Update?.Invoke();
        }
    }
}