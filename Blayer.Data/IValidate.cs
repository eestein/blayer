using System.Data.Entity;

namespace Blayer.Data
{
    public interface IValidate
    {
        void Validate(EntityState state, object entityObject, object originalEntity);
    }
}
