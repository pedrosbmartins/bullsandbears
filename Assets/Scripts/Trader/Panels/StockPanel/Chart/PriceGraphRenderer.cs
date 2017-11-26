using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceGraphRenderer : StockChartLineRenderer {

    public override void Draw(Stock stock, int? ceilingMargin, int? positionsCount) {
        lineRenderer.numPositions = (int)positionsCount;

        List<float> recentPriceHistory;

        if (stock.PriceHistory.Count <= positionsCount) {
            recentPriceHistory = stock.PriceHistory;
        }
        else {
            int rangeStartIndex = stock.PriceHistory.Count - (int)positionsCount;
            recentPriceHistory = stock.PriceHistory.GetRange(rangeStartIndex, (int)positionsCount);
        }

        var positions = new Vector3[(int)positionsCount];
        float incrementX = width / ((int)positionsCount - 1);

        for (int i = 0; i < recentPriceHistory.Count; i++) {
            float x = incrementX * i;
            float y = height * (recentPriceHistory[i] / (stock.Ceiling + (int)ceilingMargin));
            positions[i] = new Vector3(x, y);
        }

        if (recentPriceHistory.Count < positionsCount) {
            for (int i = recentPriceHistory.Count; i < positionsCount; i++) {
                positions[i] = positions[recentPriceHistory.Count - 1];
            }
        }

        lineRenderer.SetPositions(positions);
    }

}
