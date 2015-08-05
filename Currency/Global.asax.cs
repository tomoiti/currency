using Currency.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Currency
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CurrencyDbContext>());
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
