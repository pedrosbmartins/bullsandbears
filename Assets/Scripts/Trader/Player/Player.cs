using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private bool forceMechanics = false;

    public event Action<Stock> OnBuyStock = delegate { };
    public event Action<Stock> OnSellStock = delegate { };
    public event Action<Stock> OnShortStock = delegate { };
    public event Action OnAccountChange = delegate { };
    public event Action OnAllPositionsClosed = delegate { };

    private StockMarket market;

    public Account Account { get; private set; }

    public Dictionary<string, int> Portfolio { get; private set; }

    public void Awake() {
        market = GetComponent<StockMarket>();
        Portfolio = new Dictionary<string, int>();
        Account = new Account(GameData.GetAccountBalance());
        market.OnStockProcessed += HandleStockProcessed;

        if (forceMechanics) {
            GameAchievements.ForceUnlockMechanics = true;
        }
    }

    public void SaveAccountBalance() {
        GameData.SetAccountBalance(Account.Balance);
    }

    public bool Owns(Stock stock) {
        return Portfolio.ContainsKey(stock.Symbol);
    }

    // shorted stocks are represented by
    // a negative quantity
    public bool Shorted(Stock stock) {
        return Portfolio.ContainsKey(stock.Symbol)
            && Portfolio[stock.Symbol] < 0;
    }

    public void Buy(Stock stock, int quantity) {
        float amount = stock.CurrentPrice() * quantity;

        if (Owns(stock)) {
            Portfolio[stock.Symbol] = Portfolio[stock.Symbol] + quantity;
        }
        else {
            Portfolio.Add(stock.Symbol, quantity);
        }
        OnBuyStock(stock);

        Account.Subtract(amount);
        OnAccountChange();
    }

    public void BuyAllShorted(Stock stock) {
        // shorted quantity is negative
        // so it has to be multiplied by -1
        int quantity = -1 * Portfolio[stock.Symbol];
        float amount = stock.CurrentPrice() * quantity;
        Portfolio.Remove(stock.Symbol);
        OnBuyStock(stock);
        Account.Subtract(amount);
        OnAccountChange();
    }

    public void Sell(Stock stock, int quantity) {
        if (!Owns(stock)) {
            return;
        }

        Portfolio[stock.Symbol] -= quantity;
        if (Portfolio[stock.Symbol] == 0) {
            Portfolio.Remove(stock.Symbol);
        }
        OnSellStock(stock);

        float amount = stock.CurrentPrice() * quantity;
        Account.Add(amount);
        OnAccountChange();
    }

    public void SellAll(Stock stock) {
        if (!Owns(stock)) {
            return;
        }
        int quantity = Portfolio[stock.Symbol];
        Sell(stock, quantity);
    }

    public void Short(Stock stock, int quantity) {
        Portfolio.Add(stock.Symbol, 0);
        Sell(stock, quantity);
        OnShortStock(stock);
    }

    public bool Affords(Stock stock, int quantity) {
        return stock.CurrentPrice() * quantity <= Account.Balance;
    }

    public void CloseAllPositions() {
        StartCoroutine(CloseAllPositionsCoroutine());
    }

    private IEnumerator CloseAllPositionsCoroutine() {
        float delay = 1.5f;
        List<string> ownedStocksSymbols = new List<string>(Portfolio.Keys);
        foreach (var symbol in ownedStocksSymbols) {
            yield return new WaitForSeconds(delay);
            int quantity = Portfolio[symbol];
            if (quantity > 0) {
                MessageCentral.Instance.DisplayMessage("Message", "Selling remaining " + symbol);
                SellAll(market.GetStock(symbol));
            }
            else {
                MessageCentral.Instance.DisplayMessage("Message", "Buying remaining shorted " + symbol);
                BuyAllShorted(market.GetStock(symbol));
            }
        }
        yield return new WaitForSeconds(delay);
        OnAllPositionsClosed();
    }

    private void HandleStockProcessed(Stock stock) {
        if (Owns(stock)) {
            OnAccountChange();
        }
    }

}
