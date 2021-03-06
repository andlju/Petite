using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Petite.Tests
{
    [TestClass]
    public class ContainerCoreTests
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
        public void Standard_Registration_Returns_An_Instance_On_Resolve()
        {
            var target = new Container();

            target.Register<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolved = target.Resolve<ISimpleTestService>("Name");

            Assert.IsNotNull(resolved);
        }

        [TestMethod]
        public void Singleton_Registration_Returns_Same_Instance_Every_Resolve()
        {
            var target = new Container();

            target.RegisterSingleton<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolvedFirst = target.Resolve<ISimpleTestService>("Name");
            var resolvedSecond = target.Resolve<ISimpleTestService>("Name");

            Assert.AreSame(resolvedFirst, resolvedSecond);
        }

        [TestMethod]
        public void Singleton_Registration_Returns_An_Instance_On_Resolve()
        {
            var target = new Container();

            target.RegisterSingleton<ISimpleTestService>("Name", c => new SimpleTestService());

            var resolved = target.Resolve<ISimpleTestService>("Name");

            Assert.IsNotNull(resolved);
        }

        [TestMethod]
        public void Instance_Registration_Returns_Given_Instance_Every_Resolve()
        {
            var target = new Container();
            var instance = new SimpleTestService();
            target.RegisterInstance<ISimpleTestService>("Name", instance);

            var resolvedFirst = target.Resolve<ISimpleTestService>("Name");
            var resolvedSecond = target.Resolve<ISimpleTestService>("Name");

            Assert.AreSame(instance, resolvedFirst);
            Assert.AreSame(instance, resolvedSecond);
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
            Assert.IsTrue(thrownException.Message.Contains(typeof(ISimpleTestService).ToString()), "Exception should contain the name of the interface that wasn't resolved.");
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
            Assert.IsTrue(thrownException.Message.Contains(typeof(ISimpleTestService).ToString()), "Exception should contain the name of the interface that wasn't resolved.");
            Assert.IsTrue(thrownException.Message.Contains(typeof(ISimpleTestService).ToString()), "Exception should contain the name of the original interface that failed.");
        }


		[TestMethod]
		public void Exception_In_Nested_Factory_Method_Throws_ResolveException_On_NonGenericResolve()
		{
			var target = new Container();

			target.Register<ISimpleTestService>(c => { throw new InvalidOperationException(); });
			target.Register<IServiceWithNestedService>(c => new ServiceWithNestedService(c.Resolve<ISimpleTestService>()));

			ResolveException thrownException = null;
			try
			{
				target.Resolve(typeof(IServiceWithNestedService));
			}
			catch(ResolveException ex)
			{
				thrownException = ex;
			}
			Assert.IsNotNull(thrownException);
			Assert.AreEqual(thrownException.ServiceType, typeof(IServiceWithNestedService));
			Assert.IsTrue(thrownException.Message.Contains(typeof(ISimpleTestService).ToString()), "Exception should contain the name of the interface that wasn't resolved.");
			Assert.IsTrue(thrownException.Message.Contains(typeof(ISimpleTestService).ToString()), "Exception should contain the name of the original interface that failed.");
		}

		[TestMethod]
		public void Non_Generic_Unnamed_Resolve_Returns_Correct_Service()
		{
			var target = new Container();

			target.RegisterSingleton<ISimpleTestService>(c => new SimpleTestService());

			var resolvedFirst = target.Resolve(typeof(ISimpleTestService));

			Assert.IsInstanceOfType(resolvedFirst, typeof(SimpleTestService));
		}

		[TestMethod]
		public void Non_Generic_Unnamed_Unknown_Resolve_Throws_UnknownRegistrationException()
		{
			var target = new Container();

			target.RegisterSingleton<ISimpleTestService>(c => new SimpleTestService());

			UnknownRegistrationException thrownException = null;
			try
			{
				var resolvedFirst = target.Resolve(typeof(IOtherSimpleTestService));
			}
			catch(UnknownRegistrationException ex)
			{
				thrownException = ex;
			}
			Assert.IsNotNull(thrownException);
		}

		[TestMethod]
		public void Non_Generic_Named_Resolve_Returns_Correct_Service()
		{
			var target = new Container();

			target.RegisterSingleton<ISimpleTestService>("Test", c => new SimpleTestService());

			var resolvedFirst = target.Resolve("Test", typeof(ISimpleTestService));

			Assert.IsInstanceOfType(resolvedFirst, typeof(SimpleTestService));
		}

		[TestMethod]
		public void Non_Generic_Named_Unknown_Resolve_Throws_UnknownRegistrationException()
		{
			var target = new Container();

			target.RegisterSingleton<ISimpleTestService>("Test", c => new SimpleTestService());

			UnknownRegistrationException thrownException = null;
			try
			{
				var resolvedFirst = target.Resolve("Unknown", typeof(ISimpleTestService));
			}
			catch(UnknownRegistrationException ex)
			{
				thrownException = ex;
			}
			Assert.IsNotNull(thrownException);
		}

		[TestMethod]
		public void Resolve_All_Returns_All_Named_Items()
		{
			var target = new Container();

			target.Register<ISimpleTestService>("First", c => new SimpleTestService());
			target.Register<ISimpleTestService>("Second", c => new SecondSimpleTestService());

			var allResolved = target.ResolveAll<ISimpleTestService>().ToArray();

			var simpleTestServicesResolved = allResolved.OfType<SimpleTestService>().ToArray();
			var secondSimpleTestServicesResolved = allResolved.OfType<SecondSimpleTestService>().ToArray();

			Assert.AreEqual(2, allResolved.Length);
			Assert.AreEqual(1, simpleTestServicesResolved.Length);
			Assert.AreEqual(1, secondSimpleTestServicesResolved.Length);
		}

		[TestMethod]
		public void Resolve_All_Returns_All_Named_And_UnnamedItems()
		{
			var target = new Container();

			target.Register<ISimpleTestService>(c => new SimpleTestService());
			target.Register<ISimpleTestService>("Second", c => new SecondSimpleTestService());

			var allResolved = target.ResolveAll<ISimpleTestService>().ToArray();

			var simpleTestServicesResolved = allResolved.OfType<SimpleTestService>().ToArray();
			var secondSimpleTestServicesResolved = allResolved.OfType<SecondSimpleTestService>().ToArray();

			Assert.AreEqual(2, allResolved.Length);
			Assert.AreEqual(1, simpleTestServicesResolved.Length);
			Assert.AreEqual(1, secondSimpleTestServicesResolved.Length);
		}


		[TestMethod]
		public void Non_Generic_Named_ResolveAll_Returns_Correct_Services()
		{
			var target = new Container();

			target.Register<ISimpleTestService>(c => new SimpleTestService());
			target.Register<ISimpleTestService>("Second", c => new SecondSimpleTestService());

			var allResolved = target.ResolveAll(typeof(ISimpleTestService)).ToArray();

			var simpleTestServicesResolved = allResolved.OfType<SimpleTestService>().ToArray();
			var secondSimpleTestServicesResolved = allResolved.OfType<SecondSimpleTestService>().ToArray();

			Assert.AreEqual(2, allResolved.Length);
			Assert.AreEqual(1, simpleTestServicesResolved.Length);
			Assert.AreEqual(1, secondSimpleTestServicesResolved.Length);
		}

		/*
		[TestMethod]
		public void Per_Resolve_Registration_Nested_Resolve_Returns_Same_Instance()
		{
			var target = new Container();

			target.RegisterPerResolve<ISimpleTestService>(c => new SimpleTestService());
			target.Register<IServiceWithTwoNestedServices>(c => new ServiceWithTwoNestedServices(c.Resolve<ISimpleTestService>(), c.Resolve<ISimpleTestService>()));

			var resolved = target.Resolve<IServiceWithTwoNestedServices>();

			Assert.AreSame(resolved.FirstNestedService, resolved.SecondNestedService);
		}

		[TestMethod]
		public void Per_Resolve_Registration_Second_Nested_Resolve_Returns_Different_Instance()
		{
			var target = new Container();

			target.RegisterPerResolve<ISimpleTestService>(c => new SimpleTestService());
			target.Register<IServiceWithTwoNestedServices>(c => new ServiceWithTwoNestedServices(c.Resolve<ISimpleTestService>(), c.Resolve<ISimpleTestService>()));

			var resolvedFirst = target.Resolve<IServiceWithTwoNestedServices>();
			var resolvedSecond = target.Resolve<IServiceWithTwoNestedServices>();

			Assert.AreNotSame(resolvedFirst.FirstNestedService, resolvedSecond.FirstNestedService);
			Assert.AreNotSame(resolvedFirst.SecondNestedService, resolvedSecond.SecondNestedService);
		}
		*/
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

	class SecondSimpleTestService : ISimpleTestService
	{
		public SecondSimpleTestService()
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

	interface IServiceWithTwoNestedServices
	{
		ISimpleTestService FirstNestedService { get; }
		ISimpleTestService SecondNestedService { get; }
	}

	class ServiceWithTwoNestedServices : IServiceWithTwoNestedServices
	{
		public ISimpleTestService FirstNestedService { get; private set; }
		public ISimpleTestService SecondNestedService { get; private set; }

		public ServiceWithTwoNestedServices(ISimpleTestService firstNestedService, ISimpleTestService secondNestedService)
		{
			FirstNestedService = firstNestedService;
			SecondNestedService = secondNestedService;
		}
	}
}