namespace Blayer.Data.Utils
{
    /// <summary>
    /// Classe não genérica para copiar propriedades de uma instância para outra de tipos iguais ou diferentes
    /// </summary>
    public static class PropertyCopy
    {
        /// <summary>
        /// Copia as propriedades de mesmo nome de um objeto para outro.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
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
