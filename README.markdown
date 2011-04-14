Petite
======

The Petite library contains a couple of things I often need when I set up a new project. Two main
parts at the moment, the Petite Container and the Petite Repository base classes.

Petite Container
----------------
The Petite Container is a very light-weight container that only covers the very basic needs. It is
inspired mainly by Funq and Munq, but has even less bells and whistles.

Sample usage:

     var container = new Container();

	 // Simple case
	 container.Register<IMyDependency>(c => new MyDependencyImplementation());

	 // Will get you an instance of MyDependencyImplementation
	 var myDependency = container.Resolve<IMyDependency>();


	 // Using nested dependencies
	 container.Register<IMyService>(
	     c => new MyServiceImplementation(
		           c.Resolve<IMyDependency>()
			  ));
	 
	 // Will get you an instance of MyServiceImplementation (in it's turn 
	 // created with a new instance of MyDependencyImplementation)
     var myService = container.Resolve<IMyService>();

	 // We also support Singletons out-of-the-box
     container.RegisterSingleton<IMySingleton>(c => new MySingletonImplementation());

     // Both of these will be the same instance
	 var mySingleton = container.Resolve<IMySingleton>();
	 var sameSingleton = container.Resolve<IMySingleton>();

	 // You can also register an existing instance as a singleton if you're into that
	 var myInstance = new MyOtherSingletonImplementation();
	 container.RegisterInstance<IMyOtherSingleton>(myInstance);

	 // This will be the same as myInstance
	 var otherSingleton = container.Resolve<IMyOtherSingleton>();

Note that all registration methods seen here are actually extension methods found in the `ContainerExtensions` class,
you therefore need to make sure you are `using Petite;` since they won't be picked up otherwise.

If you need another kind of lifetime manager (or ServiceHandler as they are called in Petite), feel free
to write your own deriving from `ServiceHandlerBase<TService>`. You should then probably create your own extension
methods to handle the registration in the same way as the rest of the overloads are handled.

Also note that it would be quite possible to create a ServiceHandler that *doesn't* use factory methods. If you
feel adventurous, please try to do something different - perhaps it could be released as Petite.Container.AutoRegistration
or something like that.


Petite Repository
-----------------
Petite Repository is divided into two separate files, Domain and Data. The Domain file contains just the IRepository<T>
and the IObjectContext interfaces. The Data file has base class implementations of those interfaces as well as 
an ObjectContext adapter that can be used within a WCF service. 

The Petite Repository is probably best explained with a sample project or two. I'll try to get around to that..

And that is pretty much it for now. Feel free to fork and send me a pull request or two!

TODO
----
 - Test everything in a real-world scenario. (ongoing)
 - Make SingletonServiceHandler thread-safe
 - Compatibility with Silverlight
 - Take over the world
