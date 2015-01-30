using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IWebApp
{
	IAuthenticator Authenticator { get; }
	IStockQuote StockQuote { get; }
	void Run();
}