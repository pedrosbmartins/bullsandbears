using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public interface IRandomGenerator {
    float NextRandomFloat(float min = 0f, float max = 1f);
}

public enum MarketState { Idle, DayStarted, DayEnded, Closed };

public class StockMarket : MonoBehaviour, IRandomGenerator {

    [SerializeField] private int RandomSeed;

    [SerializeField] private int StockCount;
    [SerializeField] private bool ActivateFirstStock;

    [SerializeField] private bool AutoInitialize;
    [SerializeField] private bool OpenMarketOnStart;

    [SerializeField] private float MinStockProcessingDuration = 1f;
    [SerializeField] private float MaxStockProcessingDuration = 3f;

    [SerializeField] private float PriceEffectDuration = 15f;

    public event Action OnDayStarted = delegate { };
    public event Action OnDayEnded = delegate { };

    public event Action<Stock> OnStockAdded = delegate { };
    public event Action<Stock> OnStockProcessed = delegate { };

    public event Action<Stock> OnActiveStockProcessed = delegate { };
    public event Action OnActiveStockCleared = delegate { };

    public List<Stock> StockList { get; private set; }
    public Stock ActiveStock { get; private set; }
    public MarketState CurrentState { get; private set; }

    private Player player;
    [NonSerialized] public TimeTracker TimeTracker;

    private System.Random randomGenerator;

    private List<Company> companyList;

    private void Awake() {
        StockList = new List<Stock>();

        player = GetComponent<Player>();
        TimeTracker = GetComponent<TimeTracker>();

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
        SetState(MarketState.Idle);
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
        StockList.Add(stock);
        stock.OnProcessed += HandleStockProcessed;
        OnStockAdded(stock);
        return stock;
    }

    public Stock GetStock(string symbol) {
        return StockList.Find(stock => stock.Symbol == symbol);
    }

    public void SetActiveStock(string symbol) {
        ActiveStock = StockList.Find(stock => stock.Symbol == symbol);
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

    public void SetNewPriceEffect(PriceEffect effect) {
        StopCoroutine("PriceEffectTimeout");
        StockList.ForEach(stock => {
            if (stock.CompanyIndustry == effect.AffectedIndustry) {
                stock.SetExternalEffect(effect);
            }
        });
        StartCoroutine("PriceEffectTimeout");
    }

    private IEnumerator PriceEffectTimeout() {
        yield return new WaitForSeconds(PriceEffectDuration);
        StockList.ForEach(stock => stock.ClearExternalEffect());
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
        if (TimeTracker != null) {
            TimeTracker.OnMarkedDayEnded += HandleMarketDayEnded;
        }
        player.OnAllPositionsClosed += HandleAllPositionsClosed;
    }

    private void InitializeRandomStocks() {
        for (int i = 0; i < StockCount; i++) {
            AddRandomStock();
        }
        if (ActivateFirstStock) {
            SetActiveStock(StockList[0].Symbol);
        }
    }

    private void AddRandomStock() {
        var company = companyList[StockList.Count];
        AddStock(company.TickerSymbol, company.Name, company.Industry);
    }

    public void BeginDay() {
        SetState(MarketState.DayStarted);
        if (TimeTracker != null) {
            TimeTracker.StartTracking();
        }
        StockList.ForEach(stock => StartCoroutine(ProcessStock(stock)));
        DisplayMarketDayStartedMessages();
        OnDayStarted();
    }

    private void EndDay() {
        SetState(MarketState.DayEnded);
        StopAllCoroutines(); // stops all "ProcessStock" and Price Effect coroutines
        DisplayMarketDayEndedMessages();
        OnDayEnded();
    }

    private void DisplayMarketDayStartedMessages() {
        var messages = new List<string>() {
            "The market is open",
            "It closes at 05:00PM",
        };

        if (!GameAchievements.IsInLastAchievementLevel()) {
            messages.AddRange(new List<string>() {
                "Your current balance target is:",
                GameAchievements.Current().BalanceTarget.ToString("C2"),
            });
        }

        MessageCentral.Instance.DisplayMessages("Message", messages.ToArray(), true);
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
