using System;

public class Database : IDatabase
{
	ILogger logger;
	IErrorHandler handler;

	public Database(ILogger logger, IErrorHandler handler)
	{
		this.logger = logger;
		this.handler = handler;
	}

	public ILogger Logger { get { return logger; } }
	public IErrorHandler ErrorHandler { get { return handler; } }

	#region Behavior

	#endregion
}
