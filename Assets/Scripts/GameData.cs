using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData {

    #region AccountBalance
    private const string AccountBalanceKey = "PlayerAccountBalance";
    private const float AccountBalanceDefault = 50000f;

    public static float GetAccountBalance() {
        return PlayerPrefs.GetFloat(AccountBalanceKey, AccountBalanceDefault);
    }

    public static void SetAccountBalance(float balance) {
        PlayerPrefs.SetFloat(AccountBalanceKey, balance);
    }
    #endregion

    #region DayCount
    private const string DayCountKey = "DayCount";
    private const int DayCountDefault = 1;

    public static int GetDayCount() {
        return PlayerPrefs.GetInt(DayCountKey, DayCountDefault);
    }

    public static void IncrementDayCount() {
        PlayerPrefs.SetInt(DayCountKey, GetDayCount() + 1);
    }
    #endregion

    #region AchievementLevel
    private const string AchievementLevelKey = "AchievementLevel";
    private const int AchievementLevelDefault = 0;

    public static int GetAchievementLevel() {
        return PlayerPrefs.GetInt(AchievementLevelKey, AchievementLevelDefault);
    }

    public static void SetAchievementLevel(int level) {
        PlayerPrefs.SetInt(AchievementLevelKey, level);
    }

    public static void IncrementAchievementLevel() {
        SetAchievementLevel(GetAchievementLevel() + 1);
    }
    #endregion

    #region Sound
    private const string MusicOnKey = "MusicOn";
    private const bool MusicOnDefault = true;

    private const string SFXOnKey = "SFXOn";
    private const bool SFXOnDefault = true;

    public static bool GetMusicOn() {
        return GetBool(MusicOnKey, MusicOnDefault);
    }

    public static void SetMusicOn(bool musicOn) {
        SetBool(MusicOnKey, musicOn);
    }

    public static bool GetSFXOn() {
        return GetBool(SFXOnKey, SFXOnDefault);
    }

    public static void SetSFXOn(bool sfxOn) {
        SetBool(SFXOnKey, sfxOn);
    }
    #endregion

    #region TerminalColor
    private const string ColorRedKey = "TerminalColorRed";
    private const float ColorRedDefault = 0.22f;

    private const string ColorGreenKey = "TerminalColorGreen";
    private const float ColorGreenDefault = 0.44f;

    private const string ColorBlueKey = "TerminalColorBlue";
    private const float ColorBlueDefault = 0.20f;

    public static Color GetTerminalColor() {
        return new Color(
            PlayerPrefs.GetFloat(ColorRedKey, ColorRedDefault),
            PlayerPrefs.GetFloat(ColorGreenKey, ColorGreenDefault),
            PlayerPrefs.GetFloat(ColorBlueKey, ColorBlueDefault)
        );
    }

    public static void SetTerminalColor(Color color) {
        PlayerPrefs.SetFloat(ColorRedKey, color.r);
        PlayerPrefs.SetFloat(ColorGreenKey, color.g);
        PlayerPrefs.SetFloat(ColorBlueKey, color.b);
    }
    #endregion

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
