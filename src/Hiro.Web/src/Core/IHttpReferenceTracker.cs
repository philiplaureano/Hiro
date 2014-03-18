using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a component that tracks objects that are created on a per web request lifetime.
    /// </summary>
    public interface IHttpReferenceTracker
    {
        void SetReference(string key, object value, HttpContext context);
        object GetReference(string key, HttpContext context);
    }
}
