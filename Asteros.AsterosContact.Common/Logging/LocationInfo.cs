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
        #endregion defs

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
        /// Возвращает или устанавливает всю доступную информацию о коллере,
        /// то есть функции, вызывающей метод записи в журнал логирования
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
                // Это локальные переменные, как если бы это был метод-сеттер.
                string className;
                string methodName;
                string fileName;

                int lineNumber;

                // Если данная проверка не проходит, то в таком случае установка свойств
                // ClassName, MethodName, FileName, LineNumber не происходит.
                if (ParseFullInfo(value, out className, out methodName, out fileName, out lineNumber) == false)
                {
                    return;
                }

                ClassName = className;
                MethodName = methodName;
                FileName = fileName;
                LineNumber = lineNumber;
            }
        }
        #endregion props

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
        #endregion ctors

        /// <summary>
        /// Разбирает указанную строку в инстанс класса <смотри cref="LocationInfo"/>
        /// 
        /// Parses specified string into <see cref="LocationInfo"/>
        /// </summary>
        /// <param name="fullInfo"></param>
        /// <returns>Возвращает местоопределение в коде, где происходит вызов </returns>
        public static LocationInfo FromFullInfo(string fullInfo)
        {
            string className;
            string methodName;
            string fileName;

            int lineNumber;

            return ParseFullInfo(fullInfo, out className, out methodName, out fileName, out lineNumber)
                    ? new LocationInfo(className, methodName, fileName, lineNumber)
                    : null;
        }

        /// <summary>
        /// Разбирает указанную входную строчку (<смотри cref="fullInfo"/>) в поля объекта класса <смотри cref="LocationInfo"/>.
        /// Parses specified input string (<see cref="fullInfo"/>) into <see cref="LocationInfo"/> fields.
        /// </summary>
        /// <param name="fullInfo">Строчка с полной информацией о месте вызова функции, выполняющей запись в журнал логирования.</param>
        /// <param name="className">Имя класса, в котором осуществляется вызов функции, выполняющей запись в журнал логирования.</param>
        /// <param name="methodName">Имя метода, в котором осуществляется вызов функции, выполняющей запись в журнал логирования.</param>
        /// <param name="fileName">Имя файла, в котором осуществляется вызов функции, выполняющей запись в журнал логирования.</param>
        /// <param name="lineNumber">Номер линии кодового файла, в котором осуществляется вызов функции, выполняющей запись в журнал логирования.</param>
        /// <returns>Возвращает логическое значение.</returns>
        public static bool ParseFullInfo(
            string fullInfo,
            out string className,
            out string methodName,
            out string fileName,
            out int lineNumber
            )
        {
            className = null;
            methodName = null;
            fileName = null;
            
            // -1 - значение по умолчанию для линии, в которой вызывается функция записи сообщения в журнал логирования.
            lineNumber = -1;

            if (string.IsNullOrEmpty(fullInfo))
            {
                // Если входная строка пустая или равна null - то нечего синтаксически разбирать. 
                return false;
            }

            // parts1 содержит массив подстрок, которые разделяются символом "(".
            // Если между двумя символами "(" есть пустая строка, как в строке "((", 
            // то такие вхождения в возвращаемый массив - удаляются.
            // Примеры строк, содержащихся в fullInfo.
            var parts1 = fullInfo.Split(new[] { "(" }, StringSplitOptions.RemoveEmptyEntries);
            // Если строка не разделена на две части, не формировать инстанс класса FullInfo.
            if (parts1.Length != 2)
            {
                return false;
            }

            // Левая (половина) часть изначальной строки fullInfo.
            // Срезать слева и справа от левой половины входящие в неё символы открывающей круглой скобки.
            // TODO: Довольно странно, что у левой части срезается открывающая круглая скобка.
            // TODO: Этот код возможно, в зависимости от того, какие строки fileInfo приходят
            // TODO: на вход потребуется пересмотреть.
            var leftPart = parts1[0].Trim('(');
            // Правая (половина) часть изначальной строки fullInfo.
            // Срезать слева и справа от правой половины строки fullInfo символы открывающих и закрывающих скобок.
            // TODO: Довольно странно так же и здесь, что в одной из двух частей строки, которая 
            // TODO: разделяется на части по открывающей скобке, происходит тримминг слева и справа
            // TODO: символа открывающей круглой скобки. Открывающей скобки впринципе не может быть
            // TODO: в правой части. А вот закрывающая округлая скобка в правой части исходной строки
            // TODO: fileInfo наверняка будет. Этот код возможно, в зависимости от того, какие строки
            // TODO: fileInfo приходят на вход потребуется пересмотреть.
            var rightPart = parts1[1].Trim('(', ')');

            // Если левая часть не содержит точки, а правая часть не содержит
            // двоеточия, не формировать инстанс класса FullInfo.
            if (leftPart.Contains(".") == false || rightPart.Contains(":") == false)
            {
                return false;
            }

            // Возвращает целочисленный номер последнего вхождения символа точки, начиная с нуля, в левой части.
            var lastDotIndex = leftPart.LastIndexOf(".", StringComparison.Ordinal);
            // Данный входной параметр является внешним по отношению к текущему статическому методу.
            className = leftPart.Substring(0, lastDotIndex);
            // Данный входной параметр является внешним по отношению к текущему статическому методу.
            methodName = leftPart.Substring(lastDotIndex + 1);

            // Возвращает целочисленный номер последнего вхождения символа двоеточия, начиная с нуля, в правой части.
            var lastSemicolonIndex = rightPart.LastIndexOf(":", StringComparison.Ordinal);
            // Данный входной параметр является внешним по отношению к текущему статическому методу.
            fileName = rightPart.Substring(0, lastSemicolonIndex);

            return int.TryParse(rightPart.Substring(lastSemicolonIndex + 1), out lineNumber);
        }

        // TODO: Как работает данным метод ещё только предстоит выяснить,
        // TODO: когда данное переопределение будет срабатывать в конкретном типе,
        // TODO: с конкретной строкой.
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return $"[{GetType().Name}: {FullInfo}]";
        }
    }
}
