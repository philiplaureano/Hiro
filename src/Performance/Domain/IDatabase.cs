﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IDatabase
{
	ILogger Logger { get; }
	IErrorHandler ErrorHandler { get; }
}
