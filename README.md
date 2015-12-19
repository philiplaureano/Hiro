Hiro is an IOC container compiler (currently in development) that will support the following features: 

- Blazingly fast. Unlike other containers, each precompiled Hiro container is written in pure IL, making it very, very vast. Preliminary tests show that it's running at nearly the same speed as native code, and with a few optimizations, it should hit the "magical" 1:1 performance ratio. 

- Statically precompiled. Hiro builds a dependency graph of all the components in your application and compiles an IOC container with all the necessary code to instantiate the dependencies in your application. It uses absolutely no reflection, and it can run on anything that supports the CLR 2.0 

- Tiny. Each compiled container instance typically is less than 5 kilobytes, making it one of the smallest IOC containers ever made. 

- Supports Constructor and Property Injection. Hiro can perform both Constructor and Property injection without any of the speed penalties that are typically associated with other container frameworks that use reflection to inject their dependencies. 
