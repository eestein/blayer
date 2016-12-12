using System.Data.Entity;

namespace Blayer.Data
{
    public interface IModelConfiguration
    {
        void Configure(DbModelBuilder modelBuilder);
    }
}
