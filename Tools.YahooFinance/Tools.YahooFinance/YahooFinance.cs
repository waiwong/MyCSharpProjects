using System;
using System.Collections.Generic;

namespace Tools.YahooFinance
{
    public static class YahooFinance
    {
        public static List<Price> Parse(string csvData)
        {
            List<Price> prices = new List<Price>();

            string[] rows = csvData.Replace("\r", "").Split('\n');

            foreach (string row in rows)
            {
                if (string.IsNullOrEmpty(row))
                    continue;

                string[] cols = row.Split(',');

                Price p = new Price();
                p.Symbol = cols[0];
                p.Name = cols[1];
                p.Bid = ParseDecimal(cols[2]);
                p.Bid = ParseDecimal(cols[2]);
                p.Ask = ParseDecimal(cols[3]);
                p.Open = ParseDecimal(cols[4]);
                p.PreviousClose = ParseDecimal(cols[5]);
                p.Last = ParseDecimal(cols[6]);
                p.LastTradeTime = cols[7];
                p.LastTrade = cols[8];
                p.Volume = cols[9];
                p.LastTradeRealTimeWithTime = cols[10];
                prices.Add(p);
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

    public class Price
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
        public decimal? Open { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? Last { get; set; }
        public string LastTradeTime { get; set; }
        public string LastTrade { get; set; }
        public string Volume { get; set; }
        public string LastTradeRealTimeWithTime { get; set; }
        public override string ToString()
        {
            return string.Format("{0} ({1})  Bid:{2} Offer:{3} Last:{4} Open: {5} PreviousClose:{6} Last Trade Time:{7} LastTrade:{8} Volume:{9}"
                + " LastTradeRealTimeWithTime:{10}",
                this.Name, this.Symbol,
                this.Bid.HasValue ? this.Bid.Value.ToString("N2") : "N/A",
                this.Ask.HasValue ? this.Ask.Value.ToString("N2") : "N/A",
                this.Last.HasValue ? this.Last.Value.ToString("N2") : "N/A",
                this.Open.HasValue ? this.Open.Value.ToString("N2") : "N/A",
                this.PreviousClose.HasValue ? this.PreviousClose.Value.ToString("N2") : "N/A",
                this.LastTradeTime,
                this.LastTrade,
                this.Volume,
                this.LastTradeRealTimeWithTime
                );
        }
    }

    
}