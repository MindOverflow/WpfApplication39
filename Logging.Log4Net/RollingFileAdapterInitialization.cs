using log4net.Appender;

namespace Logging.Log4Net
{
    public class RollingFileAdapterInitialization : Log4NetAppenderAdapterInitializationBase
    {
        public string LogFileName
        {
            get;
            set;
        }

        public bool AppendToFile
        {
            get;
            set;
        }

        public bool ImmediateFlush
        {
            get;
            set;
        }

        public int CountDirection
        {
            get;
            set;
        }

        public bool StaticLogFileName
        {
            get;
            set;
        }

        public RollingFileAppender.RollingMode RollingStyle
        {
            get;
            set;
        }

        public int MaxSizeRollBackups
        {
            get;
            set;
        }

        public string MaximumFileSize
        {
            get;
            set;
        }

        public string DatePattern
        {
            get;
            set;
        }

        // В какой файл писать логи.
        public RollingFileAdapterInitialization()
        {
            // В конструкторе происходит инициализация всех свойств, кроме
            // свойства MaximumFileSize.
            LogFileName = @"${LOCALAPPDATA}\Logs\log.txt";
            AppendToFile = true;
            CountDirection = -1;
            MaxSizeRollBackups = 10;
            StaticLogFileName = true;
            ImmediateFlush = true;
            // Данному свойству присваивается такое значение, которое
            // задаётся для того, чтобы "выкатывать" файлы логирования 
            // единожды, при выполнении программы.
            RollingStyle = RollingFileAppender.RollingMode.Once;
            DatePattern = ".yyyy-MM-dd";
        }
    }
}
