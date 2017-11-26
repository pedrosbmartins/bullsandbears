using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MarketPanelContext { Idle, RowSelected, RowSelectedAndMarketActive }
public enum KeyBindingAction { Help, Buy, Sell}

public class KeyBinding {

    public KeyBindingAction Action { get; private set; }
    public KeyCode Key { get; private set; }
    public List<MarketPanelContext> Contexts { get; private set; }

    public KeyBinding(KeyBindingAction action, KeyCode key, List<MarketPanelContext> contexts) {
        Action = action;
        Key = key;
        Contexts = contexts;
    }

}

public class MarketPanel : MonoBehaviour {

    private StockMarket market;
    private Player player;

    public StockTable Table;
    public KeyBindingPanel KeyBindingPanel;

    public BuyModal BuyModalPrefab;
    public AlertModal AlertModalPrefab;

    public AudioClip SellSoundEffect;

    public MarketPanelContext CurrentContext;

    private bool isModalOpened = false;

    private void Awake() {
        market = GetComponentInParent<StockMarket>();
        player = GetComponentInParent<Player>();

        market.OnDayStarted += HandleMarketDayStarted;
        market.OnStockAdded += HandleStockAdded;
        Table.OnRowSelected += HandleTableRowSelected;
        Table.OnRowSelectionCleared += HandleTableRowSelectionCleared;

        SetContext(MarketPanelContext.Idle);
    }

    private void Start() {
        KeyBindingPanel.Render(CurrentContext);
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
                 ||  player.OwnedStocks[market.ActiveStock.Symbol] == 0) {
                    return;
                }
                DisplaySellModal();
                break;
        }
    }

    private void DisplayBuyModal() {
        BuyModal buyModal = Instantiate(BuyModalPrefab, transform.root, false);
        buyModal.SetTitle(String.Format("BUY {0}", market.ActiveStock.Symbol));
        isModalOpened = true;
        buyModal.OnQuantitySubmit += HandleBuyModalSubmit;
        buyModal.OnExit += HandleModalExit;
    }

    private void DisplaySellModal() {
        AlertModal modal = Instantiate(AlertModalPrefab, transform.root, false);
        modal.SetTitle(String.Format("SELL {0}", market.ActiveStock.Symbol));
        modal.SetMessage("Sell all stocks?");
        isModalOpened = true;
        modal.OnSubmit += HandleSellModalSubmit;
        modal.OnExit += HandleModalExit;
    }

    private void DisplayInsufficientFundsModal(string stockSymbol, int quantity) {
        AlertModal modal = Instantiate(AlertModalPrefab, transform.root, false);
        modal.SetTitle("WARNING");
        modal.SetMessage(
            String.Format("Insufficient funds to buy {0} {1} stocks", quantity, stockSymbol)
        );
        isModalOpened = true;
        modal.OnSubmit += HandleCantAffordModalExit;
        modal.OnExit += HandleCantAffordModalExit;
    }

    private void HandleBuyModalSubmit(int quantity) {
        HandleModalExit();
        Stock stock = market.ActiveStock;
        if (player.Affords(stock, quantity)) {
            player.Buy(stock, quantity);
        }
        else {
            DisplayInsufficientFundsModal(stock.Symbol, quantity);
        }
    }

    private void HandleCantAffordModalExit() {
        DisplayBuyModal();
    }

    private void HandleSellModalSubmit() {
        HandleModalExit();
        AudioSource.PlayClipAtPoint(SellSoundEffect, Vector3.one);
        player.Sell(market.ActiveStock);
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
