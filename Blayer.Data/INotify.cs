using System.Data.Entity;

namespace Blayer.Data
{
    public interface INotify
    {
        void Notify(EntityState state, object entityObject, object originalEntity);
    }
}
