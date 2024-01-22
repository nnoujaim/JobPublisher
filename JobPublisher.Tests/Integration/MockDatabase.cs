// using Testcontainers;
// using Testcontainers.PostgreSql;
// using Npgsql;


// public sealed class PostgreSqlTest : IAsyncLifetime
// {
//     private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
//         .WithImage("postgres:14.7")
//         .WithDatabase("postgres")
//         .WithUsername("postgres")
//         .WithPassword("postgres")
//         .WithCleanUp(true)
//         .Build();

//     [Fact]
//     public void ExecuteCommand()
//     {
//         using var connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
//         using var command = new NpgsqlCommand();
//         connection.Open();
//         command.Connection = connection;
//         command.CommandText = "SELECT 1";
//         var reader = command.ExecuteReader();
//         var myvar = 1;
//     }

//     public Task InitializeAsync()
//     {
//         return _postgreSqlContainer.StartAsync();
//     }

//     public Task DisposeAsync()
//     {
//         return _postgreSqlContainer.DisposeAsync().AsTask();
//     }
// }