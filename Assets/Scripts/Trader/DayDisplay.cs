﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour {

    public Text Label;

    public delegate void HideHandler();
    public event HideHandler OnHide = delegate {};

    private void Start() {
        StartCoroutine(Display());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            Hide();
        }
    }

    private IEnumerator Display() {
        Label.text = String.Format("Day {0}", GameData.GetDayCount());
        yield return new WaitForSeconds(5f);
        Hide();
    }

    private void Hide() {
        Destroy(gameObject);
        OnHide();
    }

}