using System;

namespace Petite
{
    public class ServiceHandler<TService> : IRegistration
    {
        private readonly Func<Container, TService> _factoryMethod;
        private Container _container;

        public ServiceHandler(Func<Container, TService> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public Container Container
        {
            set { _container = value; }
        }

        protected TService CreateInstance()
        {
            try
            {
                return _factoryMethod.Invoke(_container);
            } 
            catch(Exception ex)
            {
                throw new ResolveException(typeof(TService), ex);
            }
        }

        public virtual TService GetInstance()
        {
            return CreateInstance();
        }
    }

    public class SingletonServiceHandler<TService> : ServiceHandler<TService>
    {
        private TService _instance;

        public SingletonServiceHandler(Func<Container, TService> factoryMethod) : base(factoryMethod)
        {
        }

        public override TService GetInstance()
        {
            if (object.Equals(_instance, default(TService)))
                return _instance;

            return _instance = base.GetInstance();
        }
    }
}