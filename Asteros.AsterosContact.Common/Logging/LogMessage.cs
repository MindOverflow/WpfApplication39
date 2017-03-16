using System;
using System.Collections.Generic;

//  Asteros.AsterosContact.Common - название проекта C#
// Logging - название папки в проекте Asteros.AsterosContact.Common
namespace Asteros.AsterosContact.Common.Logging
{
    // Класс представляет собой сообщение (запись) журнала.
    /// <summary>
	/// Представляет сообщение (запись) журнала.
	/// </summary>
    public sealed class LogMessage
    {
        #region Props
        public Guid Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает уровень сложности логирования.
        /// </summary>
        /// <value>
        /// Уровень сложности логирования.
        /// </value>
        public LogLevel Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает тип сообщения в журнале логирования.
        /// </summary>
        /// <value>
        /// Тип сообщения в журнале логирования.
        /// </value>
        public LogMessageType MessageType
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает время типа DateTime.
        /// </summary>
        /// <value>
        /// Отпечаток времени по Гринвичу.
        /// </value>
        public DateTime Time
        {
            get;
            private set;
        }

        /// <summary>
        /// Время создания лога в UTC со смещением по часовому поясу компьютера-источника записи.
        /// </summary>
        public DateTimeOffset UtcTimeWithOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает сообщение, записываемое в журнал.
        /// </summary>
        /// <value>
        /// Сообщение, записываемое в журнал.
        /// </value>
        public string Message
        {
            get;
            private set;
        }


        /// <summary>
        /// Запись со строкой исключения.
        /// </summary>
        /// <value>
        /// Текст ошибки-исключения.
        /// </value>
        public string ExceptionString
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает место, где происходит запись.
        /// </summary>
        /// <value>
        /// Место кода в котором происходит запись в журнал.
        /// </value>
        public LocationInfo Location
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает имя машины, на которой производится запись.
        /// </summary>
        /// <value>
        /// Строка с именем машины, на которой происходит запись.
        /// </value>
        public string MachineName
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает строку с именем пользователя.
        /// </summary>
        /// <value>
        /// Строка с именем пользователя.
        /// </value>
        public string UserName
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает или устанавливает величину уровня логирования.
        /// </summary>
        public int LevelValue
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает категорию логирования.
        /// </summary>
        /// <value>
        /// Категория логирования.
        /// </value>
        public string Category
        {
            get;
            private set;
        }

        public string Source
        {
            get;
            set;
        }

        public string Target
        {
            get;
            set;
        }


        public int ManagedThreadId
        {
            get;
            private set;
        }

        public int? TaskId
        {
            get;
            private set;
        }

        public Guid? WorkSessionId
        {
            get;
            set;
        }

        /// <summary>
        /// Пользовательские аттрибуты журнальной записи. 
        /// </summary>
        public KeyValuePair<string, string>[] Attributes
        {
            get;
            private set;
        }
        #endregion Props

        #region ctors

        /// <summary>
        /// Создаёт новый инстанс класса "Запись журнала".
        /// </summary>
        /// <param name="level">Уровень сложности логирования.</param>
        /// <param name="type">Тип сообщения в журнале логирования.</param>
        /// <param name="time">Отпечаток времени по Гринвичу.</param>
        /// <param name="message">Сообщение, записываемое в журнал.</param>
        /// <param name="exceptionText">Текст ошибки-исключения.</param>
        /// <param name="target"></param>
        /// <param name="machineName">Строка с именем машины, на которой происходит запись.</param>
        /// <param name="location">Место кода в котором происходит запись в журнал.</param>
        /// <param name="userName">Строка с именем пользователя.</param>
        /// <param name="category">Категория логирования.</param>
        /// <param name="managedThreadId"></param>
        /// <param name="taskId"></param>
        /// <param name="attributes">Пользовательские аттрибуты журнальной записи</param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static LogMessage Create(
            LogLevel level,
            LogMessageType type,
            DateTime time,
            string message,
            string exceptionText = null,
            string source = null,
            string target = null,
            string machineName = null,
            LocationInfo location = null,
            string userName = null,
            string category = "*",
            int managedThreadId = 0,
            int? taskId = null,
            params KeyValuePair<string, string>[] attributes)
        {

            return new LogMessage
            {
                Id = Guid.NewGuid(),
                Level = level,
                MessageType = type,
                Time = time,
                UtcTimeWithOffset = DateTime.SpecifyKind(time.ToUniversalTime(), DateTimeKind.Local),
                Message = message,
                ExceptionString = exceptionText,
                MachineName = machineName,
                Location = location,
                UserName = userName,
                LevelValue = (int)level,
                Category = category,
                ManagedThreadId = managedThreadId,
                TaskId = taskId,
                Attributes = attributes,
                Source = source,
                Target = target,
            };
        }
        #endregion ctors
    }
}
