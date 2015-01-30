using System;

public class StockQuote : IStockQuote
{
	ILogger logger;
	IErrorHandler handler;
	IDatabase database;

	public StockQuote(ILogger logger, IErrorHandler handler, IDatabase database)
	{
		this.logger = logger;
		this.handler = handler;
		this.database = database;
	}

	public ILogger Logger { get { return logger; } }
	public IErrorHandler ErrorHandler { get { return handler; } }
	public IDatabase Database { get { return database; } }

	#region Behavior

	#endregion
}
