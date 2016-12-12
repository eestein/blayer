using System.Data.Entity;

namespace Blayer.Data
{
    /// <summary>
    /// Validations executed every time an entity is added/updated
    /// </summary>
    public interface IValidate
    {
        void Validate(EntityState state, object entityObject, object originalEntity);
    }
}
