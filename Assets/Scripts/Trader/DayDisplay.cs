using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour {

    private const float DISPLAY_SECONDS = 1.6f;

    public AudioSource SFX;
    public Text Label;

    public event Action OnHide = delegate { };

    private void Start() {
        if (GameData.GetSFXOn()) {
            SFX.Play();
        }
        StartCoroutine(Display());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            Hide();
        }
    }

    private IEnumerator Display() {
        Label.text = String.Format("Day {0}", GameData.GetDayCount());
        yield return new WaitForSeconds(DISPLAY_SECONDS);
        Hide();
    }

    private void Hide() {
        Destroy(gameObject);
        OnHide();
    }

}
