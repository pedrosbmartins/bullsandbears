using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour {

    private const float DisplaySecondsAmount = 1.6f;

    [SerializeField] private AudioSource sfx;
    [SerializeField] private Text textLabel;

    public event Action OnFinish = delegate { };

    private void Start() {
        if (GameData.GetSFXOn()) {
            sfx.Play();
        }
        StartCoroutine(Display());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            Finish();
        }
    }

    private IEnumerator Display() {
        textLabel.text = String.Format("Day {0}", GameData.GetDayCount());
        yield return new WaitForSeconds(DisplaySecondsAmount);
        Finish();
    }

    private void Finish() {
        Destroy(gameObject);
        OnFinish();
    }

}
