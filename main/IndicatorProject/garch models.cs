
using System;
using System.Collections.Generic;
using System.Linq;

public static class GARCH
{

    /// <summary>
    /// Calculates volatility using the GARCH(1,1) formula.
    /// This calculation was derived from a spreadsheet at Bionic Turtle
    /// (http://www.bionicturtle.com/)
    /// (NOTE: Gamma is determined using:  1 - alpha - beta)
    /// </summary>
    /// <param name="stockPrices">An array of StockPrice objects</param>
    /// <param name="idx">The index of the 'StockPrices' where this calculation applies</param>
    /// <param name="longRunVariance">The long run variance for this series of prices</param>
    /// <param name="alpha">The Alpha weight used for this calculation</param>
    /// <param name="beta">The Beta weight used for this calculation</param>
    /// <param name="lambdaConst">The lambda value used for the calculation. Defaults to the Risk Metrics value of 94%</param>
    /// <param name="tradingDays">The # of days used in the calculation (252 for annualized volatility)</param>
    /// <param name="arraySize">The number of days to go back in the history of prices to make this calculation.</param>
    /// <returns>Volatility of the GARCH(1,1) formula in decimal form. Multiply by 100 to get a percentage.</returns>
    public static double GetGarch1_1Volatility(List<BarData> stockPrices, int idx, double longRunVariance,
            double alpha, double beta, double lambdaConst = 0.94F, int tradingDays = 252, int arraySize = 252)
    {
        var adjArraySize = arraySize;
        if (stockPrices.Count < arraySize)
            adjArraySize = stockPrices.Count;

        // The three weights Alpha + Beta + Gamma can not be greater than 1
        // Throw an exception if this occurs.
        if (alpha + beta > 1.0)
            throw new Exception("The weights Alpha + Beta + Gamma should not be greater than 1");
        double gamma = 1 - alpha - beta;

        var periodReturns = new List<double>();
        var periodReturnsSquared = new List<double>(); // Returns squared, which is the same as simple volatility

        // Create a subset of the prices based on the arraySize
        var tmpPriceSt = idx - (adjArraySize - 1);
        var tmpStockPrices = stockPrices.GetRange(tmpPriceSt, adjArraySize);

        double lastPrice = 0;
        for (int x = 0; x < tmpStockPrices.Count; x++)
        {
            if (x == 0)
            {
                periodReturns.Add(0);
                periodReturnsSquared.Add(0);
                lastPrice = (double)tmpStockPrices.ElementAt(x).Close;
            }
            else
            {
                var priceChange = Math.Log((double)tmpStockPrices.ElementAt(x).Close / lastPrice);
                periodReturns.Add(priceChange);
                periodReturnsSquared.Add(Math.Pow(priceChange, 2));
                lastPrice = (double)tmpStockPrices.ElementAt(x).Close;
            }
        }

        var weightVars = new List<double>(); // Weighted variances for day 'N'
        var weightVarsPrev = new List<double>(); // Weighted variances for day 'N-1'

        double weight = 0; // The weight for day 'N'
        double weightPrev = 0; // The weight for day 'N-1'

        // Use a reverse loop to run through the prices from most recent to oldest
        for (int i = tmpStockPrices.Count; i > 0; i--)
        {
            if (i == (tmpStockPrices.Count() - 2)) // Weight for day 'N'
            {
                weight = 1.0F - lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
            }
            else if (i == (tmpStockPrices.Count() - 3)) // Weight for day 'N-1'
            {
                weightPrev = 1.0F - lambdaConst;
                weight = weight * lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
                weightVarsPrev.Add((double)periodReturnsSquared.ElementAt(i) * weightPrev);
            }
            else if (i < tmpStockPrices.Count() - 3)
            {
                weight = weight * lambdaConst;
                weightPrev = weightPrev * lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
                weightVarsPrev.Add((double)periodReturnsSquared.ElementAt(i) * weightPrev);
            }
        }
        var sumVars = weightVars.Sum();
        var sumVarsPrev = weightVarsPrev.Sum();
        var garch = (longRunVariance * gamma) + (alpha *
                periodReturnsSquared.ElementAt(adjArraySize - 2)) + (beta * sumVarsPrev);
        return Math.Sqrt(garch) * Math.Sqrt(tradingDays);
    }



}


