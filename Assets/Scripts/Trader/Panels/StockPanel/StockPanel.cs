using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockPanel : MonoBehaviour {

    private StockMarket market;

    private const string EMPTY_VALUE = "...";

    public Text SymbolContainer;
    public Text CompanyNameContainer;
    public Text IndustryContainer;

    public Text CeilingContainer;
    public Text PriceContainer;
    public Text VolumeContainer;
    public Text TrendContainer;

    public StockChart Chart;

    private void Awake() {
        market = GetComponentInParent<StockMarket>();
        market.OnActiveStockProcessed += HandleActiveStockChanged;
        market.OnActiveStockCleared += HandleActiveStockCleared;
    }

    private void HandleActiveStockChanged(Stock stock) {
        FillInfo(stock);
        Chart.Draw(stock);
    }

    private void HandleActiveStockCleared() {
        ClearInfo();
        Chart.Clear();
    }

    private void FillInfo(Stock stock) {
        SymbolContainer.text = stock.Symbol;
        CompanyNameContainer.text = stock.CompanyName;
        IndustryContainer.text = FormattedIndustryString(stock.CompanyIndustry);
        CeilingContainer.text = stock.Ceiling.ToString("N2");
        PriceContainer.text = stock.CurrentPrice().ToString("N2");
        VolumeContainer.text = stock.CurrentVolume().ToString("N2") + "M";
        TrendContainer.text = stock.CurrentTrend().ToString("N3");
    }

    private void ClearInfo() {
        SymbolContainer.text = EMPTY_VALUE;
        CompanyNameContainer.text = EMPTY_VALUE;
        IndustryContainer.text = EMPTY_VALUE;
        CeilingContainer.text = EMPTY_VALUE;
        PriceContainer.text = EMPTY_VALUE;
        VolumeContainer.text = EMPTY_VALUE;
        TrendContainer.text = EMPTY_VALUE;
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
