using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingLineRenderer : StockChartLineRenderer {

    private IEnumerator Start() {
        yield return null; // waits for one frame (otherwise, width and height returns 0)
        width = GetComponent<RectTransform>().rect.width;
        height = GetComponent<RectTransform>().rect.height;
    }

    public override void Draw(Stock stock, int? ceilingMargin, int? pointsCount = null) {
        var positions = new Vector3[2];
        float y = height * (stock.Ceiling / (stock.Ceiling + (int)ceilingMargin));
        positions[0] = new Vector3(0, y);
        positions[1] = new Vector3(width, y);
        lineRenderer.SetPositions(positions);
    }

}