public class EWMAVolatility
{
    /// <summary>
    /// Exponential weighted moving average (EWMA) volatility
    /// This calculation was derived from a spreadsheet at Bionic Turtle
    /// (http://www.bionicturtle.com/)
    /// </summary>
    /// <param name="stockPrices">An array of StockPrice objects</param>
    /// <param name="idx">The index of the 'stock_prices_array' where this calculation applies</param>
    /// <param name="tradingDays">The # of days used in the calculation (252 for annualized volatility)</param>
    /// <param name="lambdaConst">The lambda value used for the calculation. Defaults to the Risk Metrics value of 94%</param>
    /// <param name="arraySize">The number of days to go back in the history of prices to make this calculation.</param>
    /// <returns>Volatility of the exponentially weighted moving average in decimal form. Multiply by 100 to get a percentage.</returns>
    public static double GetEWMAVolatility(List<BarData> stockPrices, int idx, int tradingDays = 252,
            double lambdaConst = 0.94F, int arraySize = 252)
    {
        var adjArraySize = arraySize;
        if (stockPrices.Count < arraySize)
            adjArraySize = stockPrices.Count;

        var periodReturns = new List<double>();
        var periodReturnsSquared = new List<double>(); // Returns squared, which is the same as simple volatility

        // Create a subset of the prices based on the arraySize
        var tmpPriceSt = idx - (adjArraySize - 1);
        var tmpStockPrices = stockPrices.GetRange(tmpPriceSt, adjArraySize);

        double lastPrice = 0;
        for (int x = 0; x < tmpStockPrices.Count; x++)
        {
            if (x == 0)
            {
                periodReturns.Add(0);
                periodReturnsSquared.Add(0);
                lastPrice = (double)tmpStockPrices.ElementAt(x).Close;
            }
            else
            {
                var priceChange = Math.Log((double)tmpStockPrices.ElementAt(x).Close / lastPrice);
                periodReturns.Add(priceChange);
                periodReturnsSquared.Add(Math.Pow(priceChange, 2));
                lastPrice = (double)tmpStockPrices.ElementAt(x).Close;
            }
        }

        var weightVars = new List<double>(); // Weighted variances for day 'N'
        var weightVarsPrev = new List<double>(); // Weighted variances for day 'N-1'

        double weight = 0; // The weight for day 'N'
        double weightPrev = 0; // The weight for day 'N-1'

        // Use a reverse loop to run through the prices from most recent to oldest
        for (int i = tmpStockPrices.Count; i > 0; i--)
        {
            if (i == (tmpStockPrices.Count() - 2)) // Weight for day 'N'
            {
                weight = 1.0F - lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
            }
            else if (i == (tmpStockPrices.Count() - 3)) // Weight for day 'N-1'
            {
                weightPrev = 1.0F - lambdaConst;
                weight = weight * lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
                weightVarsPrev.Add((double)periodReturnsSquared.ElementAt(i) * weightPrev);
            }
            else if (i < tmpStockPrices.Count() - 3)
            {
                weight = weight * lambdaConst;
                weightPrev = weightPrev * lambdaConst;
                weightVars.Add((double)periodReturnsSquared.ElementAt(i) * weight);
                weightVarsPrev.Add((double)periodReturnsSquared.ElementAt(i) * weightPrev);
            }
        }
        var sumVars = weightVars.Sum();
        var sumVarsPrev = weightVarsPrev.Sum();
        var ewma = lambdaConst * sumVarsPrev + (1 - lambdaConst) * periodReturnsSquared.ElementAt(adjArraySize - 2);
        return Math.Sqrt(ewma) * Math.Sqrt(tradingDays);
    }




