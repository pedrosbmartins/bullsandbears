using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private const float INITIAL_ACCOUNT = 50000;

    public delegate void BuyStockHandler(Stock stock);
    public event BuyStockHandler OnBuyStock;

    public delegate void SellStockHandler(Stock stock);
    public event SellStockHandler OnSellStock;

    public delegate void AccountChangHandler();
    public event AccountChangHandler OnAccountChange;

    public Account Account;

    public Dictionary<string, int> OwnedStocks { get; private set; }
    public float StocksValue { get; private set; }

    public void Awake() {
        OwnedStocks = new Dictionary<string, int>();
        Account = new Account(INITIAL_ACCOUNT);
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
        if (OnAccountChange != null) OnAccountChange();
        if (OnBuyStock != null) OnBuyStock(stock);
    }

    public void Sell(Stock stock) {
        if (!OwnedStocks.ContainsKey(stock.Symbol)) {
            return;
        }
        float amount = stock.CurrentPrice() * OwnedStocks[stock.Symbol];
        Account.Add(amount);
        OwnedStocks.Remove(stock.Symbol);
        if (OnAccountChange != null) OnAccountChange();
        if (OnSellStock != null) OnSellStock(stock);
    }

}
