using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public interface IRandomGenerator {
    float NextRandomFloat(float min = 0f, float max = 1f);
}

public class StockMarket : MonoBehaviour, IRandomGenerator {

    public int RandomSeed;

    public int StockCount;
    public bool ActivateFirstStock;

    public bool OpenMarketOnStart;

    public float MinStockProcessingDuration = 1f;
    public float MaxStockProcessingDuration = 5f;

    public bool EnableTimeTracker = true;
    public TimeTracker TimeTracker;

    public delegate void MarketDayStartedHandler();
    public event MarketDayStartedHandler OnMarketDayStarted;

    public delegate void StockAddedHandler(Stock stock);
    public event StockAddedHandler OnStockAdded;

    public delegate void StockProcessedHandler(Stock stock);
    public event StockProcessedHandler OnStockProcessed;

    public delegate void ActiveStockProcessedHandler(Stock stock);
    public event ActiveStockProcessedHandler OnActiveStockProcessed;

    public delegate void ActiveStockClearedHandler();
    public event ActiveStockClearedHandler OnActiveStockCleared;

    public Stock ActiveStock { get; private set; }
    public bool MarketDayStarted { get; private set; }

    private System.Random randomGenerator;

    private List<Stock> stockList = new List<Stock>();
    private List<Company> companyList;

    private void Awake() {
        InitializeRandomGenerator();
        InitializeRandomCompanyList();
        SetEventsHandlers();
    }

    private void Start() {
        InitializeRandomStocks();
        DisplayPreMarketDayMessages();
        if (OpenMarketOnStart) {
            BeginMarketDay();
        }
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            if (!MarketDayStarted) {
                BeginMarketDay();
            }
        }
    }

    public float NextRandomFloat(float min = 0f, float max = 1f) {
        return (float)randomGenerator.NextDouble() * (max - min) + min;
    }

    public Stock AddStock(string symbol, string companyName, Industry industry) {
        Stock stock = new Stock(symbol, companyName, industry, this);
        stockList.Add(stock);
        stock.OnProcessed += HandleStockProcessed;
        if (OnStockAdded != null) {
            OnStockAdded(stock);
        }
        return stock;
    }

    public Stock GetStock(string symbol) {
        return stockList.Find(stock => stock.Symbol == symbol);
    }

    public void SetActiveStock(string symbol) {
        ActiveStock = stockList.Find(stock => stock.Symbol == symbol);
        if (ActiveStock != null && OnActiveStockProcessed != null) {
            OnActiveStockProcessed(ActiveStock);
        }
    }

    public void ClearActiveStock() {
        ActiveStock = null;
        if (OnActiveStockCleared != null) {
            OnActiveStockCleared();
        }
    }

    public DateTime GetCurrentTime() {
        return TimeTracker.GetCurrentTime();
    }

    private void InitializeRandomGenerator() {
        randomGenerator = (RandomSeed == 0) ? new System.Random() : new System.Random(RandomSeed);
    }

    private void InitializeRandomCompanyList() {
        companyList = GenerateCompanylist().OrderBy(company => randomGenerator.Next()).ToList<Company>();
    }

    private void SetEventsHandlers() {
        if (EnableTimeTracker) {
            TimeTracker.OnMarkedClosed += HandleMarketClosed;
        }
    }

    private void InitializeRandomStocks() {
        for (int i = 0; i < StockCount; i++) {
            AddRandomStock();
        }
        if (ActivateFirstStock) {
            SetActiveStock(stockList[0].Symbol);
        }
    }

    private void AddRandomStock() {
        var company = companyList[stockList.Count];
        AddStock(company.TickerSymbol, company.Name, company.Industry);
    }

    private void BeginMarketDay() {
        MarketDayStarted = true;
        if (EnableTimeTracker) {
            TimeTracker.StartTracking();
        }
        stockList.ForEach(stock => StartCoroutine(ProcessStock(stock)));
        DisplayMarketDayStartedMessages();
        if (OnMarketDayStarted != null) {
            OnMarketDayStarted();
        }
    }

    private void EndMarketDay() {
        MarketDayStarted = false;
        StopAllCoroutines(); // stops all "ProcessStock" coroutines
    }

    private void DisplayPreMarketDayMessages() {
        var messages = new string[] {
            "Good morning, day traders",
            "The market will soon be open",
            "Press [space] to skip messages",
            "Press [F1] to display help",
            "Or press [enter] to start trading",
        };
        MessageCentral.Instance.DisplayMessages("Message", messages, false, true);
    }

    private void DisplayMarketDayStartedMessages() {
        var messages = new string[] {
            "The market is open",
            "It closes at 05:00PM",
            "Your current balance target is:",
            "$100,000.00",
        };
        MessageCentral.Instance.DisplayMessages("Message", messages, true);
    }

    private IEnumerator ProcessStock(Stock stock) {
        while (true) {
            yield return new WaitForSeconds(
                NextRandomFloat(MinStockProcessingDuration, MaxStockProcessingDuration)
            );
            stock.Process();
        }
    }

    private List<Company> GenerateCompanylist() {
        return new List<Company>() {
            new Company("AAPN", "Pineapple, Inc.", Industry.Technology),
            new Company("GOOF", "Goofle LLC", Industry.Technology),
            new Company("MIFT", "Minisoft Corporation", Industry.Technology),
            new Company("INDC", "Indell Corporation", Industry.Technology),
            new Company("RFST", "Rainforest.com, Inc.", Industry.Technology),
            new Company("ZON", "Ezon Moboil Corporation", Industry.OilAndGas),
            new Company("PETL", "Petrosil S.A.", Industry.OilAndGas),
            new Company("CVO", "Chevroom Corporation", Industry.OilAndGas),
            new Company("JPMO", "JPMoney Chase & Co.", Industry.BanksAndFinance),
            new Company("T", "Towngroup, Inc.", Industry.BanksAndFinance),
            new Company("GSA", "Golden Sax Group, Inc.", Industry.BanksAndFinance),
        };
    }

    private void HandleStockProcessed(Stock stock) {
        if (OnStockProcessed != null) {
            OnStockProcessed(stock);
        }
        if (ActiveStock != null && ActiveStock.Symbol == stock.Symbol && OnActiveStockProcessed != null) {
            OnActiveStockProcessed(ActiveStock);
        }
    }

    private void HandleMarketClosed() {
        Debug.Log("Market day ends");
        EndMarketDay();
    }

}

public enum Industry { Technology, OilAndGas, BanksAndFinance };

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
