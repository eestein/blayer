using Blayer.Data;

namespace Blayer.Test.Items
{
    /// <summary>
    /// Additional steps to be executed for the entity TestItem
    /// </summary>
    public class TestItemAdditionalSteps : IAdditionalStep
    {
        /// <summary>
        /// Execute additional steps for TestItem
        /// </summary>
        /// <param name="state">Entity's current state</param>
        /// <param name="entityObject">Entity</param>
        /// <param name="originalEntity">Unmodified entity</param>
        public void Execute(System.Data.Entity.EntityState state, object entityObject, object originalEntity)
        {
            TestItem entity = (TestItem)entityObject;
            TestItem dbEntity = originalEntity as TestItem;
            BlayerContext ctx = entity.Context;

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
                        // testing logical removal
                        entity.WillBeDeleted = false;
                        entity.Name = "Rmoved";
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