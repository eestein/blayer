using Blayer.Data;

namespace Blayer.Test.Items
{
    public class TestsConfiguration : RepositoryConfiguration
    {
        public TestsConfiguration()
        {
            ConnectionString = "FindSheepContext";
        }

        public TestItemRepository TestItemRepository;
    }
}
