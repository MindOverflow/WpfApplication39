using System.Collections.Generic;

namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Выполняет операции записи сообщений в различные места назначения.
    /// Performs writing log messages to different destinations
    /// </summary>
    public interface ILogAdapter
    {
        // Свойства.
        // Properties
        #region Properties
        /// <summary>
        /// Возвращает или устанавливает Имя.
        /// Gets or sets Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Адаптер включён или выключен? Переключает триггер. 
        /// Is adapter enable or disable
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Возвращает или устанавливает массив разрешённых атрибутов. 
        /// </summary>
        KeyValuePair<string, string>[] AllowedAttrs { get; set; }

        /// <summary>
        /// Gets or sets <see cref="LogMessageFormatString"/>
        /// </summary>
        LogMessageFormatString MessageFormatString { get; set; }
        #endregion 


        void Initialize(string initXml);

        // Methods
        #region MyRegion
        /// <summary>
        /// Writes <see cref=" logMessage"/> to destination
        /// </summary>
        /// <param name="logMessage"></param>
        void Log(LogMessage logMessage);
        #endregion

    }
}
