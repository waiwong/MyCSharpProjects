using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Tools.YahooFinance
{
    public static class YahooFinanceLite
    {
        public static List<PriceLite> Parse(string symbols)
        {
            List<PriceLite> prices = new List<PriceLite>();
            List<string> lstSymbol = new List<string>();
            foreach (string row in symbols.Split(','))
            {
                string val = row;
                if (int.Parse(val) > 9999)
                {
                    Console.WriteLine(string.Format("symbol error:{0}", val));
                    continue;
                }

                val = val.PadLeft(8, '0').Substring(4) + ".HK";
                if (!lstSymbol.Contains(val))
                    lstSymbol.Add(val);
                if (lstSymbol.Count >= 500)
                    break;
            }

            if (lstSymbol.Count == 0)
                return prices;

            string stockData = string.Empty;
            string url = string.Format("http://finance.yahoo.com/d/quotes.csv?s={0}&f=snopl1", string.Join("+", lstSymbol.ToArray()));
            using (WebClient client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                int cnt = 0;
                while (cnt <= 2)
                {
                    try
                    {
                        //if (cnt == 0)
                        //{
                        //    WebProxy proxy = new WebProxy("ITD1SG511S", 8085);
                        //    proxy.BypassProxyOnLocal = true;
                        //    proxy.UseDefaultCredentials = true;
                        //    client.Proxy = proxy;
                        //}
                        //else if (cnt == 1)
                        //{
                        //    client.Proxy = WebRequest.GetSystemWebProxy();
                        //}
                        //else if (cnt == 2)
                        //{
                        //    client.Proxy = WebRequest.DefaultWebProxy;
                        //}

                        stockData = client.DownloadString(new Uri(url));

                        if (stockData.Length > 0)
                            break;
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    cnt++;
                }
            }

            if (stockData.Length > 0)
            {
                string[] rows = stockData.Replace("\r", "").Split('\n');
                foreach (string row in rows)
                {
                    if (string.IsNullOrEmpty(row))
                        continue;

                    string[] cols = row.Split(',');

                    PriceLite p = new PriceLite();
                    p.Symbol = cols[0];
                    p.Name = cols[1];
                    p.Open = ParseDecimal(cols[2]);
                    p.PreviousClose = ParseDecimal(cols[3]);
                    p.Last = ParseDecimal(cols[4]);
                    prices.Add(p);
                }
            }

            return prices;
        }

        private static decimal? ParseDecimal(string strValue)
        {
            decimal bid;
            if (!decimal.TryParse(strValue, out bid))
                return null;

            return bid;
        }
    }

    public class PriceLite
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? Open { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? Last { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1}) Last:{2} Open: {3} PreviousClose:{4}",
                this.Name, this.Symbol,
                this.Last.HasValue ? this.Last.Value.ToString("N2") : "N/A",
                this.Open.HasValue ? this.Open.Value.ToString("N2") : "N/A",
                this.PreviousClose.HasValue ? this.PreviousClose.Value.ToString("N2") : "N/A"
                );
        }
    }
}
