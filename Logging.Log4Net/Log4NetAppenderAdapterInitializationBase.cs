using Common.Logging;

namespace Logging.Log4Net
{
    public class Log4NetAppenderAdapterInitializationBase : InitializationBase
    {
        public string Layout
        {
            get;
            set;
        }

        public Log4NetAppenderAdapterInitializationBase()
        {
            // В конструкторе происходит инициализация свойства Layout
            Layout = "%message%newline";
        }
    }
}
