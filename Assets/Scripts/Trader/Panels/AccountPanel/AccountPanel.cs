using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountPanel : MonoBehaviour {

    private StockMarket market;
    private Player player;

    public Text AccountField;
    public Text BalanceField;
    public Text AssetsField;

    public Text DayChangeField;
    public Text PercentageDayChangeField;

    private void Awake() {
        market = GetComponentInParent<StockMarket>();
        player = GetComponentInParent<Player>();
        player.OnAccountChange += HandleAccountChange;
    }

    private void Start() {
        HandleAccountChange();
    }

    private void HandleAccountChange() {
        UpdateBalanceFields();
        UpdateChangeFields();
    }

    private void UpdateBalanceFields() {
        float balance = player.Account.Balance;
        float assetsValue = CalculateAssetsValue();
        float totalAccount = balance + assetsValue;
        AccountField.text = totalAccount.ToString("N2");
        BalanceField.text = balance.ToString("N2");
        AssetsField.text = assetsValue.ToString("N2");
    }

    private void UpdateChangeFields() {
        float change = player.Account.Balance - player.Account.InitialBalance;
        float percentageChange = change / player.Account.InitialBalance;
        DayChangeField.text = String.Format("{0}{1}", change >= 0 ? "+" : "", change.ToString("N2"));
        PercentageDayChangeField.text = String.Format("{0}{1}", percentageChange >= 0 ? "+" : "", percentageChange.ToString("P1"));
    }

    private float CalculateAssetsValue() {
        float assetsValue = 0;
        foreach (var asset in player.OwnedStocks) {
            var stockSymbol = asset.Key;
            var quantity = Math.Abs(asset.Value); // shorted quantities are negative
            assetsValue += market.GetStock(stockSymbol).CurrentPrice() * quantity;
        }
        return assetsValue;
    }

}
