using System;
using System.IO;

public static class Logger
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

    /// <summary>
    /// Записывает сообщение в лог.
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public static void WriteLog(string message)
    {
        try
        {
            // Форматируем сообщение с текущей датой и временем
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            // Записываем сообщение в файл
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Если возникает ошибка при записи в лог, выводим её в консоль
            Console.WriteLine($"Ошибка записи в лог: {ex.Message}");
        }
    }
}
