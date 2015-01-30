using System;

public class WebApp : IWebApp
{
	IAuthenticator authenticator;
	IStockQuote quotes;

	public WebApp(IAuthenticator authenticator, IStockQuote quotes)
	{
		this.authenticator = authenticator;
		this.quotes = quotes;
	}

	public IAuthenticator Authenticator { get { return authenticator; } }
	public IStockQuote StockQuote { get { return quotes; } }

	#region Behavior

	public void Run()
	{
	}

	#endregion
}