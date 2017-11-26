using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public event Action<Stock> OnBuyStock = delegate { };
    public event Action<Stock> OnSellStock = delegate { };
    public event Action OnAccountChange = delegate { };
    public event Action OnAllPositionsClosed = delegate { };

    public StockMarket Market;

    public Account Account { get; private set; }

    public Dictionary<string, int> OwnedStocks { get; private set; }
    public float StocksValue { get; private set; }

    public void Awake() {
        OwnedStocks = new Dictionary<string, int>();
        Account = new Account(GameData.GetBalance());
        Market.OnStockProcessed += HandleStockProcessed;
    }

    public void SaveBalance() {
        GameData.SetBalance(Account.Balance);
    }

    public void Buy(Stock stock, int quantity) {
        float amount = stock.CurrentPrice() * quantity;
        Account.Subtract(amount);
        if (Owns(stock)) {
            OwnedStocks[stock.Symbol] = OwnedStocks[stock.Symbol] + quantity;
        }
        else {
            OwnedStocks.Add(stock.Symbol, quantity);
        }
        OnAccountChange();
        OnBuyStock(stock);
    }

    public void Sell(Stock stock) {
        if (!Owns(stock)) {
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

    public void CloseAllPositions() {
        StartCoroutine(CloseAllPositionsRoutine());
    }

    private IEnumerator CloseAllPositionsRoutine() {
        List<string> ownedStocksSymbols = new List<string>(OwnedStocks.Keys);
        foreach (var symbol in ownedStocksSymbols) {
            yield return new WaitForSeconds(1.5f);
            MessageCentral.Instance.DisplayMessage("Message", "Selling remaining " + symbol);
            Sell(Market.GetStock(symbol));
        }
        yield return new WaitForSeconds(1.5f);
        OnAllPositionsClosed();
    }

    private bool Owns(Stock stock) {
        return OwnedStocks.ContainsKey(stock.Symbol);
    }

    private void HandleStockProcessed(Stock stock) {
        if (Owns(stock)) {
            OnAccountChange();
        }
    }

}
