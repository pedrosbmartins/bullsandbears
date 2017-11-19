using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData {

    private static readonly string BALANCE_KEY = "PlayerBalance";
    private static readonly float BALANCE_DEFAULT_VALUE = 50000f;

    private static readonly string DAY_COUNT_KEY = "DayCount";
    private static readonly int DAY_COUNT_DEFAULT_VALUE = 1;

    private static readonly string RED_KEY = "TerminalColorRed";
    private static readonly float RED_DEFAULT_VALUE = 0.22f;

    private static readonly string GREEN_KEY = "TerminalColorGreen";
    private static readonly float GREEN_DEFAULT_VALUE = 0.44f;

    private static readonly string BLUE_KEY = "TerminalColorBlue";
    private static readonly float BLUE_DEFAULT_VALUE = 0.20f;

    public static float GetBalance() {
        return PlayerPrefs.GetFloat(BALANCE_KEY, BALANCE_DEFAULT_VALUE);
    }

    public static void SetBalance(float balance) {
        PlayerPrefs.SetFloat(BALANCE_KEY, balance);
    }

    public static int GetDayCount() {
        return PlayerPrefs.GetInt(DAY_COUNT_KEY, DAY_COUNT_DEFAULT_VALUE);
    }

    public static void IncrementDayCount() {
        int count = PlayerPrefs.GetInt(DAY_COUNT_KEY, DAY_COUNT_DEFAULT_VALUE);
        PlayerPrefs.SetInt(DAY_COUNT_KEY, count + 1);
    }

    public static Color GetTerminalColor() {
        return new Color(
            PlayerPrefs.GetFloat(RED_KEY, RED_DEFAULT_VALUE),
            PlayerPrefs.GetFloat(GREEN_KEY, GREEN_DEFAULT_VALUE),
            PlayerPrefs.GetFloat(BLUE_KEY, BLUE_DEFAULT_VALUE)
        );
    }

    public static void SetTerminalColor(Color color) {
        PlayerPrefs.SetFloat(RED_KEY, color.r);
        PlayerPrefs.SetFloat(GREEN_KEY, color.g);
        PlayerPrefs.SetFloat(BLUE_KEY, color.b);
    }

    public static void Reset() {
        PlayerPrefs.DeleteAll();
    }

}
