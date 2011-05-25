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
	/// This class implements a service, however is intended to be derived from to provide
	/// another constructor signature or explicitly created. It is inteded to have missing 
	/// dependencies	
	/// </summary>
	public class SampleMissingDependency:IMissing
	{
		private System.Type _type;
		/// <summary>
		/// The constructor for the missing dependency
		/// </summary>
		/// <param name="type">The missing dependency</param>
		public SampleMissingDependency(System.Type type)
		{
			_type=type;
		}
		public string TypeName
		{
			get
			{
				return _type.Name;
			}
		}
	}
}
