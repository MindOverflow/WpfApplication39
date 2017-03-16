using System;

namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Место кода в котором происходит запись в журнал.
    /// The internal representation of caller location information.
    /// </summary>
    public class LocationInfo
    {
        #region defs

        // Строковый формат, представляющий полную информацию для записи в журнал логирования.
        // В данном случае в шаблоны будут подставляться значения:
        // {0} - имя класса, в котором происходит вызов функции логирования.
        // {1} - имя метода, в котором происходит вызов функции логирования
        // {2} - имя файла, в котором происходит вызов функции логирования.
        // {3} - номер линии файла, в которой в указанном файле происходит вызов функции логирования.
        // Такая строка будет выглядеть следующим образом:
        // Имя_Класса.Имя_Метода(Имя_Файла:Номер_Линии_Файла)
        // Реальные примеры из журнала логирования:
        private const string FullInfoStringFormat = "{0}.{1}({2}:{3})";
        #endregion

        #region props

        /// <summary>
        /// Возвращает или устанавливает полностью определяющее
        /// имя вызывающего класса, выполняющего запись в журнал логирования.
        /// Имя класса, в котором происходит вызов функции записи в журнал логирования. 
        /// Gets or sets the fully qualified class name of the caller
        /// making the logging request. 
        /// </summary>
        /// <value>
        /// Имя класса.
        /// </value>
        public string ClassName
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает или устанавливает имя метода класса, в
        /// котором производится запись в журнал логирования.
        /// Имя метода, в котором происходит вызов функции записи в журнал логирования.
        /// Gets or sets the method name of the caller.
        /// </summary>
        /// <value>
        /// Имя метода.
        /// The method name.
        /// </value>
        public string MethodName
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает или устанавливает имя файла,
        /// в котором происходит вызов для записи в журнал логирования.
        /// Имя файла, в котором происходит вызов функции записи в журнал логирования. 
        /// Gets or sets the file name of the caller.
        /// </summary>
        /// <value>
        /// Имя файла.
        /// The file name.
        /// </value>
        public string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает или устанавливает номер линии в коде,
        /// где происходит вызов функции, осуществляющей
        /// запись в журнал логирования.
        /// Gets or sets the line number of the caller.
        /// </summary>
        /// <value>
        /// Номер линии, в которой происходит вызов метода,
        /// осуществляющего запись в журнал логирования.
        /// The line number.
        /// </value>
        public int LineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets all available caller information.
        /// </summary>
        /// <value>
        /// The full info.
        /// </value>
        public string FullInfo
        {
            get
            {
                return string.Format(FullInfoStringFormat, ClassName, MethodName, FileName, LineNumber);
            }
            private set
            {
                string className;
                string methodName;
                string fileName;

                int lineNumber;

                if (ParseFullInfo(value, out className, out methodName, out fileName, out lineNumber) == false)
                    return;

                ClassName = className;
                MethodName = methodName;
                FileName = fileName;
                LineNumber = lineNumber;
            }
        }
        #endregion

        #region ctors

        /// <summary>
        /// Конструктор, который инициализирует новый инстанс типа <see cref="LocationInfo"/> с указанными параметрами.
        /// Initializes new instance of <see cref="LocationInfo"/> with specified parameters.
        /// </summary>
        /// <param name="className">
        /// Строковое прдставление имени класса, в котором происходит вызов функции записи в журнал.
        /// </param>
        /// <param name="methodName">
        /// Строковое представление имени метода, в котором происходит вызов функции записи в журнал.
        /// </param>
        /// <param name="fileName">
        /// Строковое представление имени файла, в котором происходит вызов функции записи в журнал.
        /// </param>
        /// <param name="lineNumber">
        /// Целочисленное представление номера линии файла, в которой присходит вызов вункции записи в журнал.
        /// </param>
        public LocationInfo(string className, string methodName, string fileName, int lineNumber)
        {
            // В конструкторе инициализируются все свойства, которые потом используются в строке форматирования диагностического сообщения. 
            ClassName = className;
            FileName = fileName;
            LineNumber = lineNumber;
            MethodName = methodName;
        }

        /// <summary>
        /// Parses specified string into <see cref="LocationInfo"/>
        /// </summary>
        /// <param name="fullInfo"></param>
        /// <returns></returns>
        public static LocationInfo FromFullInfo(string fullInfo)
        {
            string className, methodName, fileName;
            int lineNumber;
            return ParseFullInfo(fullInfo, out className, out methodName, out fileName, out lineNumber)
                    ? new LocationInfo(className, methodName, fileName, lineNumber)
                    : null;
        }

        /// <summary>
        /// Parses specified input string (<see cref="fullInfo"/>) into <see cref="LocationInfo"/> fields
        /// </summary>
        /// <param name="fullInfo"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public static bool ParseFullInfo(string fullInfo, out string className, out string methodName, out string fileName, out int lineNumber)
        {
            className = methodName = fileName = null;
            lineNumber = -1;
            if (string.IsNullOrEmpty(fullInfo))
                return false;

            var parts1 = fullInfo.Split(new[] { "(" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts1.Length != 2)
                return false;

            var leftPart = parts1[0].Trim('(');
            var rightPart = parts1[1].Trim('(', ')');
            if (leftPart.Contains(".") == false || rightPart.Contains(":") == false)
                return false;

            var lastDotIndex = leftPart.LastIndexOf(".");
            className = leftPart.Substring(0, lastDotIndex);
            methodName = leftPart.Substring(lastDotIndex + 1);

            var lastSemicolonIndex = rightPart.LastIndexOf(":");
            fileName = rightPart.Substring(0, lastSemicolonIndex);

            return Int32.TryParse(rightPart.Substring(lastSemicolonIndex + 1), out lineNumber);
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("[{0}: {1}]", GetType().Name, FullInfo);
        }
    }
}
