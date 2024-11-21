using Microsoft.Data.SqlClient;
using System.Reflection;

namespace e_commerce.Infra.Data
{
    public class DatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Inicia a transação
        public async Task BeginTransactionAsync()
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
            _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
        }

        // Confirma a transação
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                _transaction = null;
            }
        }

        // Reverte a transação
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction = null;
            }
        }

        // Método para executar o comando com ou sem transação
        public async Task<int> ExecuteNonQueryAsync(string query, bool useTransaction = false)
        {
            // Se estamos usando transação, usamos a conexão que já foi criada no método BeginTransactionAsync.
            var connectionToUse = useTransaction && _transaction != null ? _connection : new SqlConnection(_connectionString);

            // A conexão só precisa ser aberta se não estamos em uma transação
            if (!useTransaction || _transaction == null)
            {
                await connectionToUse.OpenAsync();
            }

            using (var command = new SqlCommand(query, connectionToUse))
            {
                if (useTransaction && _transaction != null)
                {
                    command.Transaction = _transaction; // Associar transação ao comando
                }

                return await command.ExecuteNonQueryAsync();
            }
        }



        // Método para executar comandos com SqlCommand (com ou sem transação)
        public async Task<int> ExecuteNonQueryAsync(SqlCommand command, bool useTransaction = false)
        {
            if (useTransaction && _transaction != null)
            {
                command.Transaction = _transaction;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                command.Connection = connection;
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
        }

        // Método para executar consultas com ou sem transação
        public async Task<List<T>> ExecuteQueryAsync<T>(string query, bool useTransaction = false) where T : new()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    if (useTransaction && _transaction != null)
                    {
                        command.Transaction = _transaction;
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var result = new List<T>();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        while (await reader.ReadAsync())
                        {
                            var item = new T();
                            foreach (var property in properties)
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                                {
                                    var value = reader[property.Name];
                                    property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                                }
                            }
                            result.Add(item);
                        }
                        return result;
                    }
                }
            }
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(SqlCommand command, bool useTransaction = false) where T : new()
        {
            if (useTransaction && _transaction != null)
            {
                command.Transaction = _transaction;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                command.Connection = connection;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<T>();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    while (await reader.ReadAsync())
                    {
                        var item = new T();
                        foreach (var property in properties)
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                            {
                                var value = reader[property.Name];
                                property.SetValue(item, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                        result.Add(item);
                    }

                    return result;
                }
            }
        }
    }
}
