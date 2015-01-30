using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hiro;

using Hiro.Containers;
using Hiro.Interfaces;

using Mono.Cecil;

namespace Performance
{
    public class HiroUseCase : UseCase
    {
        IMicroContainer container;



        public HiroUseCase()
        {
            var map = new DependencyMap();
            map.AddService(typeof(IWebApp), typeof(WebApp));
            map.AddService(typeof(IAuthenticator), typeof(Authenticator));
            map.AddService(typeof(IStockQuote), typeof(StockQuote));
            map.AddService(typeof(IDatabase), typeof(Database));
            map.AddService(typeof(IErrorHandler), typeof(ErrorHandler));
            map.AddService(typeof(ILogger), typeof(Logger));

            IContainerCompiler compiler = new ContainerCompiler();
            var assembly = compiler.Compile("Container","CompiledContainer","CompiledContainer",map);
            
            assembly.Write("CompiledContainer.dll");
                
            var loadedAssembly = assembly.ToAssembly();
            
            var containerType = loadedAssembly.GetTypes()[0];
            container = (IMicroContainer)Activator.CreateInstance(containerType);
 
        }

        public override void Run()
        {
   
            var webApp = (IWebApp)container.GetInstance(typeof(IWebApp), null);
            webApp.Run();
        }
    }
}