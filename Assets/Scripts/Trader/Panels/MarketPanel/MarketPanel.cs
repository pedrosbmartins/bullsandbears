using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MarketPanelContext { Idle, RowSelected, RowSelectedAndMarketActive }

public class MarketPanel : MonoBehaviour {

    private StockMarket market;
    private Player player;

    public StockTable Table;
    public KeyBindingPanel KeyBindingPanel;

    public bool AutoCheckInput = false;

    public QuantityModal QuantityModalPrefab;
    public AlertModal AlertModalPrefab;

    public AudioClip SellSoundEffect;

    public MarketPanelContext CurrentContext;

    private bool isModalOpened = false;

    private void Awake() {
        market = GetComponentInParent<StockMarket>();
        player = GetComponentInParent<Player>();

        market.OnDayStarted += HandleMarketDayStarted;
        market.OnDayEnded += HandleMarketDayEnded;
        market.OnStockAdded += HandleStockAdded;
        Table.OnRowSelected += HandleTableRowSelected;
        Table.OnRowSelectionCleared += HandleTableRowSelectionCleared;

        SetContext(MarketPanelContext.Idle);
    }

    private void Start() {
        KeyBindingPanel.Render(CurrentContext);
    }

    private void Update() {
        if (AutoCheckInput) {
            CheckInput();
        }
    }

    public void CheckInput() {
        if (!isModalOpened) {
            InterceptNavigationKeys();
            InterceptActionKeys();
        }
    }

    private void SetContext(MarketPanelContext context) {
        CurrentContext = context;
        KeyBindingPanel.Render(CurrentContext);
    }

    private void InterceptNavigationKeys() {
        if (Input.GetKeyUp(KeyCode.DownArrow)) {
            Table.SelectNextRow();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow)) {
            Table.SelectPreviousRow();
        }
        else if (Input.GetKeyUp(KeyCode.Escape)) {
            Table.DeselectRows();
        }
    }

    private void InterceptActionKeys() {
        KeyBindingPanel.KeyBindings.ForEach(binding => {
            if (Input.GetKeyUp(binding.Key)) {
                PerformAction(binding.Action);
            }
        });
    }

    private void PerformAction(KeyBindingAction action) {
        switch (action) {
            case KeyBindingAction.Help:
                DisplayHelpMessages();
                break;
            case KeyBindingAction.Buy:
                if (market.ActiveStock == null || market.CurrentState != MarketState.DayStarted) return;
                DisplayBuyModal();
                break;
            case KeyBindingAction.Sell:
                if  (market.ActiveStock == null 
                 || !player.OwnedStocks.ContainsKey(market.ActiveStock.Symbol)
                 ||  player.OwnedStocks[market.ActiveStock.Symbol] <= 0
                 ||  market.CurrentState != MarketState.DayStarted) {
                    return;
                }
                DisplaySellModal();
                break;
            case KeyBindingAction.Short:
                if (market.ActiveStock == null
                 || market.CurrentState != MarketState.DayStarted
                 || player.Owns(market.ActiveStock)) {
                    return;
                }
                DisplayShortModal();
                break;
        }
    }

    private void DisplayAlertModal(string title, string message, Action onSubmit, Action onExit) {
        var modal = Instantiate(AlertModalPrefab, transform.root, false);
        modal.SetTitle(title);
        modal.SetMessage(message);
        isModalOpened = true;
        modal.OnSubmit += onSubmit;
        modal.OnExit += onExit;
    }

    private void DisplayQuantityModal(string title, Action<int> onSubmit) {
        var modal = Instantiate(QuantityModalPrefab, transform.root, false);
        modal.SetTitle(title);
        isModalOpened = true;
        modal.OnSubmit += onSubmit;
        modal.OnExit += HandleModalExit;
    }

    private void DisplayBuyModal() {
        var stock = market.ActiveStock;
        var title = String.Format("BUY {0}", stock.Symbol);
        if (player.Borrowed(stock)) {
            DisplayAlertModal(title, "Buy all shorted stocks?", HandleBuyAllModalSubmit, HandleModalExit);
        }
        else {
            DisplayQuantityModal(title, HandleBuyQuantityModalSubmit);
        }
    }

    private void DisplayShortModal() {
        DisplayQuantityModal(String.Format("SHORT {0}", market.ActiveStock.Symbol), HandleShortModalSubmit);
    }

    private void DisplaySellModal() {
        DisplayAlertModal(String.Format("SELL {0}", market.ActiveStock.Symbol), "Sell all stocks?", HandleSellModalSubmit, HandleModalExit);
    }

    private void DisplayInsufficientFundsModal(string stockSymbol, int quantity) {
        DisplayAlertModal(
            "WARNING",
            String.Format("Insufficient funds to buy {0} {1} stocks", quantity, stockSymbol),
            HandleCantAffordModalExit,
            HandleCantAffordModalExit
        );
    }

    private void HandleBuyQuantityModalSubmit(int quantity) {
        HandleModalExit();
        Stock stock = market.ActiveStock;
        if (player.Affords(stock, quantity)) {
            player.Buy(stock, quantity);
        }
        else {
            DisplayInsufficientFundsModal(stock.Symbol, quantity);
        }
    }

    private void HandleBuyAllModalSubmit() {
        HandleModalExit();
        player.BuyAllShorted(market.ActiveStock);
    }

    private void HandleShortModalSubmit(int quantity) {
        HandleModalExit();
        player.Short(market.ActiveStock, quantity);
    }

    private void HandleCantAffordModalExit() {
        DisplayBuyModal();
    }

    private void HandleSellModalSubmit() {
        HandleModalExit();
        if (GameData.GetSFXOn()) {
            AudioSource.PlayClipAtPoint(SellSoundEffect, Vector3.one);
        }
        player.SellAll(market.ActiveStock);
    }

    private void HandleModalExit() {
        isModalOpened = false;
    }

    private void DisplayHelpMessages() {
        var messages = new string[] {
            "B&B Tutorial:",
            "[enter] to start market day",
            "[up] / [down] arrows select stocks",
            "[F2] to buy, [F3] to sell",
            "TREND shows price trend direction",
            "VOLUME indicates trend strenth",
            "Watchout for the stock's CEILING:",
            "Prices may fall quickly around it",
            "That's it!"
        };
        MessageCentral.Instance.DisplayMessages("Help", messages, true);
    }

    private void HandleMarketDayStarted() {
        if (CurrentContext == MarketPanelContext.RowSelected) {
            HandleTableRowSelected(Table.GetCurrentRow());
        }
    }

    private void HandleMarketDayEnded() {
        KeyBindingPanel.Render(CurrentContext);
    }

    private void HandleStockAdded(Stock stock) {
        Table.InsertRow(stock, player);
    }

    private void HandleTableRowSelected(StockTableRow row) {
        if (market.CurrentState == MarketState.DayStarted) {
            SetContext(MarketPanelContext.RowSelectedAndMarketActive);
        }
        else {
            SetContext(MarketPanelContext.RowSelected);
        }
        market.SetActiveStock(row.AssignedStockSymbol);
    }

    private void HandleTableRowSelectionCleared() {
        SetContext(MarketPanelContext.Idle);
        market.ClearActiveStock();
    }

    public void DestroyOpenedModals() {
        if (isModalOpened) {
            var modals = new List<GameObject>(GameObject.FindGameObjectsWithTag("Modal"));
            modals.ForEach(modal => Destroy(modal));
            isModalOpened = false;
        }
    }

}
