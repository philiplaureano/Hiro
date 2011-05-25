/*
 * Created by SharpDevelop.
 * User: jcastro
 * Date: 2011-05-24
 * Time: 4:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SampleAssembly
{
	/// <summary>
	/// This interface is supposed to be implemented in a 'destination' assembly different from this one
	/// </summary>
	public interface IMissing
	{
		string TypeName{ get;}
	}
}
