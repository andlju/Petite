using System;

namespace Petite
{
    public static class ContainerExtensions
    {
        public static void Register<TService>(this Container container, Func<Container, TService> factoryMethod)
        {
            container.Register(null, new ServiceHandler<TService>(factoryMethod));
        }

        public static void Register<TService>(this Container container, string name, Func<Container, TService> factoryMethod)
        {
            container.Register(name, new ServiceHandler<TService>(factoryMethod));
        }

        public static void RegisterSingleton<TService>(this Container container, Func<Container, TService> factoryMethod)
        {
            container.Register(null, new SingletonServiceHandler<TService>(factoryMethod));
        }

        public static void RegisterSingleton<TService>(this Container container, string name, Func<Container, TService> factoryMethod)
        {
            container.Register(name, new SingletonServiceHandler<TService>(factoryMethod));
        }

        public static TService Resolve<TService>(this Container container)
        {
            return container.Resolve<TService>(null);
        }
    }
}