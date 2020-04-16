# ModernLogging
Dotnet core / Dependency injection based system data logging without modifying existing code.

Credit to the original code this is based on: [Aspect Oriented Programming In C# Using DispatchProxy](https://www.c-sharpcorner.com/article/aspect-oriented-programming-in-c-sharp-using-dispatchproxy/)

The core concept here is to add behaviour to existing code without making changes to the code itself. Logging is the pain of professional development. Too much, not enough, no central point of logic and just flooding code left and right to get some sence of where things get to and whats happening.

### Classic examples are painful 
***
 The key one that bothers me the most? Decorators. The essential duplication of code and implementations to inject and execute the original code inside a logger version of that implementation for example. So same method calls, change one, need to change the other.... nightmare.
***

Whats needed must by dynamic. You cannot be adding more work each time just for logging and I'm sure as hell not flooding my code with line after line of differning information that cannot be maintained.

### DispatchProxy
 
 > Provides a mechanism for instantiating proxy objects and handling their method dispatch.

This is the modern key. Originally in c# I beleive the equivelent is the RealProxy class.
 
This can allow us to execute and mimic any class given. No repeating code. No new addtions per service and method.
The original post referenced is still clunky as hell. Makes no use of dependancy injection etc so still too much work. After much trial and error we have a working implementation that can be applied to anything being injected. ie the things we want to follow.

A good listing example of implementation limitations can be found [Here](https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection)

The ServiceCollectionsExtension provided in this project is the part that wont change. This is just the extension that will allow you to register a proxy, even different ones for different services or layers depening on how you want to do it.

The default example provided is for logging.
With the ***proxylogger.cs*** is the example file that essentually will be logging message before and after the execution of methods. 
This will provide: 
1. Class name
2. Method being executed
3. Paramaters provided
4. Output of execution

All very useful details for debugging. I'd recomed changes for production like logging level and detail, user / tenant etc etc. 

Setup is simple. Anything we want logging for, just add a new line in your services. 

```csharp
// Normal service registration
services.AddSingleton<IMyClass, MyClass>();

// Logging added for the example class
services.Decorate<IMyClass, ProxyLogger<IMyClass>>();
```
Adding the decorate line will now privde logging whenever the methods are used!

```
Added Pasta
Added Cereals
[11:22:01 INF] Invoking:  {"Class":"ModernLogging.MyClass","Method":"MyMethod","paramList":[{"name":"param","type":"String","value":"Pasta"}]} <s:ModernLogging.ProxyLogger>
[11:22:01 INF] Invoking:  {"Class":"ModernLogging.MyClass","Method":"MyMethod","paramList":[{"name":"param","type":"String","value":"Cereals"}]} <s:ModernLogging.ProxyLogger>
[11:22:01 INF] Executed:  {"Class":"ModernLogging.MyClass","Method":"MyMethod","paramList":[{"name":"param","type":"String","value":"Pasta"}]}, Result: {"length":5,"originalValue":"Pasta"} <s:ModernLogging.ProxyLogger>
Submitted Order
[11:22:01 INF] Executed:  {"Class":"ModernLogging.MyClass","Method":"MyMethod","paramList":[{"name":"param","type":"String","value":"Cereals"}]}, Result: {"length":7,"originalValue":"Cereals"} <s:ModernLogging.ProxyLogger>
[11:22:01 INF] Invoking:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]} <s:ModernLogging.ProxyLogger>
[11:22:01 INF] Executed:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]}, Result: "Done! WOOFF!" <s:ModernLogging.ProxyLogger>
Submitted Order
[11:22:03 INF] Invoking:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]} <s:ModernLogging.ProxyLogger>
[11:22:03 INF] Executed:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]}, Result: "Done! WOOFF!" <s:ModernLogging.ProxyLogger>
Submitted Order
[11:22:05 INF] Invoking:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]} <s:ModernLogging.ProxyLogger>
[11:22:05 INF] Executed:  {"Class":"ModernLogging.MyClass","Method":"Speak","paramList":[]}, Result: "Done! WOOFF!" <s:ModernLogging.ProxyLogger>
```

As its using the ILogger these can then be logged anywhere as with your other normal logs and chosen implementation. 

Other ideas for this. Some level of testing, timing the execution of methods for things taking too long!

Again, the great advantage of this is being able to implement something as commonly required as logging without the insane upkeep it typically requires. 
New service? Just resister the dam thing, done. 

You could also take this a step forward and do one call that added a proxy for all registered services! 
Up to you. Its just that flexable. 