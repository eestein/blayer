using System.Data.Entity;

namespace Blayer.Data
{
    /// <summary>
    /// Allows the repository to modify/create relationships
    /// </summary>
    public interface IModelConfiguration
    {
        void Configure(DbModelBuilder modelBuilder);
    }
}
