using System;
using System.Linq;
using Blayer.Data;
using Blayer.Test.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blayer.Test
{
    [TestClass]
    public class DeleteTest
    {
        [TestMethod]
        public void RemoveItem()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            TestItem newItem;

            using (var context = new BlayerContext(new TestsConfiguration()))
            {
                newItem = context.Add(new TestItem
                {
                    Name = "Test"
                });

                context.Save(false);
            }

            using (var context = new BlayerContext(new TestsConfiguration()))
            {
                var lItem = context.GetRepository<TestItem>().GetById(newItem.TestItemId);
                context.Delete(lItem);
                context.Save(false);
            }

            using (var context = new BlayerContext(new TestsConfiguration()))
            {
                var items = context.GetRepository<TestItem>().GetAll();
                var x = items.FirstOrDefault();
            }
        }
    }
}
