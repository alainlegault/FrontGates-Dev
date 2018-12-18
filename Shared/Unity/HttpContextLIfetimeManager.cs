using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Shared.Unity
{
	/// <summary>
	/// Lifetime manager for Unity containers that persist its information in the http context.
	/// </summary>
	/// <typeparam name="T">The type of the objects that will be affected by the lifetime manager.</typeparam>
	public class HttpContextLifetimeManager<T> : LifetimeManager, IDisposable
	{
		/// <summary>
		/// Extracts a value from the underlying store 
		///</summary>
		/// <returns>The desired object, or null if the object is not present.</returns>
		public override object GetValue()
		{
			if (HttpContext.Current?.Session != null)
				return HttpContext.Current.Session[typeof(T).AssemblyQualifiedName];

			return null;
		}

		/// <summary>
		/// Removes a givent object from the underlying store.
		///</summary>
		public override void RemoveValue()
		{
			if (HttpContext.Current?.Session != null)
				HttpContext.Current.Session.Remove(typeof(T).AssemblyQualifiedName);
		}

		/// <summary>
		/// Stores a given value in the underlying store for a future extraction.
		///</summary>
		/// <param name="newValue">L'objet qui sera entreposé.</param>
		public override void SetValue(object newValue)
		{
			if (HttpContext.Current?.Session != null)
				HttpContext.Current.Session[typeof(T).AssemblyQualifiedName] = newValue;
		}

		/// <summary>
		/// Peforms resources freeing tasks.
		/// </summary>
		public void Dispose()
		{
			RemoveValue();
		}
	}
}
