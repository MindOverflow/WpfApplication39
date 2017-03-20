using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Log4Net
{
    public class Log4NetAppenderAdapterBase<TInit, TAppender> : LogAdapterBase<TInit>
        where TInit : Log4NetAppenderAdapterInitializationBase, new()
        where TAppender : class, IAppender
    {
        #region defs

        private TAppender _appender;
        #endregion

        #region props

        protected TAppender Appender
        {
            get { return _appender ?? (_appender = GetAppender()); }
        }
        #endregion

        #region creating appender

        private TAppender GetAppender()
        {
            return CreateAppender();
        }

        protected virtual TAppender CreateAppender()
        {
            try
            {
                return Activator.CreateInstance<TAppender>();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc, "Coud not create log4net appender.");
                return null;
            }
        }

        protected virtual void PrepareAppender(TAppender appender)
        {
            if (appender == null)
            {
                return;
            }

            appender.Name = Name;
            var appenderSkeleton = appender as AppenderSkeleton;
            if (appenderSkeleton != null)
            {
                appenderSkeleton.Layout = new PatternLayout(Initialization.Layout ?? "%message%newline");
            }
        }
        #endregion

        protected override void DoLog(LogMessage logMessage)
        {
            if (logMessage == null)
            {
                WriteDebugLine("logMessage is null", "DoLog");
                return;
            }

            if (Appender == null)
            {
                WriteDebugLine("log4net appender is null", "DoLog");
                return;
            }

            var log4NetLevel = Level.Debug;
            switch (logMessage.Level)
            {
                case LogLevel.Off:
                    log4NetLevel = Level.Off;
                    break;
                case LogLevel.Info:
                    log4NetLevel = Level.Info;
                    break;
                case LogLevel.Debug:
                    log4NetLevel = Level.Debug;
                    break;
                case LogLevel.Warn:
                    log4NetLevel = Level.Warn;
                    break;
                case LogLevel.Error:
                    log4NetLevel = Level.Error;
                    break;
                case LogLevel.Fatal:
                    log4NetLevel = Level.Fatal;
                    break;
            }
            var propertiesDictionary = new PropertiesDictionary();

            propertiesDictionary["logId"] = logMessage.Id.ToString();
            propertiesDictionary["target"] = logMessage.Target != null ? logMessage.Target.ToString() : "null";
            propertiesDictionary["source"] = logMessage.Source;
            propertiesDictionary["levelValue"] = logMessage.LevelValue;
            propertiesDictionary["userName"] = logMessage.UserName;
            propertiesDictionary["machineName"] = Environment.MachineName;
            propertiesDictionary["category"] = logMessage.Category;
            propertiesDictionary["messageType"] = logMessage.MessageType.ToString();

            if (logMessage.Location != null)
            {
                propertiesDictionary["shortMessageStackTrace"] = logMessage.Location.ClassName.Split('\\').Last() + ":" + logMessage.Location.LineNumber;
            }

            propertiesDictionary["*"] = "";
            foreach (var attribute in logMessage.Attributes.EmptyIfNull())
            {
                if (string.IsNullOrEmpty(attribute.Key)) continue;

                propertiesDictionary[attribute.Key] = attribute.Value != null ? attribute.Value.ToString() : string.Empty;

                propertiesDictionary["*"] += String.Format("<{0}: {1}>", attribute.Key, attribute.Value);
            }


            var initialization = Initialization as RollingFileAdapterInitialization;
            if (initialization != null)
            {
                propertiesDictionary["customDate"] = logMessage.Time.ToString(initialization.DatePattern, CultureInfo.InvariantCulture);
            }
            propertiesDictionary["taskId"] = (object)logMessage.TaskId ?? string.Empty;


            var eventData = new LoggingEventData
            {
                Level = log4NetLevel,
                TimeStamp = logMessage.Time,
                ExceptionString = logMessage.ExceptionString,
                ThreadName = logMessage.ManagedThreadId.ToString(),
                Properties = propertiesDictionary,
            };

            eventData.Message = logMessage.Message;

            if (logMessage.Location != null)
            {
                eventData.LocationInfo = new log4net.Core.LocationInfo(
                    logMessage.Location.ClassName,
                    logMessage.Location.MethodName,
                    logMessage.Location.FileName,
                    logMessage.Location.LineNumber.ToString()
                    );
            }

            try
            {
                Appender.DoAppend(new LoggingEvent(eventData));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc, String.Format("{0}.DoLog", GetType().Name));
            }
        }


        public override void ApplyInitialization()
        {
            base.ApplyInitialization();

            try
            {
                PrepareAppender(Appender);
                var appenderSkeleton = Appender as AppenderSkeleton;
                if (appenderSkeleton != null)
                    appenderSkeleton.ActivateOptions();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc, String.Format("{0}.ApplyInitialization", GetType().Name));
            }
        }
    }
}
