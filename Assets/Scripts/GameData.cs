using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData {

    private const string BALANCE_KEY = "PlayerBalance";
    private const float BALANCE_DEFAULT = 50000f;

    private const string DAY_COUNT_KEY = "DayCount";
    private const int DAY_COUNT_DEFAULT = 1;

    private const string ACHIEVEMENT_LEVEL_KEY = "AchievementLevel";
    private const int ACHIEVEMENT_LEVEL_DEFAULT = 0;

    private const string RED_KEY = "TerminalColorRed";
    private const float RED_DEFAULT = 0.22f;

    private const string GREEN_KEY = "TerminalColorGreen";
    private const float GREEN_DEFAULT = 0.44f;

    private const string BLUE_KEY = "TerminalColorBlue";
    private const float BLUE_DEFAULT = 0.20f;

    public static float GetBalance() {
        return PlayerPrefs.GetFloat(BALANCE_KEY, BALANCE_DEFAULT);
    }

    public static void SetBalance(float balance) {
        PlayerPrefs.SetFloat(BALANCE_KEY, balance);
    }

    public static int GetDayCount() {
        return PlayerPrefs.GetInt(DAY_COUNT_KEY, DAY_COUNT_DEFAULT);
    }

    public static void IncrementDayCount() {
        PlayerPrefs.SetInt(DAY_COUNT_KEY, GetDayCount() + 1);
    }

    public static int GetAchievementLevel() {
        return PlayerPrefs.GetInt(ACHIEVEMENT_LEVEL_KEY, ACHIEVEMENT_LEVEL_DEFAULT);
    }

    public static void SetAchievementLevel(int level) {
        PlayerPrefs.SetInt(ACHIEVEMENT_LEVEL_KEY, level);
    }

    public static void IncrementAchievementLevel() {
        SetAchievementLevel(GetAchievementLevel() + 1);
    }

    public static Color GetTerminalColor() {
        return new Color(
            PlayerPrefs.GetFloat(RED_KEY, RED_DEFAULT),
            PlayerPrefs.GetFloat(GREEN_KEY, GREEN_DEFAULT),
            PlayerPrefs.GetFloat(BLUE_KEY, BLUE_DEFAULT)
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
