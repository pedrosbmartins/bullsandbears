using System;

public enum Industry { Technology, OilAndGas, BanksAndFinance, Automotive };

public class Company {

    public string TickerSymbol;
    public string Name;
    public Industry Industry;

    public Company(string symbol, string name, Industry industry) {
        TickerSymbol = symbol;
        Name = name;
        Industry = industry;
    }

}
