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

    private const string MUSIC_ON_KEY = "MusicOn";
    private const bool MUSIC_ON_DEFAULT = true;

    private const string SFX_ON_KEY = "SFXOn";
    private const bool SFX_ON_DEFAULT = true;

    #region TerminalColor
    private const string RED_KEY = "TerminalColorRed";
    private const float RED_DEFAULT = 0.22f;

    private const string GREEN_KEY = "TerminalColorGreen";
    private const float GREEN_DEFAULT = 0.44f;

    private const string BLUE_KEY = "TerminalColorBlue";
    private const float BLUE_DEFAULT = 0.20f;
    #endregion

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

    public static bool GetMusicOn() {
        return GetBool(MUSIC_ON_KEY, MUSIC_ON_DEFAULT);
    }

    public static void SetMusicOn(bool musicOn) {
        SetBool(MUSIC_ON_KEY, musicOn);
    }

    public static bool GetSFXOn() {
        return GetBool(SFX_ON_KEY, SFX_ON_DEFAULT);
    }

    public static void SetSFXOn(bool sfxOn) {
        SetBool(SFX_ON_KEY, sfxOn);
    }

    public static void Reset() {
        PlayerPrefs.DeleteAll();
    }

    static void SetBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    static bool GetBool(string key, bool defaultValue) {
        if (PlayerPrefs.HasKey(key)) {
            return GetBool(key);
        }
        return defaultValue;
    }

    static bool GetBool(string key) {
        return PlayerPrefs.GetInt(key) == 1 ? true : false;
    }

}