    // StockPrice Object from the method Above, for reference
    public class StockPrice
    {
        public int id { get; set; }
        public int stock_id { get; set; }
        public string symbol { get; set; }
        public System.DateTime price_date { get; set; }
        public decimal opening_price { get; set; }
        public decimal high_price { get; set; }
        public decimal low_price { get; set; }
        public decimal closing_price { get; set; }
    }

}

public static class HistVol
{
    /// <summary>
    /// Calculates historic volatility which essentially uses a SMA.
    /// </summary>
    /// <param name="stockPrices">An array of StockPrice objects</param>
    /// <param name="idx">The index of the 'stockPrices' of this value</param>
    /// <param name="period">The period for the historic volatility</param>
    /// <param name="tradingDays">Trading days to use for the volatility calculation. (252 for a year, etc.)</param>
    /// <returns></returns>
    public static double GetHistoricalVolatility(List<BarData> stockPrices, int idx, int period = 20, int tradingDays = 252)
    {
        if (idx >= period)  // User 'period', instead of (period -1) since the first calc is period+1
        {
            List<double> priceChanges = new List<double>();
            double lastPrice = 0;
            for (int x = 0; x < stockPrices.Count; x++)
            {
                if (x == 0)
                {
                    priceChanges.Add(0.0);
                    lastPrice = (double)stockPrices.ElementAt(x).Close;
                }
                else
                {
                    double priceChange = Math.Log((double)stockPrices.ElementAt(x).Close / lastPrice);
                    priceChanges.Add(priceChange);
                    lastPrice = (double)stockPrices.ElementAt(x).Close;
                }
            }
            // Grab an array of price changes over the selected period
            int tmp_price_changes_st = idx - (period - 1);
            List<double> price_changes = priceChanges.GetRange(tmp_price_changes_st, period);
            double stdDevTmp = CalculateStdDev(price_changes);
            if (stdDevTmp == Double.NaN)
                return 0.0;
            double retTemp = stdDevTmp * Math.Sqrt(tradingDays);
            return (double)retTemp;
        }
        else
        {
            return 0.0;
        }
    }


    public static double GetHistoricalStdevMA(List<double> stockPrices,List<double>ma,  int idx, int period = 20, int tradingDays = 252)
    {
        if (idx >= period)  // User 'period', instead of (period -1) since the first calc is period+1
        {
            List<double> priceChanges = new List<double>();
            double lastPrice = 0;
            for (int x = 0; x < Math.Min(ma.Count,stockPrices.Count); x++)
            {
                if (x == 0)
                {
                    priceChanges.Add(0.0);
                    lastPrice = (double)stockPrices.ElementAt(x);
                }
                else
                {
                    double priceChange = Math.Log((double)stockPrices.ElementAt(x) / (double)ma.ElementAt(x));
                    priceChanges.Add(priceChange);
                    lastPrice = (double)stockPrices.ElementAt(x);
                }
            }
            // Grab an array of price changes over the selected period
            int tmp_price_changes_st = idx - (period - 1);
            List<double> price_changes = priceChanges.GetRange(tmp_price_changes_st, period-1);
            double stdDevTmp = CalculateStdDev(price_changes);
            if (double.IsNaN(stdDevTmp))
                return 0.0;
            double retTemp = stockPrices[0]/ma[0]/stdDevTmp;
            return (double)retTemp;
        }
        else
        {
            return 0.0;
        }
    }


    /// <summary>
    /// Calculates the standard deviation for a list of double values.
    /// </summary>
    /// <param name="values">List of values used in the standard devation</param>
    /// <returns>Standard Deviation</returns>
    private static double CalculateStdDev(IEnumerable<double> values)
    {
        double ret = 0;
        if (values.Count() > 0)
        {
            //Compute the Average      
            double avg = values.Average();
            //Perform the Sum of (value-avg)_2_2      
            double sum = values.Sum(d => Math.Pow(d - avg, 2));
            //Put it all together      
            ret = Math.Sqrt((sum) / (values.Count() - 1));
        }
        return ret;
    }
}