using System;
using System.Collections.Generic;

namespace Petite
{
    public interface IRegistration
    {
        Container Container { set; }
    }

    public class Container
    {
        private readonly Dictionary<ServiceKey, IRegistration> _serviceLookup = new Dictionary<ServiceKey, IRegistration>();

        public void Register<TService>(string name, ServiceHandler<TService> service)
        {
            ServiceKey key = GetKey(name, typeof(TService));
            _serviceLookup[key] = service;
            service.Container = this;
        }

        public TService Resolve<TService>(string name)
        {
            ServiceKey key = GetKey(name, typeof(TService));
            IRegistration registration;
            if (!_serviceLookup.TryGetValue(key, out registration))
            {
                throw new UnknownRegistrationException(key);
            }
            var service = (ServiceHandler<TService>)registration;
            return service.GetInstance();
        }

        private static ServiceKey GetKey(string name, Type serviceType)
        {
            return new ServiceKey(name, serviceType);
        }
    }

    public class UnknownRegistrationException : Exception
    {
        public ServiceKey Key { get; private set; }

        public UnknownRegistrationException(ServiceKey key)
            : base(CreateMessage(key))
        {
            Key = key;
        }

        private static string CreateMessage(ServiceKey key)
        {
            if (key.Name == null)
            {
                return string.Format("No unnamed registration of type '{0}' was found.", key.ServiceType);
            }

            return string.Format("No registration with the name '{0}' and type '{1}' was found.", key.Name, key.ServiceType);
        }
    }

    public class ResolveException : Exception
    {
        public Type ServiceType { get; private set; }

        public ResolveException(Type serviceType, Exception innerException)
            : base(CreateMessage(serviceType, innerException), innerException)
        {
            ServiceType = serviceType;
        }

        public Type FirstFailedType
        {
            get { return GetFirstFailedType(ServiceType, InnerException); }
        }

        private static Type GetFirstFailedType(Type key, Exception innerException)
        {
            var innerResolveException = innerException as ResolveException;
            if (innerResolveException != null)
            {
                return innerResolveException.FirstFailedType;
            }

            return key;
        }

        private static string CreateMessage(Type serviceType, Exception innerException)
        {
            var firstFailedType = GetFirstFailedType(serviceType, innerException);

            if (firstFailedType == serviceType)
            {
                return string.Format("Failed when trying to instatiate {0}", serviceType);
            }
            else
            {
                return string.Format("Failed when trying to instatiate {0}, Original failure occured at {1}.", serviceType, firstFailedType);
            }
        }
    }
}