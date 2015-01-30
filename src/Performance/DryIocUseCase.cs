using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DryIoc;

namespace Performance
{
    public class DryIocUseCase : UseCase
    {
        DryIoc.Container _container;

      


        public DryIocUseCase()
        {
            _container = new DryIoc.Container();
            _container.Register<IWebApp,WebApp>();
     
             _container.Register<IAuthenticator, Authenticator>();
             _container.Register<IStockQuote, StockQuote>();
             _container.Register<IDatabase, Database>();
             _container.Register<IErrorHandler, ErrorHandler>();
             _container.Register<ILogger, Logger>();

 
        }

        public override void Run()
        {
            var webApp = _container.Resolve<IWebApp>();
            webApp.Run();
        }
    }
}