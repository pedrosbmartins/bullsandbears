using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountPanel : MonoBehaviour {

    [SerializeField] private Text accountField;
    [SerializeField] private Text balanceField;
    [SerializeField] private Text assetsField;

    [SerializeField] private Text dayChangeField;
    [SerializeField] private Text percentageDayChangeField;

    private StockMarket market;
    private Player player;

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
        accountField.text = totalAccount.ToString("N2");
        balanceField.text = balance.ToString("N2");
        assetsField.text = assetsValue.ToString("N2");
    }

    private void UpdateChangeFields() {
        float change = player.Account.Balance - player.Account.InitialBalance;
        float percentageChange = change / player.Account.InitialBalance;
        dayChangeField.text = String.Format("{0}{1}", change >= 0 ? "+" : "", change.ToString("N2"));
        percentageDayChangeField.text = String.Format("{0}{1}", percentageChange >= 0 ? "+" : "", percentageChange.ToString("P1"));
    }

    private float CalculateAssetsValue() {
        float assetsValue = 0;
        foreach (var asset in player.Portfolio) {
            var stockSymbol = asset.Key;
            // using absolute value because shorted quantities are negative
            var quantity = Math.Abs(asset.Value);
            assetsValue += market.GetStock(stockSymbol).CurrentPrice() * quantity;
        }
        return assetsValue;
    }

}
