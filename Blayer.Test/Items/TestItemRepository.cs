using Blayer.Data;

namespace Blayer.Test.Items
{
    public class TestItemRepository : Repository<TestItem>
    {
        public override IAdditionalStep GetAdditionalStep()
        {
            return new TestItemAdditionalSteps();
        }
    }
}