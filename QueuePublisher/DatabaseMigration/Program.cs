using DbUp;
using System;
using System.Linq;
using System.Reflection;

namespace DatabaseMigration
{
    public class Program
    {
        private static int Main(string[] args)
        {
            var connectionString =
    args.FirstOrDefault()
    ?? "Server=dev-orleans.mysql.database.azure.com;Port=3306;Database=QueueMessages;Uid=thomasNelson@dev-orleans;Pwd=1loveCIS!!!; SslMode = Preferred;";

            var upgrader =
                DeployChanges.To
                    .MySqlDatabase(connectionString) //can change to mssql
                    .WithExecutionTimeout(TimeSpan.FromMinutes(15))
                    .WithExecutionTimeout(TimeSpan.FromMinutes(15))
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}