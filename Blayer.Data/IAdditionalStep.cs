using System.Data.Entity;

namespace Blayer.Data
{
    /// <summary>
    /// Additional steps to be executed every time an entity is added/updated
    /// </summary>
    public interface IAdditionalStep
    {
        void Execute(EntityState state, object entityObject, object originalEntity);
    }
}
