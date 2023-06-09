﻿namespace GiantTeam.Logging
{
    public static class Log
    {
        private static ILoggerFactory? _factory;

        /// <summary>
        /// Configure at application start similar to this:
        /// <c>Log.Factory = LoggerFactory.Create(options => options.AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole())</c>
        /// </summary>
        public static ILoggerFactory Factory
        {
            get => _factory ?? throw new NullReferenceException("The Factory has not be set yet. The calling program should set it.");
            set
            {
                if (_factory is null)
                {
                    _factory = value;
                }
                else
                {
                    try
                    {
                        throw new InvalidOperationException("The Factory has already been set. It can only be set once.");
                    }
                    catch (Exception ex)
                    {
                        // To get the call stack.
                        _factory.CreateLogger(typeof(Log)).LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetType(), ex.Message);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "I don't care")]
        public static void Debug<T>(string message, params object?[] args)
        {
            Factory.CreateLogger<T>().LogDebug(message, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "I don't care")]
        public static void Info<T>(string message, params object?[] args)
        {
            Factory.CreateLogger<T>().LogInformation(message, args);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "I don't care")]
        public static void Info(Type type, string message, params object?[] args)
        {
            Factory.CreateLogger(type).LogInformation(message, args);
        }
    }
}
