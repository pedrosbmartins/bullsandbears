using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockChart : MonoBehaviour {

    private const int PricePointsCount = 100;
    private const int CeilingMargin = 20;

    [SerializeField] private CeilingLineRenderer ceilingLine;
    [SerializeField] private PriceGraphRenderer priceGraph;

    public void Draw(Stock stock) {
        ceilingLine.Draw(stock, CeilingMargin);
        priceGraph.Draw(stock, CeilingMargin, PricePointsCount);
    }

    public void Clear() {
        ceilingLine.Clear();
        priceGraph.Clear();
    }

}
