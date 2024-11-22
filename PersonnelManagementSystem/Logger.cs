using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

    /// <summary>
    /// Записывает сообщение в лог.
    /// </summary>
    /// <param name="message">Сообщение для записи.</param>
    public static void WriteLog(string message)
    {
        try
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи в лог: {ex.Message}");
        }
    }
}
