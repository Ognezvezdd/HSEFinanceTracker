using Spectre.Console;

namespace HSEFinanceTracker.UI
{
    /// <summary>
    /// Предоставляет методы для вывода сообщений в консоль с использованием Spectre.Console.
    /// </summary>
    public static class ConsoleManager
    {
        /// <summary>
        /// Выводит предупреждающее сообщение об ошибке в консоль.
        /// </summary>
        /// <param name="message">Сообщение об ошибке, которое будет выведено.</param>
        /// <param name="flag">
        /// Если <c>true</c>, после вывода сообщения метод ожидает нажатия клавиши.
        /// Если <c>false</c>, ожидание отсутствует.
        /// Значение по умолчанию: <c>true</c>.
        /// </param>
        public static void WriteWarn(string message, bool flag = true)
        {
            var message2 = message.ToString()?.Replace("]", "").Replace("[", "").Replace("\r", "");
            AnsiConsole.Markup($"[red][bold]Ошибка[/]: {message2}[/]\n");
            if (flag)
            {
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Выводит информационное сообщение в консоль зелёным цветом.
        /// </summary>
        /// <param name="message">Сообщение, которое будет выведено.</param>
        public static void WriteMessage(object message)
        {
            var message2 = message.ToString()?.Replace("]", "").Replace("[", "").Replace("\r", "");
            AnsiConsole.Markup($"[green]{message2}[/]\n");
        }

        /// <summary>
        /// Выводит сообщение в консоль с указанным цветом.
        /// </summary>
        /// <param name="message">Сообщение, которое будет выведено. Может быть объектом любого типа.</param>
        /// <param name="color">Строка, задающая цвет (например, "blue", "yellow" и т.д.).</param>
        public static void WriteColor(object message, string color)
        {
            var message2 = message.ToString()?.Replace("]", "").Replace("[", "").Replace("\r", "");
            AnsiConsole.Markup($"[{color}]{message2}[/]\n");
        }
    }
}