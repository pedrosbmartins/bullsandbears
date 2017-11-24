using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StockChartLineRenderer : MonoBehaviour {

    protected LineRenderer lineRenderer;
    protected float width;
    protected float height;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private IEnumerator Start() {
        yield return null; // waits for one frame (otherwise, width and height returns 0)
        width = GetComponent<RectTransform>().rect.width;
        height = GetComponent<RectTransform>().rect.height;
    }

    public abstract void Draw(Stock stock, int? ceilingMargin, int? pointsCount);

    public void Clear() {
        var positions = new Vector3[lineRenderer.numPositions];
        lineRenderer.GetPositions(positions);
        for (int i = 0; i < positions.Length; i++) {
            positions[i] = new Vector3(0, 0);
        }
        lineRenderer.SetPositions(positions);
    }

}
