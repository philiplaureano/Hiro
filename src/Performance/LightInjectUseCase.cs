using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightInject;

namespace Performance
{
    public class LightInjectUseCase : UseCase
    {
        LightInject.ServiceContainer _container;

      


        public LightInjectUseCase()
        {
            _container = new LightInject.ServiceContainer();
            _container.Register<IWebApp,WebApp>();
     
             _container.Register<IAuthenticator, Authenticator>();
             _container.Register<IStockQuote, StockQuote>();
             _container.Register<IDatabase, Database>();
             _container.Register<IErrorHandler, ErrorHandler>();
             _container.Register<ILogger, Logger>();


        }

        public override void Run()
        {
            var webApp = _container.GetInstance<IWebApp>();
            webApp.Run();
        }
    }
}