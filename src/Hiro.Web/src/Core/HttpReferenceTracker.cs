using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Hiro.Web
{
    public class HttpReferenceTracker : IHttpReferenceTracker
    {
        public void SetReference(string key, object value, HttpContext context)
        {
            context.Items[key] = value;
        }

        public object GetReference(string key, HttpContext context)
        {            
            return context.Items[key];
        }
    }
}
