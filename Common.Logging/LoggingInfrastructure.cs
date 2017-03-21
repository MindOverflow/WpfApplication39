using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Common access point for logging infrastructure.
    /// </summary>
    public class LoggingInfrastructure
    {
        #region defs

        public const string ConfigurationSectionName = @"Asteros.AsterosContact.Common.Logging";
        public const string DebugCategoryPrefix = "Asteros.AsterosContact.Common.LoggingInfrastructure";
        private const int TryCountDeleteFile = 10;
        private const int AttemptsWhaitingTime = 100;

        private readonly IList<ILogger> _loggers = new List<ILogger>();

        public LogSettingsElement LoggingSettings { get; private set; }

        public static readonly IDictionary<string, Type> PredefinedLogAdapters =
            new Dictionary<string, Type>
            {
                {
                    "debug",
                    typeof(LogAdapters.DebugLogAdapter)
                    },
                {
                    "trace",
                    typeof(LogAdapters.TraceLogAdapter)
                    },
            };

        private ILogger _defaultLogger;
        #endregion

        #region props

        /// <summary>
        /// Gets value, indicating whether logging infrastructure is in debug mode
        /// </summary>
        public static bool IsDebug { get; private set; }

        /// <summary>
        /// Gets list of registered loggers
        /// </summary>
        public IEnumerable<ILogger> Loggers
        {
            get { return _loggers; }
        }

        /// <summary>
        /// Returns logger with empty <see cref="ILogger.Name"/>
        /// </summary>
        /// <returns></returns>
        public static ILogger DefaultLogger
        {
            get { return Instance.DefaultLoggerInstance; }
        }

        /// <summary>
        /// Returns logger with empty <see cref="ILogger.Name"/>
        /// </summary>
        /// <returns></returns>
        private ILogger DefaultLoggerInstance
        {
            get
            {
                if (_defaultLogger != null)
                    return _defaultLogger;

                _defaultLogger = Loggers.FirstOrDefault(logger => String.IsNullOrEmpty(logger.Name));
                if (_defaultLogger == null)
                {
                    _defaultLogger = new Logger();
                    _loggers.Add(_defaultLogger);
                }

                return _defaultLogger;
            }
        }

        private static List<string> _tempLogFiles;
        #endregion

        #region singletone

        private static LoggingInfrastructure _instance;
        public static LoggingInfrastructure Instance
        {
            get { return _instance ?? (_instance = new LoggingInfrastructure()); }
        }

        private LoggingInfrastructure()
        {
            _tempLogFiles = new List<string>();
            ReadConfiguration();
        }

        /// <summary>
        /// Sets credentials
        /// </summary>
        /// <param name="credentials">
        /// The credentials.
        /// </param>
        public void SetCredentials(Credentials credentials)
        {
            foreach (var credentialsAdapter in Loggers.SelectMany(logger => logger.Adapters.OfType<ICredentialsAdapter>()))
            {
                credentialsAdapter.SetCredentials(credentials);
            }
        }

        #endregion

        #region reading config

        /// <summary>
        /// Чтение конфигурации логирования
        /// </summary>
        private void ReadConfiguration()
        {
            LoggingInfrastructureSection loggingInfrastructureConfigurationSection = null;
            try
            {
                loggingInfrastructureConfigurationSection = ConfigurationManager.GetSection(ConfigurationSectionName) as LoggingInfrastructureSection;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            if (loggingInfrastructureConfigurationSection == null)
            {
                loggingInfrastructureConfigurationSection = new LoggingInfrastructureSection();
                loggingInfrastructureConfigurationSection.Loggers = new LoggerElementCollection();
                loggingInfrastructureConfigurationSection.Listeners = new ListenerElementCollection();
                loggingInfrastructureConfigurationSection.Loggers.Add(new LoggerElement());
            }

            IsDebug = loggingInfrastructureConfigurationSection.Debug;

            var newLogger = ReadConfiguration(loggingInfrastructureConfigurationSection);
            if (newLogger != null)
            {
                AddLogger(newLogger);
            }

        }


        /// <summary>
        /// Чтение секции конфигурации логирования
        /// </summary>
        /// <param name="sectionConfig">Секция логгирования из конфигурации</param>
        /// <returns></returns>
        private static ILogger ReadConfiguration(LoggingInfrastructureSection sectionConfig)
        {
            var listenerElements = sectionConfig.Listeners.OfType<ListenerElement>().EmptyIfNull().ToList();
            var loggers = sectionConfig.Loggers.OfType<LoggerElement>().EmptyIfNull().Where(el => el != null).ToList();
            if (!loggers.Any())
            {
                var newLogger = new Logger();
                ReadListeners(newLogger, listenerElements);
                return newLogger;
            }

            foreach (var loggerElement in loggers)
            {
                var newLogger = CreateLogger(loggerElement) ?? new Logger();

                ReadListeners(newLogger, listenerElements);
                return newLogger;

            }
            return new Logger();
        }

        /// <summary>
        /// Метод выполняет мердж настроек из сервиса и локальных
        /// - Заменяет листенер в локальнх настройках при совпаденнни
        /// - Добавляет те листенеры которых нет в локальных настройках
        /// - Заменяет логальный список атрибутов для скрытия на список из сервиса
        /// </summary>
        /// <param name="sectionFromLoadedConfig">Секция настроек десериализованная из строки из бд</param>
        /// <param name="logger">Логгер</param>
        public static void ReconfigureLogging(LoggingInfrastructureSection sectionFromLoadedConfig, ILogger logger)
        {
            var localListeners = logger.Listeners.ToList();

            //ToDo: Костыль, продумать как лучше подменять файлы для записи логов
            try
            {
                foreach (var localListener in localListeners)
                {
                    foreach (var adapter in localListener.Adapters.Where(adapter => adapter is IFileLogAdapter))
                    {
                        var localPath = ((IFileLogAdapter)adapter).FilePath;
                        localPath = ((IFileLogAdapter)adapter).ReplaceEnvironmentVariable(localPath); //Преобразуем путь если указаны переменные
                        if (File.Exists(localPath))
                        {
                            var firstLogs = File.ReadAllText(localPath, Encoding.UTF8);

                            //Поскольку имя файла в конфигурации из бд может быть таким же, то записываем все первоначальные логи во временные файлы
                            var tempPath = localPath + ".tmp";
                            File.WriteAllText(tempPath, firstLogs, Encoding.UTF8);
                            _tempLogFiles.Add(tempPath); //Копируем логи которые были уже записаны для адаптеров локально

                            var listenerElements = sectionFromLoadedConfig.Listeners.OfType<ListenerElement>().EmptyIfNull().ToList();
                            foreach (var listenerElement in listenerElements)
                            {
                                //Берем те листенеры которые будут заменены на листенеры конфигурации из бд, 
                                //Закрываем адаптеры(для записи в файл), удаляем их, чтобы при пересоздании с такиж именем для листенеров из бд не создавался дополнительный файл 
                                //Для адаптеров из бд которые не будут перезаписаны, скопированы логи для вставки		
                                if (listenerElement.Name != localListener.Name)
                                {
                                    continue;
                                }

                                ((IFileLogAdapter)adapter).CloseAppender();

                                for (var i = 0; i < TryCountDeleteFile; i++)
                                {
                                    try
                                    {
                                        File.Delete(localPath);
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                        Thread.Sleep(AttemptsWhaitingTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorDev(ex, new LoggingCategory("LoggingConfiguration"), "Error in reconfiguration method", null, null);
            }

            var loggerFromAdminConfiguration = ReadConfiguration(sectionFromLoadedConfig);
            if (loggerFromAdminConfiguration == null)
            {
                throw new Exception("Не удалось загрузить конфигурацию");
            }


            foreach (var listener in loggerFromAdminConfiguration.Listeners)
            {
                if (localListeners.Any(x => String.Equals(x.Name, listener.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var localListener = localListeners.FirstOrDefault(
                        x => String.Equals(x.Name, listener.Name, StringComparison.CurrentCultureIgnoreCase));
                    localListeners.Remove(localListener);
                }
                localListeners.Add(listener);
                logger.AddListener(listener);
            }

            foreach (var adapter in logger.Listeners.SelectMany(x => x.Adapters.Where(adapter => adapter is IFileLogAdapter)))
            {
                ((IFileLogAdapter)adapter).IsFirstFlushDone += IsFirstFlushDone;
            }

            logger.AttributesToHide = loggerFromAdminConfiguration.AttributesToHide;
        }

        //При первой записи в файл (для адаптера который загружен из бд) копируем логи из файлов локальной конфигурации
        private static void IsFirstFlushDone(object sender, EventArgs eventArgs)
        {
            try
            {
                var adapter = (IFileLogAdapter)sender;
                foreach (var defaultFile in _tempLogFiles)
                {
                    if (File.Exists(defaultFile))
                    {
                        var firstLogs = File.ReadAllText(defaultFile);
                        adapter.AppendTextToFile(firstLogs);
                        File.Delete(defaultFile);
                    }
                }

                adapter.IsFirstFlushDone -= IsFirstFlushDone;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Создаем логгер из конфигурации
        /// </summary>
        /// <param name="configuration">Элемент конфигурации логгер</param>
        /// <returns></returns>
        private static ILogger CreateLogger(LoggerElement configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ILogger result;
            if (String.IsNullOrEmpty(configuration.Type))
            {
                result = new Logger();
            }
            else
            {
                result = CreateObject(
                                configuration.Type,
                                type => type.IsAbstract == false && type.IsPublic && type.GetInterface(typeof(ILogger).Name) != null
                            ) as ILogger;
            }

            if (result != null)
            {
                result.Name = configuration.Name;
                var attrsToHide = new List<AttributeToHide>();
                foreach (var attrsList in configuration
                    .Settings
                    .AttributesListsToHide
                    .OfType<AttributeListElement>()
                    .EmptyIfNull())
                {
                    if (attrsList.IsEnabled == false) continue;

                    var attrs = attrsList.Attributes.OfType<AttributeFilterElement>().ToList();
                    attrsToHide.AddRange(
                        attrs.Select(itemElement => new AttributeToHide()
                        {
                            Message = attrsList.MessageForReplace,
                            Name = itemElement.Name,
                            Value = itemElement.Value,
                            SearchType = itemElement.SearchType,
                            Regex = itemElement.Pattern
                        }
                    ));
                }
                result.AttributesToHide = attrsToHide.ToArray();
            }
            return result;
        }

        /// <summary>
        /// Чтение листнеров из конфигурации и запись в указанный логгер
        /// </summary>
        /// <param name="logger">Целевой логгер</param>
        /// <param name="listeners">Коллекция элементов листенеров из конфигурации</param>
        private static void ReadListeners(ILogger logger, IEnumerable<ListenerElement> listeners)
        {
            var listenersFromConfg = new List<Listener>();
            foreach (var listener in listeners)
            {
                var newListener = new Listener() { Name = listener.Name };

                foreach (var filter in listener.Filters.Cast<FilterElementBase>())
                {
                    newListener.AddFilter(filter);
                }

                foreach (var adapterElement in listener.Adapters.OfType<LogAdapterElement>().Where(el => el != null))
                {
                    if (adapterElement.IsEnabled == false) //не создаем адаптер если он выключен
                        continue;

                    var newAdapter = CreateAdapter(adapterElement);
                    if (newAdapter != null)
                    {
                        newListener.AddAdapter(newAdapter);
                    }
                }

                listenersFromConfg.Add(newListener);
                logger.AddListener(newListener);
            }
        }


        public static ILogAdapter CreateAdapter(LogAdapterElement configuration)
        {
            if (configuration == null)
            {
                return null;
            }

            var adapterTypeString = configuration.Type;

            ILogAdapter result;
            var predefinedType = PredefinedLogAdapters
                .FirstOrDefault(kvp => String.Compare(kvp.Key, adapterTypeString, StringComparison.InvariantCultureIgnoreCase) == 0)
                .Value;
            if (predefinedType != null)
            {
                try
                {
                    result = Activator.CreateInstance(predefinedType) as ILogAdapter;
                }
                catch (Exception exc)
                {
                    WriteDebugLine(
                        String.Format(
                            "Could not create instance of predefined type {0}: {1}",
                            PredefinedLogAdapters[adapterTypeString].Name,
                            exc
                            ),
                        LoggingConfiguration.GetDebugCategory(typeof(LoggingInfrastructure).Name)
                        );
                    return null;
                }
            }
            else
            {
                result = CreateObject(
                    configuration.Type,
                    type => type.IsAbstract == false && type.IsPublic && type.GetInterface(typeof(ILogAdapter).Name) != null
                    ) as ILogAdapter;
            }

            if (result == null)
            {
                return null;
            }



            #region setting simple ILogAdapter props

            result.Name = configuration.Name;
            result.IsEnabled = configuration.IsEnabled;
            result.MessageFormatString = string.IsNullOrEmpty(configuration.MessageFormatString)
                ? LogMessageFormatString.Default
                : new LogMessageFormatString(configuration.MessageFormatString);
            #endregion

            #region IPacketLogAdapter stuff

            var resultWithPackageLogging = result as IPacketLogAdapter;
            if (resultWithPackageLogging != null)
            {
                if (configuration.PackSize > 0)
                {
                    resultWithPackageLogging.PackSize = configuration.PackSize;
                }

                if (configuration.SendPeriod > 0)
                {
                    resultWithPackageLogging.SendPeriod = configuration.SendPeriod;
                }
            }
            #endregion

            if (configuration.Initialization != null && configuration.Initialization.InitializationXml != null)
            {
                result.Initialize(configuration.Initialization.InitializationXml.OuterXml);
            }

            return result;
        }

        private static object CreateObject(string assemblyQualifiedTypeName, Predicate<Type> typePredicate = null)
        {
            var foundType = Type.GetType(assemblyQualifiedTypeName);

            if (foundType == null)
            {
                WriteDebugLine(
                    String.Format("Type NOT FOUND forassembly qualified name='{0}'", assemblyQualifiedTypeName),
                    LoggingConfiguration.GetDebugCategory("LoggingInfrastructure", "CreateObject")
                    );
                return null;
            }

            if (typePredicate != null && typePredicate(foundType) == false)
                return null;

            try
            {
                return Activator.CreateInstance(foundType);
            }
            catch (Exception exc)
            {
                WriteDebugLine(
                    exc.ToString(),
                    LoggingConfiguration.GetDebugCategory("LoggingInfrastructure", "CreateObject")
                    );
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Registers specified logger into infrastructure
        /// </summary>
        /// <param name="loggerToAdd"></param>
        public void AddLogger(ILogger loggerToAdd)
        {
            if (loggerToAdd == null)
            {
                throw new ArgumentNullException("loggerToAdd");
            }

            if (_loggers.Contains(loggerToAdd) || _loggers.Any(ad => ad.Name == loggerToAdd.Name))
            {
                return;
            }

            _loggers.Add(loggerToAdd);
        }

        /// <summary>
        /// Deregisters specified logger from infrastructure
        /// </summary>
        /// <param name="loggerToRemove"></param>
        public void RemoveLogger(ILogger loggerToRemove)
        {
            if (loggerToRemove == null)
            {
                throw new ArgumentNullException("loggerToRemove");
            }

            if (_loggers.Contains(loggerToRemove) == false)
            {
                return;
            }

            _loggers.Remove(loggerToRemove);
        }

        public static string GetDebugCategory(object debugObject, string methodName = null)
        {
            if (debugObject == null)
            {
                return DebugCategoryPrefix;
            }

            var result = string.Format("{0}: {1}", DebugCategoryPrefix, debugObject);
            if (string.IsNullOrEmpty(methodName) == false)
            {
                result += "." + methodName;
            }

            return result;
        }

        public static void WriteDebugLine(string message, string category)
        {
            if (IsDebug == false)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine(message, category);
        }
    }
}
