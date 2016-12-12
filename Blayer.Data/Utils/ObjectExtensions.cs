using System;

namespace Blayer.Data.Utils
{
    /// <summary>
    /// Utilitários para Object
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Invoca um método genérico em um objeto dinamicamente
        /// </summary>
        /// <param name="invokeFrom">Objeto a ter o método invocado</param>
        /// <param name="methodName">Nome do método a ser invocado</param>
        /// <param name="injectType">Tipo</param>
        /// <param name="parameters">Parâmetros</param>
        public static void InvokeGenericMethod(this object invokeFrom, string methodName, Type injectType, params object[] parameters)
        {
            var mi = invokeFrom.GetType().GetMethod(methodName);
            Type[] argTypes = { injectType };
            var result = mi.MakeGenericMethod(argTypes);

            result.Invoke(invokeFrom, parameters);
        }

        /// <summary>
        /// Invoca um método em um objeto dinamicamente
        /// </summary>
        /// <param name="invokeFrom">Objeto a ter o método invocado</param>
        /// <param name="methodName">Nome do método a ser invocado</param>
        /// <param name="parameters">Parâmetros</param>
        public static void InvokeMethod(this object invokeFrom, string methodName, params object[] parameters)
        {
            var mi = invokeFrom.GetType().GetMethod(methodName);
            mi.Invoke(invokeFrom, parameters);
        }
    }
}
