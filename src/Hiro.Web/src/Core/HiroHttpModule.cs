using System;
using System.Collections;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a custom HttpModule that disposes of any per-web request service instances at the end of every HTTP request.
    /// </summary>
    public class HiroHttpModule : IHttpModule 
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            if (context == null)
                return;

            // Dispose of all Hiro-created instances associated with the request
            foreach (var key in context.Items.Keys)
            {
                var currentKey = key.ToString();
                if (!currentKey.StartsWith("Hiro Service Instance"))
                    continue;

                var currentInstance = context.Items[key] as IDisposable;
                if (currentInstance != null)
                    currentInstance.Dispose();
            }
        }

        public void Dispose()
        {
        }
    }
}
