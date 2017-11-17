using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceGraphRenderer : StockChartLineRenderer {

    public override void Draw(Stock stock, int? ceilingMargin, int? pointsCount) {
        List<float> recentPriceHistory;

        if (stock.PriceHistory.Count <= pointsCount) {
            recentPriceHistory = stock.PriceHistory;
        }
        else {
            int rangeStartIndex = stock.PriceHistory.Count - (int)pointsCount;
            recentPriceHistory = stock.PriceHistory.GetRange(rangeStartIndex, (int)pointsCount);
        }

        var positions = new Vector3[(int)pointsCount];
        float incrementX = width / ((int)pointsCount - 1);

        for (int i = 0; i < recentPriceHistory.Count; i++) {
            float x = incrementX * i;
            float y = height * (recentPriceHistory[i] / (stock.Ceiling + (int)ceilingMargin));
            positions[i] = new Vector3(x, y);
        }

        if (recentPriceHistory.Count < pointsCount) {
            for (int i = recentPriceHistory.Count; i < pointsCount; i++) {
                positions[i] = positions[recentPriceHistory.Count - 1];
            }
        }

        lineRenderer.SetPositions(positions);
    }

}
