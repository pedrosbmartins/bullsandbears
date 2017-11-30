using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : MonoBehaviour {

    [SerializeField] private CRT CRT;

	private void Start () {
        ChangeTerminalColor(GameData.GetTerminalColor());
    }

    public void ChangeTerminalColor(Color color) {
        CRT.rgb1 = new Color(color.r, color.g, color.b);
    }

}
