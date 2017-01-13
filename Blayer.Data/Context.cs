using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    /// <summary>
    /// Modified version of the DbContext class
    /// </summary>
    public class Context : DbContext
    {
        /// <summary>
        /// Repositories added to the context
        /// </summary>
        protected static Dictionary<Type, IRepository> Repositories;

        public Context() { }

        public Context(Dictionary<Type, IRepository> extRepositories, string connectionString)
            : base(connectionString)
        {
            Repositories = extRepositories;
        }

        /// <summary>
        /// Custom model creation to modify data as needed using the IModelConfiguration pattern.
        /// </summary>
        /// <param name="modelBuilder">DbModelBuilder</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            foreach (var entityType in Repositories.Keys)
            {
                modelBuilder.InvokeGenericMethod("Entity", entityType, null);
            }

            base.OnModelCreating(modelBuilder);

            foreach (var item in Repositories.Values)
            {
                var config = item.GetConfiguration();

                config?.Configure(modelBuilder);
            }

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }
    }
}
