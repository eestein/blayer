using System;

namespace Blayer.Data.Utils
{
    /// <summary>
    /// Business exception
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// Type of exception
        /// </summary>
        public BusinessExceptionType ExceptionType { get; private set; }

        /// <summary>
        /// Creates a business exception
        /// </summary>
        /// <param name="message">Message to be shown to the user</param>
        /// <param name="exceptionType">Exception type</param>
        public BusinessException(string message, BusinessExceptionType exceptionType = BusinessExceptionType.Default)
            : base(message)
        {
            ExceptionType = exceptionType;
        }
    }
}
