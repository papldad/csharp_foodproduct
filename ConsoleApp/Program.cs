using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Поток {threadId}: Начало работы");

            // 1. Создаем коллекцию из 1000 объектов FoodProduct
            var products = new List<FoodProduct>();
            var random = new Random();
            var currentDate = DateTime.Now;

            for (int i = 0; i < 1000; i++)
            {
                // 10% продуктов с истекшим сроком годности
                bool isExpired = random.Next(0, 10) == 0;
                var expirationDate = isExpired
                    ? currentDate.AddDays(-random.Next(1, 30))
                    : currentDate.AddDays(random.Next(1, 365));

                products.Add(new FoodProduct(
                    id: i + 1,
                    name: $"Какой-то продукт {i + 1}",
                    expirationDate: expirationDate
                ));
            }

            // 2. Создаем сервис и progress-репортер
            var service = new StreamService<FoodProduct>();
            var progress = new Progress<string>(Console.WriteLine);

            // 3. Используем MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                // 4. Синхронно запускаем методы 1 и 2 с задержкой
                var writeTask = service.WriteToStreamAsync(memoryStream, products, progress);
                await Task.Delay(100); // Задержка 100 мс
                var copyTask = service.CopyFromStreamAsync(memoryStream, "products.dat", progress);

                Console.WriteLine($"Поток {threadId}: Потоки 1 и 2 запущены");

                // 6. Ожидаем завершения выполнения методов 1 и 2
                await Task.WhenAll(writeTask, copyTask);
            }

            // 7. Асинхронно получаем статистические данные
            var expiredCount = await service.GetStatisticsAsync(
                "products.dat",
                p => p.ExpirationDate <= DateTime.Now
            );

            // 8. Выводим статистику
            Console.WriteLine($"Количество продуктов с истекшим сроком годности: {expiredCount}");
        }
    }
}