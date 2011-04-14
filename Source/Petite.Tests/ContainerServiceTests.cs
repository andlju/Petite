using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Petite.Tests
{
    [TestClass]
    public class ContainerServiceTests
    {
        [TestMethod]
        public void ServiceKey_Equals_Same_ServiceKey()
        {
            
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));

            Assert.IsTrue(target1.Equals(target2));
        }

        [TestMethod]
        public void ServiceKey_Operator_Equals_Same_ServiceKey()
        {
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));

            Assert.IsTrue(target1 == target2);
        }

        [TestMethod]
        public void ServiceKey_Equals_Same_ServiceKey_With_Null_Name()
        {
            var target1 = new ServiceKey_Accessor(null, typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor(null, typeof(ISimpleTestService));

            Assert.IsTrue(target1.Equals(target2));
        }

        [TestMethod]
        public void ServiceKey_Operator_Equals_Same_ServiceKey_With_Null_Name()
        {
            var target1 = new ServiceKey_Accessor(null, typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor(null, typeof(ISimpleTestService));

            Assert.IsTrue(target1 == target2);
        }

        [TestMethod]
        public void ServiceKey_Not_Equals_Differently_Named_ServiceKey()
        {
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("DifferentName", typeof(ISimpleTestService));

            Assert.IsFalse(target1.Equals(target2));
        }

        [TestMethod]
        public void ServiceKey_Not_Operator_Equals_Differently_Named_ServiceKey()
        {
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("DifferentName", typeof(ISimpleTestService));

            Assert.IsFalse(target1 == target2);
        }

        [TestMethod]
        public void ServiceKey_Not_Equals_Differently_Typed_ServiceKey()
        {
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("Name", typeof(IOtherSimpleTestService));

            Assert.IsFalse(target1.Equals(target2));
        }

        [TestMethod]
        public void ServiceKey_Not_Operator_Equals_Differently_Typed_ServiceKey()
        {
            var target1 = new ServiceKey_Accessor("Name", typeof(ISimpleTestService));
            var target2 = new ServiceKey_Accessor("Name", typeof(IOtherSimpleTestService));

            Assert.IsFalse(target1 == target2);
        }
    }
}