﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

/// <summary>
/// Класс для хранения строки подключения к базе данных.
/// Строка подключения задаётся через интерфейс приложения.
/// </summary>
public static class DatabaseSettings
{
    public static string ConnectionString { get; set; } = ""; // Строка подключения задаётся через приложение
}


/// <summary>
/// Класс для работы с базой данных.
/// Предоставляет методы для выполнения SQL-запросов.
/// </summary>
public class Database
{
    private readonly string connectionString;

    public Database(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Строка подключения к базе данных не задана. Настройте подключение перед использованием.");
        }

        this.connectionString = connectionString;

        try
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open(); // Тестовое подключение
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Не удалось подключиться к базе данных с указанной строкой подключения. {ex.Message}");
        }
    }

    /// <summary>
    /// Асинхронно выполняет SELECT-запрос и возвращает DataTable с результатами.
    /// </summary>
    public async Task<DataTable> ExecuteSelectQueryAsync(string query, SqlParameter[] parameters = null)
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

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));
                    return dataTable;
                }
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

    /// <summary>
    /// Выполняет SQL-запрос и возвращает первый столбец первой строки результата.
    /// </summary>
    public async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
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

                return await command.ExecuteScalarAsync();
            }
        }
    }

}
