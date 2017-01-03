using System;

namespace Blayer.Data.Utils
{
    /// <summary>
    /// Object utilities
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Invokes a generic method on a object dynamically
        /// </summary>
        /// <param name="invokeFrom">Object to invoke the method</param>
        /// <param name="methodName">Name of method to be invoked</param>
        /// <param name="injectType">Type</param>
        /// <param name="parameters">Parameters</param>
        public static void InvokeGenericMethod(this object invokeFrom, string methodName, Type injectType, params object[] parameters)
        {
            var mi = invokeFrom.GetType().GetMethod(methodName);
            Type[] argTypes = { injectType };
            var result = mi.MakeGenericMethod(argTypes);

            result.Invoke(invokeFrom, parameters);
        }

        /// <summary>
        /// Invokes a method on a object dynamically
        /// </summary>
        /// <param name="invokeFrom">Object to invoke the method</param>
        /// <param name="methodName">Name of method to be invoked</param>
        /// <param name="parameters">Parameters</param>
        public static void InvokeMethod(this object invokeFrom, string methodName, params object[] parameters)
        {
            var mi = invokeFrom.GetType().GetMethod(methodName);
            mi.Invoke(invokeFrom, parameters);
        }
    }
}
