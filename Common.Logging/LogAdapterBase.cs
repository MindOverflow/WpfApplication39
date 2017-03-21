using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Здесь ммя пространства имён не соответствует проектному пространству имён. 
// Почему так сделано, я пока не понял. Странно, что класс
namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Provides base functionality for logging through.
    /// </summary>
    /// <typeparam name="TInit"></typeparam>
    public abstract class LogAdapterBase<TInit> : ILogAdapter, IInitialize<TInit>
        where TInit : InitializationBase, new()
    {
        #region props

        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public LogMessageFormatString MessageFormatString { get; set; }

        public KeyValuePair<string, string>[] AllowedAttrs { get; set; }

        public TInit Initialization { get; private set; }
        #endregion

        #region ctors

        protected LogAdapterBase()
        {
            Name = null;
            Initialization = new TInit();
            IsEnabled = true;
        }
        #endregion

        public void Initialize(string initXml)
        {
            var initType = typeof(TInit);
            var serializer = new XmlSerializer(initType);
            TInit init = null;
            using (var reader = new XmlTextReader(new System.IO.StringReader(initXml)))
            {
                try
                {
                    init = serializer.Deserialize(reader) as TInit;
                }
                catch (Exception exc)
                {
                    WriteDebugLine(
                        string.Format(
                            "Could not deserialize initialization of type {0} from xml '{1}'. Exception: {2}",
                            initType.AssemblyQualifiedName,
                            initXml,
                            exc
                            ),
                        "LoggingInfrastructure"
                        );
                }
            }

            if (init != null)
            {
                Initialize(init);
            }
            else
            {
                WriteDebugLine(
                        string.Format(
                            "Deserialized Init is null, so no Init method called (init type = '{0}', init xml = '{1}'",
                            initType.AssemblyQualifiedName,
                            initXml
                            ),
                        "LoggingInfrastructure"
                        );
            }
        }

        /// <summary>
        /// Initializes adapter
        /// </summary>
        /// <param name="init">
        /// The init.
        /// </param>
        public virtual void Initialize(TInit init)
        {
            Initialization = init;
            ApplyInitialization();
        }

        /// <summary>
        /// Should be overriden in descendent classes to handle initialization
        /// </summary>
        public virtual void ApplyInitialization()
        {
        }

        /// <summary>
        /// Writes <see cref=" logMessage"/> to destination
        /// </summary>
        /// <param name="logMessage"></param>
        public void Log(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                WriteDebugLine("logMessage is null", "Log");
                return;
            }

            //if (LoggingHelper.IsLevelInside(logMessage.Level, MinLevel, MaxLevel) == false)
            //	return;
            try
            {
                DoLog(logMessage);
            }
            catch (Exception exc)
            {
                WriteDebugLine(exc.ToString(), "Log");
            }
        }



        /// <summary>
        /// Writes <see cref=" logMessage"/> to destination.Should be overriden to handle custom logging.
        /// </summary>
        /// <param name="logMessage">
        /// The log message.
        /// </param>
        protected abstract void DoLog(LogMessage logMessage);

        protected void WriteDebugLine(string message, string methodName = null)
        {
            LoggingInfrastructure.WriteDebugLine(message, LoggingInfrastructure.GetDebugCategory(Name ?? GetType().Name, methodName));
        }

        protected string GetFormattedMessage(LogMessage logMessage)
        {
            var formatter = MessageFormatString ?? LogMessageFormatString.Default;
            return formatter.FormatLogMessage(logMessage);
        }
    }
}
