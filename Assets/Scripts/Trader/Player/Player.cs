using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public event Action<Stock> OnBuyStock = delegate { };
    public event Action<Stock> OnSellStock = delegate { };
    public event Action<Stock> OnBorrowStock = delegate { };
    public event Action OnAccountChange = delegate { };
    public event Action OnAllPositionsClosed = delegate { };

    private StockMarket market;

    public Account Account { get; private set; }

    public Dictionary<string, int> OwnedStocks { get; private set; }
    public float StocksValue { get; private set; }

    public void Awake() {
        market = GetComponent<StockMarket>();
        OwnedStocks = new Dictionary<string, int>();
        Account = new Account(GameData.GetAccountBalance());
        market.OnStockProcessed += HandleStockProcessed;
    }

    public void SaveBalance() {
        GameData.SetAccountBalance(Account.Balance);
    }

    public bool Owns(Stock stock) {
        return OwnedStocks.ContainsKey(stock.Symbol);
    }

    public bool Borrowed(Stock stock) {
        return OwnedStocks.ContainsKey(stock.Symbol)
            && OwnedStocks[stock.Symbol] < 0;
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

    public void BuyAllShorted(Stock stock) {
        int quantity = -1 * OwnedStocks[stock.Symbol];
        float amount = stock.CurrentPrice() * quantity;
        Account.Subtract(amount);
        OwnedStocks.Remove(stock.Symbol);
        OnAccountChange();
        OnBuyStock(stock);
    }

    public void Sell(Stock stock, int quantity) {
        if (!Owns(stock)) {
            return;
        }
        float amount = stock.CurrentPrice() * quantity;
        Account.Add(amount);
        OwnedStocks[stock.Symbol] -= quantity;
        if (OwnedStocks[stock.Symbol] == 0) {
            OwnedStocks.Remove(stock.Symbol);
        }
        OnAccountChange();
        OnSellStock(stock);
    }

    public void SellAll(Stock stock) {
        if (!Owns(stock)) {
            return;
        }
        Sell(stock, OwnedStocks[stock.Symbol]);
    }

    public void Short(Stock stock, int quantity) {
        OwnedStocks.Add(stock.Symbol, 0);
        Sell(stock, quantity);
        OnAccountChange();
        OnBorrowStock(stock);
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
            int quantity = OwnedStocks[symbol];
            if (quantity > 0) {
                MessageCentral.Instance.DisplayMessage("Message", "Selling remaining " + symbol);
                SellAll(market.GetStock(symbol));
            }
            else {
                MessageCentral.Instance.DisplayMessage("Message", "Buying remaining shorted " + symbol);
                BuyAllShorted(market.GetStock(symbol));
            }
        }
        yield return new WaitForSeconds(1.5f);
        OnAllPositionsClosed();
    }

    private void HandleStockProcessed(Stock stock) {
        if (Owns(stock)) {
            OnAccountChange();
        }
    }

}
