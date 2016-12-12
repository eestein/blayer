using System;
using System.Collections.Generic;
using System.Linq;

namespace Blayer.Data
{
    /// <summary>
    /// Base class for defining repositories
    /// </summary>
    public abstract class RepositoryConfiguration
    {
        /// <summary>
        /// Returns all repositories defined in this
        /// </summary>
        /// <returns>All repositories here defined</returns>
        public IEnumerable<IRepository> GetRepositories()
        {
            var repositories = GetType().GetProperties()
                .Where(p => p.PropertyType.GetInterface("IRepository") != null)
                .Select(p => p.PropertyType)
                .Concat(GetType()
                    .GetFields()
                    .Where(f => f.FieldType.GetInterface("IRepository") != null)
                    .Select(f => f.FieldType))
                .Select(Activator.CreateInstance)
                .Cast<IRepository>();

            return repositories;
        }
    }
}
