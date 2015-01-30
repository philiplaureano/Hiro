using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IStockQuote
{
	ILogger Logger { get; }
	IErrorHandler ErrorHandler { get; }
	IDatabase Database { get; }
}