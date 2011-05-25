/*
 * Created by SharpDevelop.
 * User: jcastro
 * Date: 2011-05-24
 * Time: 4:34 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SampleAssembly
{
	//BUG: an assembly with a public delegate will generate a ConstructorNotFoundException at container compile time.
	public delegate void Proc();
}

