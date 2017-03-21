using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Обеспечивает строковое форматирование для <смотри cref="LogMessage"/>
    /// Provides <see cref="LogMessage"/> to string formatting
    /// </summary>
    public class LogMessageFormatString
    {
        #region default

        // Значение LogMessageFormatString по умолчанию. 
        // Формат записи строки в журнале логирования.
        private static LogMessageFormatString _default;

        /// <summary>
        /// Gets default <see cref="LogMessageFormatString"/>
        /// </summary>
        public static LogMessageFormatString Default
        {
            get
            {
                // Возвратить _default если _default не равен null.
                // Если _default равен null, возвратить выражение в
                // скобках, которое инициализирует значение формата
                // строки записи в журнал логирования по умолчанию. 
                return _default ?? (
                    _default = new LogMessageFormatString(
                        "%property{customDate}" +
                        " [%-5level-%property{messageType}-%property{category}]" +
                        " %message [SOURCE='%property{source}']" +
                        " [TARGET='%property{target}']" +
                        " [Calling method: %property{callingMethod}]" +
                        " %property{*}" +
                        " (%property{shortMessageStackTrace}) %newline")
                        );
            }
        }
        #endregion

        #region defs

        // Словарь, в котором в качестве ключа хранится строковое представление 
        // имени свойств класса LogMessage, все буквы в нижнем регистре.
        // В качестве значения словаря хранится результат возвращаемого свойством
        // объекта.
        private static readonly IDictionary<string, Func<LogMessage, object>> NamedValues =
            new Dictionary<string, Func<LogMessage, object>>();

        private const string NewLinePatternString = "newline";

        // Строка форматирования. 
        // Так как инициализируется в конструкторе, поэтому readonly.
        private readonly string _formatString;

        // TODO: Разобрать смысл листа _formatParameters. Пока он мне не понятен.
        private readonly IList<Func<LogMessage, object>> _formatParameters =
            new List<Func<LogMessage, object>>();

        #endregion

        #region ctors

        // Закидывает в словарь Dictionary<string, Func<LogMessage, object>>()
        // имя свойств в нижнем регистре, как ключ, и возвращаемые ими данные, как значение ключа.
        static LogMessageFormatString()
        {
            // Цикл итерирует тип LogMessage по всем его свойствам, так как используется
            // Метод GetProperties(), возвращающий массив PropertyInfo[].
            foreach (var publicPropInfo in typeof(LogMessage).GetProperties())
            {
                // Возьми для свойства геттер (getter или ещё get-метод свойства).
                var getMethod = publicPropInfo.GetGetMethod();
                // Если get-метод текущего свойства равен null или он не публичный:
                if (getMethod == null || getMethod.IsPublic == false)
                    continue; // Перейти к новой итерации, при выполнении данных условий.

                NamedValues.Add(publicPropInfo.Name.ToLower(), message => getMethod.Invoke(message, null));
            }
        }

        /// <summary>
        /// Инициализирует новый объект класса <смотри cref="LogMessageFormatString"/>,
        /// основываясь на данной, в качестве параметра, строке форматирования <see cref="formatString"/>.
        /// Initializes new instance of <see cref="LogMessageFormatString"/> based on
        /// given <see cref="formatString"/>.
        /// </summary>
        /// <param name="formatString">Строка форматирования.</param>
        public LogMessageFormatString(string formatString)
        {
            _formatString = formatString;

            // Задаём регулярное выражение для поиска по заданному шаблону.
            // TODO: Разобрать регулярное выражение. Не понятно.
            var regex = new Regex("\\%[_0-9a-zA-Z]*\\%");

            // Возвращает коллекцию MatchCollection в которой храняться все совпадения. 
            var matches = regex.Matches(_formatString);

            // Перебрать коллекцию всех совпадений.
            for (var i = 0; i < matches.Count; i++)
            {
                // Возвращает в matchValue значение i-ого совпадения. 
                var matchValue = matches[i].Value;
                // В строке форматирования меняет совпадение на строковое представление целочисленного номера.
                _formatString = _formatString.Replace(matchValue, i.ToString());
                // Взять совпадение регулярного выражения, срезать у него слева
                // и справа символ '%' и все символы обрезанной сторки перевести
                // в нижний регистр.
                var propName = matchValue.Trim('%').ToLower();

                // Если строки раавны, игнорируя регистр:
                if (string.Compare(propName, NewLinePatternString, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    // TODO: Не понятно применение класса Environment и его свойства NewLine.
                    // TODO: За данным вопросом идти сюда:
                    // TODO: https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(System.Environment);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true
                    _formatParameters.Add(message => Environment.NewLine);
                }
                // Если в словаре NamedValues нет ключа propName:
                else if (NamedValues.ContainsKey(propName) == false)
                {
                    _formatParameters.Add(message => "unknown property " + propName);
                }
                // Если в словаре NamedValues есть ключ propName, добавить его значение в лист _formatParameters
                else
                {
                    _formatParameters.Add(NamedValues[propName]);
                }
            }
        }
        #endregion

        public string FormatString
        {
            get
            {
                return _formatString;
            }
        }

        /// <summary>
        /// Форматирует инстанс <сморти cref="LogMessage"/> в строчку
        /// Formats <see cref="LogMessage"/> to string
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public string FormatLogMessage(LogMessage logMessage)
        {
            // Если предоставленный параметр равен null.
            if (logMessage == null)
            {
                Debug.WriteLine("logMessage параметр равен null", $"{GetType().Name}.FormatLogMessage");
                return null;
            }

            // А если всё-таки предоставленный параметр не пустой.
            // Здесь происходит подстановка в форматирующую строчку, _formatString
            // элементов массива, который вычисляется из коллекции _formatParameters.
            // TODO: Не понятно, как срабатывает лямбда-выражение.
            return string.Format(_formatString, _formatParameters.Select(func => func(logMessage)).ToArray());
        }
    }
}
