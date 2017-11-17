using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockChart : MonoBehaviour {

    private const int PRICE_POINTS_COUNT = 100;
    private const int CEILING_MARGIN = 20;

    public CeilingLineRenderer CeilingLine;
    public PriceGraphRenderer PriceGraph;

    public void Draw(Stock stock) {
        CeilingLine.Draw(stock, CEILING_MARGIN);
        PriceGraph.Draw(stock, CEILING_MARGIN, PRICE_POINTS_COUNT);
    }

    public void Clear() {
        CeilingLine.Clear();
        PriceGraph.Clear();
    }

}
