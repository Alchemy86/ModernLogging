using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ModernLogging
{
    public class HostedService : IHostedService
    {
        readonly IMyClass _myClass;

        private static readonly IReadOnlyList<(string name, decimal price)> Products = new List<(string, decimal)>
        {
            ("Bread", 1.20m),
            ("Milk", 0.50m),
            ("Rice", 1m),
            ("Buttons", 0.9m),
            ("Pasta", 0.9m),
            ("Cereals", 1.6m),
            ("Chocolate", 2m),
            ("Noodles", 1m),
            ("Pie", 1m),
            ("Sandwich", 1m),
        };

        public HostedService(IMyClass myClass)
        {
            _myClass = myClass;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var products = new List<(string name, decimal price)>();
            Console.WriteLine("Welcome to the Shop");
            Console.WriteLine("Press Q key to exit");
            Console.WriteLine("Press [0..9] key to order some products");
            Console.WriteLine(string.Join(Environment.NewLine, Products.Select((x, i) => $"[{i}]: {x.name} @ {x.price:C}")));

            for (;;)
            {
                var consoleKeyInfo = Console.ReadKey(true);
                if (consoleKeyInfo.Key == ConsoleKey.Q)
                {
                    break;
                }
                
                if (char.IsNumber(consoleKeyInfo.KeyChar))
                {
                    var product = Products[(int)char.GetNumericValue(consoleKeyInfo.KeyChar)];
                    products.Add(product);
                    Console.WriteLine($"Added {product.name}");
                }

                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    foreach (var item in products)
                    {
                        _myClass.MyMethod(item.name);
                    }


                    Console.WriteLine("Submitted Order");
                    _myClass.Speak();

                    products.Clear();
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    // Dummy class and interface for logging example. They themselves contain no logging
    public interface IMyClass
    {   
        Task<object> MyMethod(string param);
        string Speak();
    }
    public class MyClass : IMyClass
    {
        public async Task<object> MyMethod(string param)
        {
            return await Task.FromResult<object>(new { length = param.Length, originalValue =  param});
        }

        public string Speak()
        {
            return "Done! WOOFF!";
        }
    }
}