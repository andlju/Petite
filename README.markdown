Petite
======
The Petite Dependency Injection Container (or PetiteDic if you so prefer..) is a very light-weight
container that only covers the very basic needs. It is inspired mainly by Funq, but has even less
bells and whistles.

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


And that is pretty much it for now. Feel free to fork and send me a pull request or two!
