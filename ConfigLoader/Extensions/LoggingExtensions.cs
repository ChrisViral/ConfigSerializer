using System;
using UnityEngine;
using Object = UnityEngine.Object;

/* ConfigLoader is distributed under CC BY-NC-SA 4.0 INTL (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * You are free to redistribute, share, adapt, etc. as long as the original author stupid_chris (Christophe Savard) is properly,
 * clearly, and explicitly credited, that you do not use this material to a commercial use, and that you share-alike. */

namespace ConfigLoader.Extensions;

internal static class LoggingExtensions
{
    #region Unity Objects
    /// <summary>
    /// Logs the given message
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Message to log</param>
    public static void Log(this Object obj, object message)
    {
        Debug.Log($"[{obj.GetType().Name}]: {message}", obj);
    }

    /// <summary>
    /// Logs the given warning
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Warning to log</param>
    public static void LogWarning(this Object obj, object message)
    {
        Debug.LogWarning($"[{obj.GetType().Name}]: {message}", obj);
    }

    /// <summary>
    /// Logs the given error
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Error to log</param>
    public static void LogError(this Object obj, object message)
    {
        Debug.LogError($"[{obj.GetType().Name}]: {message}", obj);
    }

    /// <summary>
    /// Logs the given exception
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Error message to log</param>
    /// <param name="e">Exception to log</param>
    public static void LogException(this Object obj, object message, Exception e)
    {
        Debug.LogError($"[{obj.GetType().Name}]: {message}\n{e.GetType().Name}: {e.Message}\n{e.StackTrace}", obj);
    }
    #endregion

    #region Generic
    /// <summary>
    /// Logs the given message
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Message to log</param>
    public static void Log<T>(this T obj, object message)
    {
        Debug.Log($"[{typeof(T).Name}]: {message}");
    }

    /// <summary>
    /// Logs the given warning
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Warning to log</param>
    public static void LogWarning<T>(this T obj, object message)
    {
        Debug.LogWarning($"[{typeof(T).Name}]: {message}");
    }

    /// <summary>
    /// Logs the given error
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Error to log</param>
    public static void LogError<T>(this T obj, object message)
    {
        Debug.LogError($"[{typeof(T).Name}]: {message}");
    }

    /// <summary>
    /// Logs the given exception
    /// </summary>
    /// <param name="obj">Logging object</param>
    /// <param name="message">Error message to log</param>
    /// <param name="e">Exception to log</param>
    public static void LogException<T>(this T obj, object message, Exception e)
    {
        Debug.LogError($"[{typeof(T).Name}]: {message}\n{e.GetType().Name}: {e.Message}\n{e.StackTrace}");
    }
    #endregion
}
