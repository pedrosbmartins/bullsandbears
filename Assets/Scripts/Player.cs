using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public delegate void BuyStockHandler(Stock stock);
    public event BuyStockHandler OnBuyStock = delegate {};

    public delegate void SellStockHandler(Stock stock);
    public event SellStockHandler OnSellStock = delegate { };

    public delegate void AccountChangeHandler();
    public event AccountChangeHandler OnAccountChange = delegate { };

    public delegate void AllPositionsClosedHandler();
    public event AllPositionsClosedHandler OnAllPositionsClosed = delegate { };

    public StockMarket Market;

    public Account Account { get; private set; }

    public Dictionary<string, int> OwnedStocks { get; private set; }
    public float StocksValue { get; private set; }

    public void Awake() {
        OwnedStocks = new Dictionary<string, int>();
        Account = new Account(GameData.GetBalance());
        Market.OnMarketDayEnded += HandleMarketDayEnded;
    }

    public void Buy(Stock stock, int quantity) {
        float amount = stock.CurrentPrice() * quantity;
        Account.Subtract(amount);
        if (OwnedStocks.ContainsKey(stock.Symbol)) {
            OwnedStocks[stock.Symbol] = OwnedStocks[stock.Symbol] + quantity;
        }
        else {
            OwnedStocks.Add(stock.Symbol, quantity);
        }
        OnAccountChange();
        OnBuyStock(stock);
    }

    public void Sell(Stock stock) {
        if (!OwnedStocks.ContainsKey(stock.Symbol)) {
            return;
        }
        float amount = stock.CurrentPrice() * OwnedStocks[stock.Symbol];
        Account.Add(amount);
        OwnedStocks.Remove(stock.Symbol);
        OnAccountChange();
        OnSellStock(stock);
    }

    public bool Affords(Stock stock, int quantity) {
        return stock.CurrentPrice() * quantity <= Account.Balance;
    }

    private void HandleMarketDayEnded() {
        StartCoroutine(CloseAllPositions());
    }

    private IEnumerator CloseAllPositions() {
        List<string> ownedStocksSymbols = new List<string>(OwnedStocks.Keys);
        foreach (var symbol in ownedStocksSymbols) {
            yield return new WaitForSeconds(0.5f);
            MessageCentral.Instance.DisplayMessage("Message", "Selling remaining " + symbol);
            Sell(Market.GetStock(symbol));
        }
        OnAllPositionsClosed();
    }

    public void SaveBalance() {
        GameData.SetBalance(Account.Balance);
    }

}
