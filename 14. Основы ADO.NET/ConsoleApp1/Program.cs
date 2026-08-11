global using System.Data;
global using System.Data.Common;
global using System.Diagnostics;
global using Microsoft.Data.SqlClient;
global using Microsoft.Data.Sqlite;
global using Npgsql;


namespace ConsoleApp1
{
    public enum ConnectionType : byte { Sql, Sqlite, Npgsql }
    public static class ConnectionFactory
    {
        private const string SqlConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Encrypt=True";
        private const string SqliteConnectionString = @"Data Source=C:\App.db;";
        private const string NpgsqlConnectionString = @"Host=localhost;Port=5432;Database=master;Username=postgres;Password=secret_pass;";

        public static DbConnection GetConnection(ConnectionType ctype = ConnectionType.Sql, string? connectionString = null)
        {
            if (ctype == ConnectionType.Sql) return new SqlConnection(connectionString ?? SqlConnectionString);
            if (ctype == ConnectionType.Sqlite) return new SqliteConnection(connectionString ?? SqliteConnectionString);
            return new NpgsqlConnection(connectionString ?? NpgsqlConnectionString);
        }
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine($"Method 'Main' started.\n");
            var timer = Stopwatch.StartNew();

            using (var controller = new CancellationTokenSource(30000))
            {
                var tracker = controller.Token;
                using (var connection = (SqlConnection)ConnectionFactory.GetConnection(ConnectionType.Sql))
                {
                    await connection.OpenAsync();
                    Console.WriteLine($"Connection state: {connection.State}");
                    if (connection.State == ConnectionState.Open)
                    {
                        var command = connection.CreateCommand();

                        command.CommandText = @"drop database if exists Test;";
                        await command.ExecuteNonQueryAsync(tracker);

                        command.CommandText = @"create database Test;";
                        await command.ExecuteNonQueryAsync(tracker);

                        command.CommandText = @"Use Test;";
                        await command.ExecuteNonQueryAsync(tracker);

                        command.CommandText = @"drop table if exists Users;";
                        await command.ExecuteNonQueryAsync(tracker);

                        command.CommandText = @"create table Users(id int not null primary key identity(1,1),name nvarchar(200) not null,age int not null);";
                        await command.ExecuteNonQueryAsync(tracker);

                        using (var transaction = await connection.BeginTransactionAsync())
                        {
                            try
                            {
                                command.Transaction = (SqlTransaction)transaction;

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Tom");
                                command.Parameters.AddWithValue("@age", 65);
                                await command.ExecuteNonQueryAsync(tracker);

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Alice");
                                command.Parameters.AddWithValue("@age", 25);
                                await command.ExecuteNonQueryAsync(tracker);

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Bob");
                                command.Parameters.AddWithValue("@age", 31);
                                await command.ExecuteNonQueryAsync(tracker);

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Kate");
                                command.Parameters.AddWithValue("@age", 22);
                                await command.ExecuteNonQueryAsync(tracker);

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Ann");
                                command.Parameters.AddWithValue("@age", 42);
                                await command.ExecuteNonQueryAsync(tracker);

                                command.Parameters.Clear();
                                command.CommandText = @"insert into Users(name,age) values(@name,@age)";
                                command.Parameters.AddWithValue("@name", "Bill");
                                command.Parameters.AddWithValue("@age", 18);
                                await command.ExecuteNonQueryAsync(tracker);

                                await transaction.CommitAsync(tracker);
                            }
                            catch
                            {
                                await transaction.RollbackAsync(tracker);
                                command.Parameters.Clear();
                                throw;
                            }
                        }

                        command.CommandText = @"select count(*) from Users;";
                        var records = await command.ExecuteScalarAsync(tracker);

                        Console.WriteLine($"Records: {records}");

                        command.CommandText = @"select * from Users;";
                        using (var reader = await command.ExecuteReaderAsync(tracker))
                        {
                            var fields = reader.FieldCount;
                            while (await reader.ReadAsync(tracker))
                            {
                                for (int i = 0; i < fields; i++)
                                {
                                    Console.Write($"{reader.GetValue(i)?.ToString()?.PadLeft(12)} |");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }

            timer.Stop();
            Console.WriteLine($"\nMethod 'Main' stopped in {timer.ElapsedMilliseconds} ms.");
        }
    }
}