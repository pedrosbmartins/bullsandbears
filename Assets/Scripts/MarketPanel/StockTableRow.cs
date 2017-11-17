using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockTableRow : MonoBehaviour {

    public string AssignedStockSymbol { get; private set; }

    public Text SymbolTextField;
    public Text StocksOwnedTextField;
    public Text VolumeTextField;
    public Text PriceTextField;
    public Text PriceHighlightTextField;
    public Text ChangeTextField;
    public Text TrendTextField;

    public Image PriceHighlightCell;

    private Color defaultCellColor = new Color(0f, 0f, 0f, 0f);
    private Color highlightCellColor = Color.white;

    private Color defaultTextColor = Color.white;
    private Color highlightTextColor = Color.black;

    private Image TextField;

    private Player player;

    private void Awake() {
        TextField = GetComponent<Image>();
    }

    public void Setup(Stock stock, Player player) {
        AssignedStockSymbol = stock.Symbol;
        this.player = player;
        FillInfo(stock);
        stock.OnProcessed += UpdateData;
        this.player.OnBuyStock += HandleOnBuyStock;
        this.player.OnSellStock += HandleOnSellStock;
    }

    public void Select() {
        TextField.color = highlightCellColor;
        SetTextColor(highlightTextColor);
    }

    public void Deselect() {
        TextField.color = defaultCellColor;
        SetTextColor(defaultTextColor);
    }

    private void UpdateData(Stock stock) {
        StartCoroutine(HighlightPriceCell());
        FillInfo(stock);
    }

    private void FillInfo(Stock stock) {
        SymbolTextField.text = stock.Symbol;
        StocksOwnedTextField.text = CalculateOwnedCount().ToString();
        VolumeTextField.text = stock.CurrentVolume().ToString("N2");
        PriceTextField.text = stock.CurrentPrice().ToString("N2");
        ChangeTextField.text = "+0.00";
        TrendTextField.text = stock.CurrentTrend().ToString("N3");
    }

    private int CalculateOwnedCount() {
        if (player.OwnedStocks.ContainsKey(AssignedStockSymbol)) {
            return player.OwnedStocks[AssignedStockSymbol];
        }
        else {
            return 0;
        }
    }

    private void HandleOnBuyStock(Stock stock) {
        if (stock.Symbol == AssignedStockSymbol) {
            StocksOwnedTextField.text = CalculateOwnedCount().ToString();
        }
    }

    private void HandleOnSellStock(Stock stock) {
        if (stock.Symbol == AssignedStockSymbol) {
            StocksOwnedTextField.text = CalculateOwnedCount().ToString();
        }
    }

    private IEnumerator HighlightPriceCell() {
        PriceHighlightTextField.text = PriceTextField.text;
        PriceHighlightCell.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        PriceHighlightCell.gameObject.SetActive(false);
    }

    private void SetTextColor(Color color) {
        SymbolTextField.color = color;
        StocksOwnedTextField.color = color;
        VolumeTextField.color = color;
        PriceTextField.color = color;
        ChangeTextField.color = color;
        TrendTextField.color = color;
    }

}
