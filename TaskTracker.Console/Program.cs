using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace TaskTracker.app
{
    class Program
    {
        private const string ConnectionString = "Data Source=tasks.db";

        static async Task Main(string[] args)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            CreateTasksTableIfNeeded(connection);

            if (args.Length == 0)
            {
                Console.WriteLine("Por favor, insira um comando.");
                return;
            }

            switch (args[0].ToLower())
            {
                case "add":
                    await AddTask(connection, args);
                    break;
                case "remove":
                    await RemoveTask(connection, args);
                    break;
                case "complete":
                    await CompleteTask(connection, args);
                    break;
                case "view":
                    await ViewTasks(connection);
                    break;
                default:
                    Console.WriteLine("Comando desconhecido.");
                    break;
            }
        }

        static void CreateTasksTableIfNeeded(SqliteConnection connection)
        {
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = 
                @"CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Description TEXT, 
                    IsComplete BOOLEAN)";
            createTableCmd.ExecuteNonQuery();
        }

        static async Task AddTask(SqliteConnection connection, string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Por favor, insira a descrição da tarefa.");
                return;
            }

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Tasks (Description, IsComplete) VALUES ($desc, FALSE)";
            command.Parameters.AddWithValue("$desc", args[1]);
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("Tarefa adicionada com sucesso.");
        }

        static async Task RemoveTask(SqliteConnection connection, string[] args)
        {
            if (args.Length < 2 || !int.TryParse(args[1], out int taskId))
            {
                Console.WriteLine("Por favor, insira um ID de tarefa válido.");
                return;
            }

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Tasks WHERE Id = $id";
            command.Parameters.AddWithValue("$id", taskId);
            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
                Console.WriteLine("Tarefa removida com sucesso.");
            else
                Console.WriteLine("Tarefa não encontrada.");
        }

        static async Task CompleteTask(SqliteConnection connection, string[] args)
        {
            if (args.Length < 2 || !int.TryParse(args[1], out int taskId))
            {
                Console.WriteLine("Por favor, insira um ID de tarefa válido.");
                return;
            }

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tasks SET IsComplete = TRUE WHERE Id = $id";
            command.Parameters.AddWithValue("$id", taskId);
            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
                Console.WriteLine("Tarefa marcada como concluída.");
            else
                Console.WriteLine("Tarefa não encontrada.");
        }

        static async Task ViewTasks(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Description, IsComplete FROM Tasks";

            using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                Console.WriteLine("Não há tarefas.");
                return;
            }

            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var description = reader.GetString(1);
                var isComplete = reader.GetBoolean(2);
                Console.WriteLine($"{id} - {description} - {(isComplete ? "Concluída" : "Não Concluída")}");
            }
        }
    }
}
