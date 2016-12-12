using System.Data.Entity;

namespace Blayer.Data
{
    /// <summary>
    /// Notifications be executed every time after an entity is added/updated
    /// </summary>
    public interface INotify
    {
        void Notify(EntityState state, object entityObject, object originalEntity);
    }
}
