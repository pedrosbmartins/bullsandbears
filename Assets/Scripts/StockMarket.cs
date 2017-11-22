using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public interface IRandomGenerator {
    float NextRandomFloat(float min = 0f, float max = 1f);
}

public enum MarketState { PreOpen, Open, Ended, Closed };

public class StockMarket : MonoBehaviour, IRandomGenerator {

    public Player Player;

    public int RandomSeed;

    public int StockCount;
    public bool ActivateFirstStock;

    public bool AutoInitialize;
    public bool OpenMarketOnStart;

    public float MinStockProcessingDuration = 1f;
    public float MaxStockProcessingDuration = 5f;

    public bool EnableTimeTracker = true;
    public TimeTracker TimeTracker;

    public delegate void MarketDayStartedHandler();
    public event MarketDayStartedHandler OnMarketDayStarted = delegate {};

    public delegate void MarketDayEndedHandler();
    public event MarketDayEndedHandler OnMarketDayEnded = delegate {};

    public delegate void StockAddedHandler(Stock stock);
    public event StockAddedHandler OnStockAdded = delegate {};

    public delegate void StockProcessedHandler(Stock stock);
    public event StockProcessedHandler OnStockProcessed = delegate {};

    public delegate void ActiveStockProcessedHandler(Stock stock);
    public event ActiveStockProcessedHandler OnActiveStockProcessed = delegate {};

    public delegate void ActiveStockClearedHandler();
    public event ActiveStockClearedHandler OnActiveStockCleared = delegate {};

    public Stock ActiveStock { get; private set; }
    public MarketState CurrentState { get; private set; }

    private System.Random randomGenerator;

    private List<Stock> stockList = new List<Stock>();
    private List<Company> companyList;

    private void Awake() {
        InitializeRandomGenerator();
        InitializeRandomCompanyList();
        SetEventsHandlers();
    }

    private void Start() {
        if (AutoInitialize) {
            Initialize();
        }
    }

    public void Initialize() {
        SetState(MarketState.PreOpen);
        InitializeRandomStocks();
        if (OpenMarketOnStart) {
            BeginDay();
        }
    }

    public float NextRandomFloat(float min = 0f, float max = 1f) {
        return (float)randomGenerator.NextDouble() * (max - min) + min;
    }

    public Stock AddStock(string symbol, string companyName, Industry industry) {
        Stock stock = new Stock(symbol, companyName, industry, this);
        stockList.Add(stock);
        stock.OnProcessed += HandleStockProcessed;
        OnStockAdded(stock);
        return stock;
    }

    public Stock GetStock(string symbol) {
        return stockList.Find(stock => stock.Symbol == symbol);
    }

    public void SetActiveStock(string symbol) {
        ActiveStock = stockList.Find(stock => stock.Symbol == symbol);
        if (ActiveStock != null) {
            OnActiveStockProcessed(ActiveStock);
        }
    }

    public void ClearActiveStock() {
        ActiveStock = null;
        OnActiveStockCleared();
    }

    public DateTime GetCurrentTime() {
        return TimeTracker.GetCurrentTime();
    }

    private void SetState(MarketState state) {
        CurrentState = state;
    }

    private void InitializeRandomGenerator() {
        randomGenerator = (RandomSeed == 0) ? new System.Random() : new System.Random(RandomSeed);
    }

    private void InitializeRandomCompanyList() {
        companyList = GenerateCompanylist().OrderBy(company => randomGenerator.Next()).ToList<Company>();
    }

    private void SetEventsHandlers() {
        if (EnableTimeTracker) {
            TimeTracker.OnMarkedDayEnded += HandleMarketDayEnded;
        }
        Player.OnAllPositionsClosed += HandleAllPositionsClosed;
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

    public void BeginDay() {
        SetState(MarketState.Open);
        if (EnableTimeTracker) {
            TimeTracker.StartTracking();
        }
        stockList.ForEach(stock => StartCoroutine(ProcessStock(stock)));
        DisplayMarketDayStartedMessages();
        OnMarketDayStarted();
    }

    private void EndDay() {
        SetState(MarketState.Ended);
        StopAllCoroutines(); // stops all "ProcessStock" coroutines
        DisplayMarketDayEndedMessages();
        OnMarketDayEnded();
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

    private void DisplayMarketDayEndedMessages() {
        var messages = new string[] { "The market is now closed" };
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
        OnStockProcessed(stock);
        if (ActiveStock != null && ActiveStock.Symbol == stock.Symbol) {
            OnActiveStockProcessed(ActiveStock);
        }
    }

    private void HandleMarketDayEnded() {
        EndDay();
    }

    private void HandleAllPositionsClosed() {
        SetState(MarketState.Closed);
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
