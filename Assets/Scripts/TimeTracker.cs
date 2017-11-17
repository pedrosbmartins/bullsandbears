using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class TimeTracker : MonoBehaviour {

    public Text TimeField;
    public double MarketDayDurationInSeconds = 90;
    public bool InfiniteDay = false;

    public DateTime MarketOpenTime = DateTime.Today.AddHours(9.5);
    public DateTime MarketCloseTime = DateTime.Today.AddHours(17);

    public delegate void MarketClosedHandler();
    public event MarketClosedHandler OnMarkedClosed;

    private DateTime currentTime;

    public void StartTracking() {
        SetCurrentTime(MarketOpenTime);
        UpdateTimeField();
        TriggerTimeTick();
    }

    private void SetCurrentTime(DateTime time) {
        currentTime = time;
    }

    public DateTime GetCurrentTime() {
        return currentTime;
    }

    private void UpdateTimeField() {
        TimeField.text = currentTime.ToString("hh:mm tt");
    }

    private void TriggerTimeTick() {
        double tickDuration = CalculateTickDuration();
        StartCoroutine(
            TimeTick((float)tickDuration)
        );
    }

    private double CalculateTickDuration() {
        double totalMinutes = MarketCloseTime.Subtract(MarketOpenTime).TotalMinutes;
        double duration = MarketDayDurationInSeconds / totalMinutes;
        return duration;
    }

    private IEnumerator TimeTick(float tickDuration) {
        while (InfiniteDay || currentTime < MarketCloseTime) {
            yield return new WaitForSeconds(tickDuration);
            SetCurrentTime(currentTime.AddMinutes(1f));
            UpdateTimeField();
        }
        if (OnMarkedClosed != null) OnMarkedClosed();
    }

}