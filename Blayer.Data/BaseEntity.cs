using System.ComponentModel.DataAnnotations.Schema;

namespace Blayer.Data
{
    /// <summary>
    /// Base class for database entities
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Current context
        /// </summary>
        [NotMapped]
        public AppContext Context { get; set; }

        /// <summary>
        /// Whether it is marked for removal
        /// </summary>
        [NotMapped]
        public bool WillBeDeleted { get; set; }
    }
}
