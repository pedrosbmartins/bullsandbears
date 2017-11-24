using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountPanel : MonoBehaviour {

    public StockMarket Market;
    public Player Player;

    public Text AccountField;
    public Text BalanceField;
    public Text AssetsField;

    public Text DayChangeField;
    public Text PercentageDayChangeField;

    private void Awake() {
        Player.OnAccountChange += HandleAccountChange;
    }

    private void Start() {
        HandleAccountChange();
    }

    private void HandleAccountChange() {
        UpdateBalanceFields();
        UpdateChangeFields();
    }

    private void UpdateBalanceFields() {
        float balance = Player.Account.Balance;
        float assetsValue = CalculateAssetsValue();
        float totalAccount = balance + assetsValue;
        AccountField.text = totalAccount.ToString("N2");
        BalanceField.text = balance.ToString("N2");
        AssetsField.text = assetsValue.ToString("N2");
    }

    private void UpdateChangeFields() {
        float change = Player.Account.Balance - Player.Account.InitialBalance;
        float percentageChange = change / Player.Account.InitialBalance;
        DayChangeField.text = String.Format("{0}{1}", change >= 0 ? "+" : "", change.ToString("N2"));
        PercentageDayChangeField.text = String.Format("{0}{1}", percentageChange >= 0 ? "+" : "", percentageChange.ToString("P1"));
    }

    private float CalculateAssetsValue() {
        float value = 0;
        foreach (var asset in Player.OwnedStocks) {
            var stockSymbol = asset.Key;
            var quantity = asset.Value;
            value += Market.GetStock(stockSymbol).CurrentPrice() * quantity;
        }
        return value;
    }

}
