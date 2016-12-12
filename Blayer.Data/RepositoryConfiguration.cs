using System;
using System.Collections.Generic;
using System.Linq;

namespace Blayer.Data
{
    public abstract class RepositoryConfiguration
    {
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
