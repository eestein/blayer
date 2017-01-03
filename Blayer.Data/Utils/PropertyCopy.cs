namespace Blayer.Data.Utils
{
    /// <summary>
    /// Copy properties from one instance to another
    /// </summary>
    public static class PropertyCopy
    {
        /// <summary>
        /// Copy properties with the same name from one object to another
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="destination">Target object</param>
        public static void CopyValues(object source, object destination)
        {
            var destProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                foreach (var destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name &&
                        destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        if (destProperty.CanWrite)
                            destProperty.SetValue(destination, sourceProperty.GetValue(source, new object[] { }), new object[] { });

                        break;
                    }
                }
            }
        }
    }
}
