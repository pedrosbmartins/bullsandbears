using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockPanel : MonoBehaviour {

    [SerializeField] private Text symbolField;
    [SerializeField] private Text companyNameField;
    [SerializeField] private Text industryField;

    [SerializeField] private Text ceilingField;
    [SerializeField] private Text priceField;
    [SerializeField] private Text volumeField;
    [SerializeField] private Text trendField;

    [SerializeField] private StockChart chart;

    private StockMarket market;

    private const string placeholderValue = "...";

    private void Awake() {
        market = GetComponentInParent<StockMarket>();
        market.OnActiveStockProcessed += HandleActiveStockChanged;
        market.OnActiveStockCleared += HandleActiveStockCleared;
    }

    private void HandleActiveStockChanged(Stock stock) {
        FillInfo(stock);
        chart.Draw(stock);
    }

    private void HandleActiveStockCleared() {
        ClearInfo();
        chart.Clear();
    }

    private void FillInfo(Stock stock) {
        symbolField.text = stock.Symbol;
        companyNameField.text = stock.CompanyName;
        industryField.text = FormattedIndustryString(stock.CompanyIndustry);
        ceilingField.text = stock.Ceiling.ToString("N2");
        priceField.text = stock.CurrentPrice().ToString("N2");
        volumeField.text = stock.CurrentVolume().ToString("N2") + "M";
        trendField.text = stock.CurrentTrend().ToString("N3");
    }

    private void ClearInfo() {
        symbolField.text = placeholderValue;
        companyNameField.text = placeholderValue;
        industryField.text = placeholderValue;
        ceilingField.text = placeholderValue;
        priceField.text = placeholderValue;
        volumeField.text = placeholderValue;
        trendField.text = placeholderValue;
    }

    private string FormattedIndustryString(Industry industry) {
        switch (industry) {
            case Industry.BanksAndFinance:
                return "Banks & Finance";
            case Industry.OilAndGas:
                return "Oil & Gas";
            default:
                return industry.ToString();
        }
    }

}
