using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Petite.Tests
{
    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void Registers_Without_Exception()
        {
            var target = new Container();

            target.Register<ISimpleTestService>(c => new SimpleTestService());
        }

        [TestMethod]
        public void Registered_Interface_Is_Resolved()
        {
            var target = new Container();

            target.Register<ISimpleTestService>(c => new SimpleTestService());

            var resolved = target.Resolve<ISimpleTestService>();

            Assert.IsInstanceOfType(resolved, typeof(SimpleTestService));
        }

        [TestMethod]
        public void Named_Registration_Registers_Without_Exception()
        {
            var target = new Container();

            target.Register<ISimpleTestService>("Name", c => new SimpleTestService());
        }

        [TestMethod]
        public void Named_Registration_Is_Resolved()
        {
            var target = new Container();

            target.Register<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolved = target.Resolve<ISimpleTestService>("Name");

            Assert.IsInstanceOfType(resolved, typeof(SimpleTestService));
        }

        [TestMethod]
        public void Named_Registration_Is_Not_Resolved_Without_Name()
        {
            var target = new Container();

            target.Register<ISimpleTestService>("Name", c => new SimpleTestService());

            Exception exceptionThrown = null;
            try
            {
                var resolved = target.Resolve<ISimpleTestService>();
            }
            catch (UnknownRegistrationException ex)
            {
                exceptionThrown = ex;
            }

            Assert.IsNotNull(exceptionThrown);
        }

        [TestMethod]
        public void Standard_Registration_Returns_New_Instance_Every_Resolve()
        {
            var target = new Container();

            target.Register<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolvedFirst = target.Resolve<ISimpleTestService>("Name");
            var resolvedSecond = target.Resolve<ISimpleTestService>("Name");

            Assert.AreNotSame(resolvedFirst, resolvedSecond);
        }

        [TestMethod]
        public void Singleton_Registration_Returns_New_Instance_Every_Resolve()
        {
            var target = new Container();

            target.RegisterSingleton<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolvedFirst = target.Resolve<ISimpleTestService>("Name");
            var resolvedSecond = target.Resolve<ISimpleTestService>("Name");

            Assert.AreSame(resolvedFirst, resolvedSecond);
        }

        [TestMethod]
        public void Unnamed_Standard_Registration_Returns_New_Instance_Every_Resolve()
        {
            var target = new Container();

            target.Register<ISimpleTestService>(c => new SimpleTestService());

            var resolvedFirst = target.Resolve<ISimpleTestService>();
            var resolvedSecond = target.Resolve<ISimpleTestService>();

            Assert.AreNotSame(resolvedFirst, resolvedSecond);
        }

        [TestMethod]
        public void Unnamed_Singleton_Registration_Returns_New_Instance_Every_Resolve()
        {
            var target = new Container();

            target.RegisterSingleton<ISimpleTestService>(c => new SimpleTestService());

            var resolvedFirst = target.Resolve<ISimpleTestService>();
            var resolvedSecond = target.Resolve<ISimpleTestService>();

            Assert.AreSame(resolvedFirst, resolvedSecond);
        }

        [TestMethod]
        public void Resolve_Sends_Same_Container_To_Factory_Method()
        {
            var target = new Container();

            Container resolveContainer = null;
            target.Register<ISimpleTestService>(c =>
                                                    {
                                                        resolveContainer = c;
                                                        return new SimpleTestService();
                                                    });

            target.Resolve<ISimpleTestService>();

            Assert.IsNotNull(resolveContainer);
            Assert.AreSame(target, resolveContainer);
        }

        [TestMethod]
        public void Resolve_With_Nested_Singleton_Returns_Different_Instances_With_Same_Sub_Instance()
        {
            var target = new Container();

            target.RegisterSingleton<ISimpleTestService>(c=>new SimpleTestService());
            target.Register<IServiceWithNestedService>(c=>new ServiceWithNestedService(c.Resolve<ISimpleTestService>()));

            var resolvedFirst = target.Resolve<IServiceWithNestedService>();
            var resolvedSecond = target.Resolve<IServiceWithNestedService>();

            Assert.AreNotSame(resolvedFirst, resolvedSecond);
            Assert.AreSame(resolvedFirst.NestedService, resolvedSecond.NestedService);
        }

        [TestMethod]
        public void Exception_In_Factory_Method_Throws_ResolveException()
        {
            var target = new Container();

            target.Register<ISimpleTestService>(c => { throw new InvalidOperationException(); });

            ResolveException thrownException = null;
            try
            {
                target.Resolve<ISimpleTestService>();
            } 
            catch(ResolveException ex)
            {
                thrownException = ex;
            }
            
            Assert.IsNotNull(thrownException);
            Assert.AreEqual(thrownException.ServiceType, typeof(ISimpleTestService));
            Assert.AreEqual(thrownException.FirstFailedType, typeof(ISimpleTestService));
        }

        [TestMethod]
        public void Exception_In_Nested_Factory_Method_Throws_ResolveException()
        {
            var target = new Container();

            target.Register<ISimpleTestService>(c => { throw new InvalidOperationException(); });
            target.Register<IServiceWithNestedService>(c => new ServiceWithNestedService(c.Resolve<ISimpleTestService>()));

            ResolveException thrownException = null;
            try
            {
                target.Resolve<IServiceWithNestedService>();
            }
            catch (ResolveException ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.AreEqual(thrownException.ServiceType, typeof(IServiceWithNestedService));
            Assert.AreEqual(thrownException.FirstFailedType, typeof(ISimpleTestService));
        }


    }

    interface ISimpleTestService
    {

    }

    interface IOtherSimpleTestService
    {

    }

    class SimpleTestService : ISimpleTestService
    {
        public SimpleTestService()
        {
        }
    }

    interface IServiceWithNestedService
    {
        ISimpleTestService NestedService { get; }
    }

    class ServiceWithNestedService : IServiceWithNestedService
    {
        public ISimpleTestService NestedService { get; private set; }

        public ServiceWithNestedService(ISimpleTestService nestedService)
        {
            NestedService = nestedService;
        }
    }
}