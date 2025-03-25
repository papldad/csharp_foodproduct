using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class StreamService<T>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task WriteToStreamAsync(Stream stream, IEnumerable<T> data, IProgress<string> progress)
        {
            await _semaphore.WaitAsync();
            try
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                progress?.Report($"Поток {threadId}: Начало записи в поток");

                var formatter = new BinaryFormatter();
                int count = 0;
                int total = 0;

                if (data is ICollection<T> collection)
                {
                    total = collection.Count;
                }
                else
                {
                    total = 1;
                    foreach (var item in data) total++;
                }

                foreach (var item in data)
                {
                    formatter.Serialize(stream, item);
                    count++;
                    var percentage = (int)((double)count / total * 100);
                    progress?.Report($"Поток {threadId}: {percentage}% записано");
                    await Task.Delay(30); // Задержка для имитации долгой записи
                }

                progress?.Report($"Поток {threadId}: Запись в поток завершена");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CopyFromStreamAsync(Stream stream, string fileName, IProgress<string> progress)
        {
            await _semaphore.WaitAsync();
            try
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                progress?.Report($"Поток {threadId}: Начало копирования из потока в файл");

                stream.Position = 0; // Возвращаемся в начало потока для чтения

                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[4096];
                    int bytesRead;
                    long totalBytes = stream.Length;
                    long totalRead = 0;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;
                        var percentage = (int)((double)totalRead / totalBytes * 100);
                        progress?.Report($"Поток {threadId}: {percentage}% скопировано");
                        await Task.Delay(10); // Задержка для имитации долгого копирования
                    }
                }

                progress?.Report($"Поток {threadId}: Копирование из потока в файл завершено");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> GetStatisticsAsync(string fileName, Func<T, bool> filter)
        {
            int count = 0;
            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    var item = (T)formatter.Deserialize(fileStream);
                    if (filter(item))
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}