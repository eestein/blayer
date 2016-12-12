using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    public class Context : DbContext
    {
        protected static Dictionary<Type, IRepository> Repositories;

        public Context() { }

        public Context(Dictionary<Type, IRepository> extRepositories)
            : base("DbConnection")
        {
            Repositories = extRepositories;
        }

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
