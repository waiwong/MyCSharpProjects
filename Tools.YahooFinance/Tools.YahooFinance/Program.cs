using System;
using System.Collections.Generic;
using System.Net;

namespace Tools.YahooFinance
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string strSymbols = "001470,008271";
            List<PriceLite> prices = YahooFinanceLite.Parse(strSymbols);
            foreach (PriceLite price in prices)
            {
                Console.WriteLine(price.ToString());
            }

            //string stockData = string.Empty;
            //string url = "http://finance.yahoo.com/d/quotes.csv?s=1470.HK+8271.HK+0003.HK+0001.HK&f=snbaopl1t1lvk1";
            //using (WebClient client = new WebClient())
            //{
            //    client.UseDefaultCredentials = true;
            //    int cnt = 0;
            //    while (cnt <= 2)
            //    {
            //        try
            //        {
            //            if (cnt == 0)
            //            {
            //                WebProxy proxy = new WebProxy("ITD1SG511S", 8085);
            //                proxy.BypassProxyOnLocal = true;
            //                proxy.UseDefaultCredentials = true;
            //                client.Proxy = proxy;
            //            }
            //            else if (cnt == 1)
            //            {
            //                client.Proxy = WebRequest.GetSystemWebProxy();
            //            }
            //            else if (cnt == 2)
            //            {
            //                client.Proxy = WebRequest.DefaultWebProxy;
            //            }

            //            stockData = client.DownloadString(new Uri(url));

            //            if (stockData.Length > 0)
            //                break;
            //        }
            //        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            //        cnt++;
            //    }
            //}

            //List<Price> prices = YahooFinance.Parse(stockData);

            //foreach (Price price in prices)
            //{
            //    Console.WriteLine(price.ToString());
            //}

            Console.Read();

        }
    }
}