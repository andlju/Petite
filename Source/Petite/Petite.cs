using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Petite
{
    /// <summary>
    /// Extension methods to simplify registering and resolving using the Petite container
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///   Register a service
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="factoryMethod">Factory method that should be used to create instances of this service.</param>
        /// <remarks>
        ///   This extension method will use a normal <see>ServiceHandler</see> to create instances.
        /// </remarks>
        public static void Register<TService>(this Container container, Func<Container, TService> factoryMethod)
        {
            container.Register(null, new ServiceHandler<TService>(factoryMethod));
        }

        /// <summary>
        ///   Register a named service
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="name">Name of the service</param>
        /// <param name="factoryMethod">Factory method that should be used to create instances of this service.</param>
        /// <remarks>
        ///   This extension method will use a normal <see>ServiceHandler</see> to create instances.
        /// </remarks>
        public static void Register<TService>(this Container container, string name, Func<Container, TService> factoryMethod)
        {
            container.Register(name, new ServiceHandler<TService>(factoryMethod));
        }

        /// <summary>
        ///   Register a service as Singleton
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="factoryMethod">Factory method that should be used to create an instance of this service.</param>
        /// <remarks>
        ///   This extension method will use a <see>SingletonServiceHandler</see> to create and hold the instance.
        /// </remarks>
        public static void RegisterSingleton<TService>(this Container container, Func<Container, TService> factoryMethod)
        {
            container.Register(null, new SingletonServiceHandler<TService>(factoryMethod));
        }

        /// <summary>
        ///   Register a named service as Singleton
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="name">Name of the service</param>
        /// <param name="factoryMethod">Factory method that should be used to create an instance of this service.</param>
        /// <remarks>
        ///   This extension method will use a <see>SingletonServiceHandler</see> to create and hold the instance.
        /// </remarks>
        public static void RegisterSingleton<TService>(this Container container, string name, Func<Container, TService> factoryMethod)
        {
            container.Register(name, new SingletonServiceHandler<TService>(factoryMethod));
        }

        /// <summary>
        ///   Register a service instance as Singleton
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="instance">The instance to use for this service.</param>
        /// <remarks>
        ///   This extension method will use a <see>InstanceServiceHandler</see> to hold the instance.
        /// </remarks>
        public static void RegisterInstance<TService>(this Container container, TService instance)
        {
            container.Register(null, new InstanceServiceHandler<TService>(instance));
        }

        /// <summary>
        ///   Register a named service instance as Singleton
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="container">Container in which to register the service.</param>
        /// <param name="name">Name of the service</param>
        /// <param name="instance">The instance to use for this service.</param>
        /// <remarks>
        ///   This extension method will use a <see>InstanceServiceHandler</see> to hold the instance.
        /// </remarks>
        public static void RegisterInstance<TService>(this Container container, string name, TService instance)
        {
            container.Register(name, new InstanceServiceHandler<TService>(instance));
        }

        /// <summary>
        /// Resolve an instance of an unnamed service
        /// </summary>
        /// <typeparam name="TService">Type of the service to resolve</typeparam>
        /// <param name="container">Container where the service is registered.</param>
        /// <returns>An instance of type <typeparamref name="TService"/></returns>
        /// <remarks>
        ///   If no matching service has been registered using the <see>Register</see> method,
        ///   an <see>UnknownRegistrationException</see> will be thrown.
        /// </remarks>
        public static TService Resolve<TService>(this Container container)
        {
            return container.Resolve<TService>(null);
        }
    }

    public sealed class Container
    {
        private readonly ConcurrentDictionary<ServiceKey, IServiceHandler> _serviceLookup = new ConcurrentDictionary<ServiceKey, IServiceHandler>();

        /// <summary>
        ///   Core method for registering a service.
        /// </summary>
        /// <typeparam name="TService">Type of the service to register.</typeparam>
        /// <param name="name">Name of the service.</param>
        /// <param name="serviceHandler"><see>ServiceHandler</see> for this service.</param>
        /// <remarks>
        ///   You should probably use one of the extension methods in <see>ContainerExtensions</see>
        ///   when registering services instead. But this one is here in case you need it.
        /// </remarks>
        public void Register<TService>(string name, ServiceHandlerBase<TService> serviceHandler)
        {
            ServiceKey key = GetKey(name, typeof(TService));
            _serviceLookup[key] = serviceHandler;
            serviceHandler.RegisterOwnerInformation(this, name);
        }

        /// <summary>
        ///   Resolve a named service
        /// </summary>
        /// <typeparam name="TService">Type of service to resolve</typeparam>
        /// <param name="name">Name of the service. This can be null if no name was given when registering.</param>
        /// <returns>An instance of type <typeparamref name="TService"/></returns>
        /// <remarks>
        ///   If no matching service has been registered using the <see>Register</see> method,
        ///   an <see>UnknownRegistrationException</see> will be thrown.
        /// </remarks>
        public TService Resolve<TService>(string name)
        {
            ServiceKey key = GetKey(name, typeof(TService));
            IServiceHandler registration;
            if (!_serviceLookup.TryGetValue(key, out registration))
            {
                throw new UnknownRegistrationException(key.Name, key.ServiceType);
            }
            var service = (ServiceHandlerBase<TService>)registration;
            return service.GetInstance();
        }

        private static ServiceKey GetKey(string name, Type serviceType)
        {
            return new ServiceKey(name, serviceType);
        }

        /// <summary>
        ///   Used to create a string for named or unnamed registrations
        /// </summary>
        /// <param name="name">Name of the registration</param>
        /// <param name="serviceType">Type of the registration</param>
        /// <returns>A string to use when displaying a registration</returns>
        public static string CreateKeyString(string name, Type serviceType)
        {
            if (name == null)
                return serviceType.ToString();

            return string.Format("{0} (\"{1}\")", serviceType, name);
        }
    }

    /// <summary>
    ///   Common base class for all ServiceHandlers
    /// </summary>
    /// <typeparam name="TService">Type of the service to handle</typeparam>
    public abstract class ServiceHandlerBase<TService> : IServiceHandler
    {
        protected Container Container { get; private set; }
        protected string Name { get; private set; }

        /// <summary>
        /// Must be called by the Container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        public void RegisterOwnerInformation(Container container, string name)
        {
            if (Container != null)
                throw new InvalidOperationException("Owner already registered.");

            Container = container;
            Name = name;
        }

        /// <summary>
        ///   Implement this to return an instance of the service
        /// </summary>
        /// <returns>An instance of <typeparamref name="TService"/></returns>
        public abstract TService GetInstance();

    }

    /// <summary>
    ///   A "standard" service handler that creates a new instance on every Resolve.
    /// </summary>
    /// <typeparam name="TService">Type of the service to handle</typeparam>
    public class ServiceHandler<TService> : ServiceHandlerBase<TService>
    {
        private readonly Func<Container, TService> _factoryMethod;

        /// <summary>
        ///   Construct a new ServiceHandler
        /// </summary>
        /// <param name="factoryMethod">Factory method that should be used to create instances.</param>
        public ServiceHandler(Func<Container, TService> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        protected TService CreateInstance()
        {
            try
            {
                return _factoryMethod.Invoke(Container);
            }
            catch (Exception ex)
            {
                throw new ResolveException(Name, typeof(TService), ex);
            }
        }

        /// <summary>
        ///   Return a new instance
        /// </summary>
        /// <returns>A new <typeparamref name="TService"/> instance</returns>
        public override TService GetInstance()
        {
            return CreateInstance();
        }
    }

    /// <summary>
    ///   A Singleton service handler that creates one instance and returns it on every resolve.
    /// </summary>
    /// <typeparam name="TService">Type of the service to handle</typeparam>
    public class SingletonServiceHandler<TService> : ServiceHandler<TService>
    {
        private TService _instance;

        /// <summary>
        ///   Construct a new SingletonServiceHandler
        /// </summary>
        /// <param name="factoryMethod">Factory method that should be used to create the instance.</param>
        public SingletonServiceHandler(Func<Container, TService> factoryMethod)
            : base(factoryMethod)
        {
        }

        /// <summary>
        ///   Returns the instance.
        /// </summary>
        /// <returns></returns>
        public override TService GetInstance()
        {
            if (object.Equals(_instance, default(TService)))
                return _instance;

            return _instance = base.GetInstance();
        }
    }

    /// <summary>
    ///   A Singleton service handler that returns a give instance on every resolve.
    /// </summary>
    /// <typeparam name="TService">Type of the service to handle</typeparam>
    public class InstanceServiceHandler<TService> : ServiceHandlerBase<TService>
    {
        private readonly TService _instance;

        /// <summary>
        ///   Construct a new InstanceServiceHandler
        /// </summary>
        /// <param name="instance">The instance to return</param>
        public InstanceServiceHandler(TService instance)
        {
            _instance = instance;
        }

        /// <summary>
        /// Returns the instance
        /// </summary>
        /// <returns></returns>
        public override TService GetInstance()
        {
            return _instance;
        }
    }

    public interface IServiceHandler
    {
        void RegisterOwnerInformation(Container container, string name);
    }

    class ServiceKey
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }

        public ServiceKey(string name, Type serviceType)
        {
            Name = name;
            ServiceType = serviceType;
        }

        public bool Equals(ServiceKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.ServiceType, ServiceType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ServiceKey)) return false;
            return Equals((ServiceKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ ServiceType.GetHashCode();
            }
        }

        public static bool operator ==(ServiceKey left, ServiceKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceKey left, ServiceKey right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Container.CreateKeyString(Name, ServiceType);
        }
    }

    /// <summary>
    ///   Exception thrown when trying to resolve a service that has not been
    ///   registered.
    /// </summary>
    public class UnknownRegistrationException : Exception
    {
        public string Name { get; private set; }
        public Type ServiceType { get; private set; }

        public UnknownRegistrationException(string name, Type serviceType)
            : base(CreateMessage(name, serviceType))
        {
            Name = name;
            ServiceType = serviceType;
        }

        private static string CreateMessage(string name, Type serviceType)
        {
            return string.Format("No registration found for {0}.", Container.CreateKeyString(name, serviceType));
        }
    }

    /// <summary>
    ///   Exception thrown when failing to resolve a service.
    /// </summary>
    public class ResolveException : Exception
    {
        public string Name { get; private set; }
        public Type ServiceType { get; private set; }

        public ResolveException(string name, Type serviceType, Exception innerException)
            : base(CreateMessage(name, serviceType, innerException), innerException)
        {
            Name = name;
            ServiceType = serviceType;
        }

        private static ResolveException GetFirstResolveException(ResolveException resolveException)
        {
            if (resolveException == null)
                return null;

            var nextResolveException = resolveException.InnerException as ResolveException;
            
            return GetFirstResolveException(nextResolveException) ?? resolveException;
        }

        private static string CreateMessage(string name, Type serviceType, Exception innerException)
        {
            var firstResolveException = GetFirstResolveException(innerException as ResolveException);

            if (firstResolveException == null)
            {
                return string.Format("Failed when trying to instatiate {0}", Container.CreateKeyString(name, serviceType));
            }

            return string.Format("Failed when trying to instatiate {0}. Original failure occured at {1}.", Container.CreateKeyString(name, serviceType), Container.CreateKeyString(firstResolveException.Name, firstResolveException.ServiceType));
        }
    }
}