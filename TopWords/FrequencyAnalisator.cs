using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TopWordsTestApp
{
    public class FrequencyAnalisator
    {
        /// <summary>
        /// A task that returns the most common words in the specified folder
        /// </summary>
        /// <param name="cancellationTokenSource">CancellationTokenSource</param>
        /// <param name="progressInfo">Progress information</param>
        /// <param name="path">Folder path</param>
        /// <param name="searchInSubfolders">Search in subfolders</param>
        /// <param name="wordsCount">Words count to return</param>
        /// <returns></returns>
        public static Task<IEnumerable<KeyValuePair<string, int>>> GetTopWords(
            CancellationTokenSource cancellationTokenSource, ProgressInfo progressInfo, string path,
            bool searchInSubfolders = false, int wordsCount = 10)
        {
            var result = new Task<IEnumerable<KeyValuePair<string, int>>>(() =>
                {
                    var directory = new DirectoryInfo(path);

                    var files = directory.GetFiles("*.txt",
                                                   searchInSubfolders
                                                       ? SearchOption.AllDirectories
                                                       : SearchOption.TopDirectoryOnly).ToArray();

                    var dictionary = new ConcurrentDictionary<string, int>();
                    progressInfo.UpdateFilesCount(files.Length);
                    var taskFactory = new TaskFactory(cancellationTokenSource.Token,
                                                      TaskCreationOptions.AttachedToParent,
                                                      TaskContinuationOptions.None,
                                                      TaskScheduler.Default);
                    try
                    {
                        var tasks = new List<Task>();
                        foreach (var file in files)
                        {
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            var fileName = file.FullName;
                            var task = taskFactory.StartNew(() => CountWordsInFile(fileName, dictionary));
                            task.ContinueWith(o => progressInfo.UpdateProgress(fileName, o.Status, dictionary.Count));
                            tasks.Add(task);
                        }

                        Task.WaitAll(tasks.ToArray());
                    }
                    catch (AggregateException ex)
                    {
                        ex.Handle(o => o is TaskCanceledException);
                    }
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    return dictionary.OrderByDescending(o => o.Value).Take(wordsCount).ToList();
                }, cancellationTokenSource.Token);
            return result;
        }


        private static void CountWordsInFile(string fileName, ConcurrentDictionary<string, int> dictionary)
        {
            var fileContent = File.ReadAllText(fileName, Encoding.Default);

            var words =
                Regex.Matches(fileContent, "\\w+")
                     .Cast<Match>()
                     .Select(match => match.Value.ToLower())
                     .GroupBy(o => o)
                     .Select(o => new {o.Key, Count = o.Count()});
            foreach (var word in words)
            {
                var w = word;
                dictionary.AddOrUpdate(word.Key, key => w.Count, (key, count) => count + w.Count);
            }
        }
    }
}