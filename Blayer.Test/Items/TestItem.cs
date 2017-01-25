using Blayer.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blayer.Test.Items
{
    [Table("TestItems", Schema = "Blayer")]
    public class TestItem : EntityBase
    {
        /// <summary>
        /// Id
        /// </summary>
        public int TestItemId { get; set; }

        public string Name { get; set; }
    }
}