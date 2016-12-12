using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Blayer.Data
{
    public class LoadedEntity
    {
        public DbEntityEntry Entry { get; set; }
        public object OriginalEntity { get; set; }
        public EntityState OriginalState { get; set; }
    }
}
