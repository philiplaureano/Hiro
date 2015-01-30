using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Funq;

namespace Performance
{
	public class FunqUseCase : UseCase
	{
		Container container;

		public FunqUseCase()
		{
			container = new Funq.Container { DefaultReuse = ReuseScope.None };
			container.Register<IWebApp>(
				c => new WebApp(
					c.Resolve<IAuthenticator>(),
					c.Resolve<IStockQuote>()));

			container.Register<IAuthenticator>(
				c => new Authenticator(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>(),
					c.Resolve<IDatabase>()));

			container.Register<IStockQuote>(
				c => new StockQuote(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>(),
					c.Resolve<IDatabase>()));

			container.Register<IDatabase>(
				c => new Database(
					c.Resolve<ILogger>(),
					c.Resolve<IErrorHandler>()));

			container.Register<IErrorHandler>(
				c => new ErrorHandler(c.Resolve<ILogger>()));

			container.Register<ILogger>(c => new Logger());

	
		}

		public override void Run()
		{
			var webApp = container.Resolve<IWebApp>();
			webApp.Run();
		}
	}
}
