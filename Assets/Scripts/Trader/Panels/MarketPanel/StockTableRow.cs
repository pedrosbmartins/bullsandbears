using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockTableRow : MonoBehaviour {

    [SerializeField] private Text symbolTextField;
    [SerializeField] private Text stocksOwnedTextField;
    [SerializeField] private Text volumeTextField;
    [SerializeField] private Text priceTextField;
    [SerializeField] private Text priceHighlightTextField;
    [SerializeField] private Text changeTextField;
    [SerializeField] private Text trendTextField;
    [SerializeField] private Image priceHighlightCell;

    public string AssignedStockSymbol { get; private set; }

    private Color defaultCellColor = new Color(0f, 0f, 0f, 0f);
    private Color highlightCellColor = Color.white;

    private Color defaultTextColor = Color.white;
    private Color highlightTextColor = Color.black;

    private Image textField;

    private Player player;

    private void Awake() {
        textField = GetComponent<Image>();
        player = GetComponentInParent<Player>();
    }

    public void Setup(Stock stock) {
        AssignedStockSymbol = stock.Symbol;
        FillInfo(stock);
        stock.OnProcess += UpdateData;
        player.OnBuyStock += OwnedStocksCountChanged;
        player.OnSellStock += OwnedStocksCountChanged;
        player.OnShortStock += OwnedStocksCountChanged;
    }

    public void Select() {
        textField.color = highlightCellColor;
        SetTextColor(highlightTextColor);
    }

    public void Deselect() {
        textField.color = defaultCellColor;
        SetTextColor(defaultTextColor);
    }

    private void UpdateData(Stock stock) {
        StartCoroutine(HighlightPriceCell());
        FillInfo(stock);
    }

    private void FillInfo(Stock stock) {
        symbolTextField.text = stock.Symbol;
        stocksOwnedTextField.text = CalculateOwnedCount().ToString();
        volumeTextField.text = stock.CurrentVolume().ToString("N2");
        priceTextField.text = stock.CurrentPrice().ToString("N2");
        changeTextField.text = stock.CurrentPriceChange().ToString("N2");
        trendTextField.text = stock.CurrentTrend().ToString("N3");
    }

    private int CalculateOwnedCount() {
        if (player.Portfolio.ContainsKey(AssignedStockSymbol)) {
            return player.Portfolio[AssignedStockSymbol];
        }
        else {
            return 0;
        }
    }

    private void OwnedStocksCountChanged(Stock stock) {
        if (stock.Symbol == AssignedStockSymbol) {
            stocksOwnedTextField.text = CalculateOwnedCount().ToString();
        }
    }

    private IEnumerator HighlightPriceCell() {
        priceHighlightTextField.text = priceTextField.text;
        priceHighlightCell.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        priceHighlightCell.gameObject.SetActive(false);
    }

    private void SetTextColor(Color color) {
        symbolTextField.color = color;
        stocksOwnedTextField.color = color;
        volumeTextField.color = color;
        priceTextField.color = color;
        changeTextField.color = color;
        trendTextField.color = color;
    }

}
