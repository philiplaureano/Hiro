using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IErrorHandler
{
	ILogger Logger { get; }
}
