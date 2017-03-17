using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Категории записей в журнале логирования логирования.
    /// Так же описывает саму сущность категории.
    /// </summary>
    public struct LoggingCategory
    {
        /// <summary>
        /// Возвращает имя категории записи в журнале логирования.
        /// Gets CategoryName.
        /// </summary>
        /// <value>
        /// Имя категории записи в журнале логирования.
        /// The category name.
        /// </value>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Ининциализирует новый инстанс структуры <смотри cref="LoggingCategory"/>
        /// категории записи в журнал логирования.
        /// Initializes a new instance of the <see cref="LoggingCategory"/> struct.
        /// </summary>
        /// <param name="categoryName">
        /// Строковое представление имени категории записи в журнал логирования. 
        /// The category name.
        /// </param>
        public LoggingCategory(string categoryName)
            : this()
        {
            CategoryName = categoryName;
        }

        /// <summary>
        /// Категория записи в журнал логирования по-умолчанию ("*").
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static readonly LoggingCategory Default = new LoggingCategory("*");

        /// <summary>
        /// Вспомогательная категория для общих случаев когда невозможно определить категорию более конкретно
        /// </summary> 
        public static readonly LoggingCategory InternalStuff = new LoggingCategory("InternalStuff");
        
        /// <summary>
        /// LoginWorkflow category
        /// </summary>
        public static readonly LoggingCategory LoginWorkflow = new LoggingCategory("LoginWorkflow");
    }
}
