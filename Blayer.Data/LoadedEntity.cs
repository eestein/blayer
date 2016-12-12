using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Blayer.Data
{
    /// <summary>
    /// Entity loaded to the database
    /// </summary>
    public class LoadedEntity
    {
        /// <summary>
        /// Entry
        /// </summary>
        public DbEntityEntry Entry { get; set; }

        /// <summary>
        /// Original/unmodified entity
        /// </summary>
        public object OriginalEntity { get; set; }

        /// <summary>
        /// Original state
        /// </summary>
        public EntityState OriginalState { get; set; }
    }
}
