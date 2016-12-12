using Blayer.Data;

namespace $rootnamespace$
{
    /// <summary>
    /// Additional steps to be executed for the entity $fileinputname$
    /// </summary>
    public class $fileinputname$AdditionalSteps : IAdditionalStep
    {
        /// <summary>
        /// Execute additional steps for $fileinputname$
        /// </summary>
        /// <param name="state">Entity's current state</param>
        /// <param name="entityObject">Entity</param>
        /// <param name="originalEntity">Unmodified entity</param>
        public void Execute(System.Data.Entity.EntityState state, object entityObject, object originalEntity)
        {
            $fileinputname$ entity = ($fileinputname$)entityObject;
            $fileinputname$ dbEntity = originalEntity as $fileinputname$;
            AppContext ctx = entity.Context;

            switch (state)
            {
                    // If the entity was added (data creation)
                case System.Data.Entity.EntityState.Added:
                    {

                    }
                    break;
                    // If the entity was removed (data removal)
                case System.Data.Entity.EntityState.Deleted:
                    {

                    }
                    break;
                    // If the entity was updated (data update)
                case System.Data.Entity.EntityState.Modified:
                    {

                    }
                    break;
                default:
                    break;
            }
        }
    }
}