using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Petite
{
	public class PetiteCommonServiceLocator : ServiceLocatorImplBase
	{
		private readonly Container _container;

		public PetiteCommonServiceLocator(Container container)
		{
			_container = container;
		}

		protected override object DoGetInstance(Type serviceType, string key)
		{
			return _container.Resolve(key, serviceType);
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			return _container.ResolveAll(serviceType);
		}
	}
}