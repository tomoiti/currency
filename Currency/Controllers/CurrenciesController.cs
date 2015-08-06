using Currency.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Currency.Controllers
{
    /// <summary>
    /// Currencies API Controller
    /// </summary>
    public class CurrenciesController : ApiController
    {
        private CurrencyDbContext db = new CurrencyDbContext();

        // GET: api/Currencies
        /// <summary>
        /// Get All Known Currencies
        /// </summary>
        /// <returns>List of Currencies</returns>
        public IQueryable<Models.Currency> GetCurrencies()
        {
            return db.Currencies;
        }

        // GET: api/Currencies/5
        /// <summary>
        /// Get a specific currency
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Requested Currency</returns>
        /// <response code="200" type="Currency">Requested Currency</response>
        /// <response code="404" type="void">Currency Not Found</response>
        [ResponseType(typeof(Models.Currency))]
        public IHttpActionResult GetCurrency(long id)
        {
            Models.Currency currency = db.Currencies.Find(id);
            if (currency == null)
            {
                return NotFound();
            }

            return Ok(currency);
        }

        /// <summary>
        /// Convert amount from a currency to another
        /// </summary>
        /// <param name="from">From currency</param>
        /// <param name="to">To currency</param>
        /// <param name="amount">Amount to be converted</param>
        /// <returns>Converted Amount</returns>
        /// <response code="200" type="Currency">Converted Amount</response>
        /// <response code="404" type="void">One of the currencies cannot be found</response>
        [ResponseType(typeof(decimal))]
        [HttpGet]
        [Route("api/Currencies/{from}/{to}/{amount}")]
        public IHttpActionResult ConvertCurrency(string from, string to, decimal amount)
        {
            var fromCurrency = db.Currencies.SingleOrDefault(c => c.Code == from);
            var toCurrency = db.Currencies.SingleOrDefault(c => c.Code == to);
            if (fromCurrency == null || toCurrency == null)
            {
                return NotFound();
            }
            var result = amount * (toCurrency.Rate / fromCurrency.Rate);
            return Ok(result);
        }

        /// <summary>
        /// Sync List of Currencies with OpenExchangeRates.org
        /// </summary>
        /// <returns>List of Currencies</returns>
        [HttpPost]
        [Route("api/Currencies/Sync")]
        public IQueryable<Models.Currency> ImportCurrencies()
        {
            Trace.TraceInformation("Updating Exchange Rates");
            try
            {
                using (var context = new CurrencyDbContext())
                {
                    var channel = (HttpWebRequest)WebRequest.Create("https://openexchangerates.org/api/currencies.json?app_id=246a8007294a4dae9eb553f35969b09d");
                    var response = channel.GetResponse();
                    var sr = new StreamReader(response.GetResponseStream());
                    string responseString = sr.ReadToEnd();
                    sr.Close();

                    var currencies = JObject.Parse(responseString);
                    foreach (var currency in currencies)
                    {
                        var code = currency.Key;
                        var name = currency.Value.ToString();
                        var entity = context.Currencies.SingleOrDefault(c => c.Code == code);
                        if (entity == null)
                        {
                            context.Currencies.Add(new Models.Currency() { Code = code, Name = name });
                        }
                    }
                    context.SaveChanges();

                    channel = (HttpWebRequest)WebRequest.Create("https://openexchangerates.org/api/latest.json?app_id=246a8007294a4dae9eb553f35969b09d");
                    response = channel.GetResponse();
                    sr = new StreamReader(response.GetResponseStream());
                    responseString = sr.ReadToEnd();
                    sr.Close();

                    currencies = JObject.Parse(responseString);
                    foreach (JProperty currencyRates in currencies["rates"])
                    {
                        var code = currencyRates.Name;
                        var val = (JValue)currencyRates.Value;
                        var rate = decimal.Parse(val.ToString());
                        var entity = context.Currencies.SingleOrDefault(c => c.Code == code);
                        if (entity != null)
                        {
                            entity.Rate = rate;
                        }
                    }
                    context.SaveChanges();
                }
                Trace.TraceInformation("Updating Exchange Rates Completed");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message, ex);
            }
            return GetCurrencies();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}