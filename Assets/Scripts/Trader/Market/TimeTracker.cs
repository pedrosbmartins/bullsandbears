using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeTracker : MonoBehaviour {

    [SerializeField] private Text timeField;
    [SerializeField] private double marketDayDurationInSeconds = 90;
    [SerializeField] private bool infiniteDay = false;

    public DateTime MarketOpenTime = DateTime.Today.AddHours(9.5);
    public DateTime MarketCloseTime = DateTime.Today.AddHours(17);

    public event Action OnMarkedDayEnded = delegate { };

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
        timeField.text = currentTime.ToString("hh:mm tt");
    }

    private void TriggerTimeTick() {
        double tickDuration = CalculateTickDuration();
        StartCoroutine(
            TimeTick((float)tickDuration)
        );
    }

    private double CalculateTickDuration() {
        double totalMinutes = MarketCloseTime.Subtract(MarketOpenTime).TotalMinutes;
        double duration = marketDayDurationInSeconds / totalMinutes;
        return duration;
    }

    private IEnumerator TimeTick(float tickDuration) {
        while (infiniteDay || currentTime < MarketCloseTime) {
            yield return new WaitForSeconds(tickDuration);
            SetCurrentTime(currentTime.AddMinutes(1f));
            UpdateTimeField();
        }
        OnMarkedDayEnded();
    }

}