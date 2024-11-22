using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;

public class Database
{
    private readonly string connectionString;

    public Database()
    {
        // Получаем строку подключения из app.config
        connectionString = ConfigurationManager.ConnectionStrings["PersonnelDB"].ConnectionString;
    }

    /// <summary>
    /// Выполняет SELECT-запрос и возвращает DataTable с результатами.
    /// </summary>
    public DataTable ExecuteSelectQuery(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }
    }

    /// <summary>
    /// Выполняет INSERT, UPDATE или DELETE запрос.
    /// </summary>
    public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Выполняет асинхронный SQL-запрос.
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}
