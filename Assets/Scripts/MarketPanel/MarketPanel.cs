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

    public StockMarket Market;
    public Player Player;

    public StockTable Table;
    public KeyBindingPanel KeyBindingPanel;

    public Transform ModalContainer;
    public BuyModal BuyModalPrefab;
    public AlertModal AlertModalPrefab;

    public MarketPanelContext CurrentContext;

    private bool isModalOpened = false;

    private void Awake() {
        SetContext(MarketPanelContext.Idle);
        Market.OnMarketDayStarted += HandleMarketDayStarted;
        Market.OnStockAdded += HandleStockAdded;
        Table.OnRowSelected += HandleTableRowSelected;
        Table.OnRowSelectionCleared += HandleTableRowSelectionCleared;
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
                if (Market.ActiveStock == null || Market.CurrentState != MarketState.Open) return;
                DisplayBuyModal();
                break;
            case KeyBindingAction.Sell:
                if  (Market.ActiveStock == null 
                 || !Player.OwnedStocks.ContainsKey(Market.ActiveStock.Symbol)
                 ||  Player.OwnedStocks[Market.ActiveStock.Symbol] == 0) {
                    return;
                }
                DisplaySellModal();
                break;
        }
    }

    private void DisplayBuyModal() {
        BuyModal buyModal = Instantiate(BuyModalPrefab, ModalContainer, false);
        buyModal.SetTitle(String.Format("BUY {0}", Market.ActiveStock.Symbol));
        isModalOpened = true;
        buyModal.OnQuantitySubmit += HandleBuyModalSubmit;
        buyModal.OnExit += HandleModalExit;
    }

    private void DisplaySellModal() {
        AlertModal modal = Instantiate(AlertModalPrefab, ModalContainer, false);
        modal.SetTitle(String.Format("SELL {0}", Market.ActiveStock.Symbol));
        modal.SetMessage("Sell all stocks?");
        isModalOpened = true;
        modal.OnSubmit += HandleSellModalSubmit;
        modal.OnExit += HandleModalExit;
    }

    private void DisplayInsufficientFundsModal(string stockSymbol, int quantity) {
        AlertModal modal = Instantiate(AlertModalPrefab, ModalContainer, false);
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
        Stock stock = Market.ActiveStock;
        if (Player.Affords(stock, quantity)) {
            Player.Buy(stock, quantity);
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
        Player.Sell(Market.ActiveStock);
    }

    private void HandleModalExit() {
        isModalOpened = false;
    }

    private void DisplayHelpMessages() {
        var messages = new string[] {
            "B&B Tutorial:",
            "[up] / [down] arrows select stocks",
            "[F2] to buy, [F3] to sell",
            "TREND shows price trend direction",
            "VOLUME indicates trend strenth",
            "Watchout for the stock's CEILING:",
            "It may lose value quickly around it",
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
        Table.InsertRow(stock, Player);
    }

    private void HandleTableRowSelected(StockTableRow row) {
        if (Market.CurrentState == MarketState.Open) {
            SetContext(MarketPanelContext.RowSelectedAndMarketActive);
        }
        else {
            SetContext(MarketPanelContext.RowSelected);
        }
        Market.SetActiveStock(row.AssignedStockSymbol);
    }

    private void HandleTableRowSelectionCleared() {
        SetContext(MarketPanelContext.Idle);
        Market.ClearActiveStock();
    }

}
