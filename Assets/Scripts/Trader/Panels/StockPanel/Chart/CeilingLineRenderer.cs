using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingLineRenderer : StockChartLineRenderer {

    public override void Draw(Stock stock, int? ceilingMargin, int? pointsCount = null) {
        var positions = new Vector3[2];
        float y = height * (stock.Ceiling / (stock.Ceiling + (int)ceilingMargin));
        positions[0] = new Vector3(0, y);
        positions[1] = new Vector3(width, y);
        lineRenderer.SetPositions(positions);
    }

}
