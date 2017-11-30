using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class GameAchievements {

    public static bool ForceUnlockMechanics = false;

    private const float FirstLevelBalanceTarget = 100000f;
    private const float SecondLevelBalanceTarget = 1000000f;

    public static readonly IList<Achievement> Levels = new ReadOnlyCollection<Achievement>
        (new List<Achievement>() {
            new Achievement(new Mechanic[] { }, FirstLevelBalanceTarget),
            new Achievement(new Mechanic[] { Mechanic.News }, SecondLevelBalanceTarget),
            new Achievement(new Mechanic[] { Mechanic.News, Mechanic.Short }, 0),
        });

    public static Achievement Current() {
        return Levels[CurrentAchievementLevel()];
    }

    private static int CurrentAchievementLevel() {
        return GameData.GetAchievementLevel();
    }

    public static bool IsInLastAchievementLevel() {
        return CurrentAchievementLevel() == Levels.Count - 1;
    }

    public static bool IsCurrentAchievementComplete(float balance) {
        return balance >= Current().BalanceTarget;
    }

    public static void Check(float balance) {
        if (IsInLastAchievementLevel()) {
            return;
        }
        else if (IsCurrentAchievementComplete(balance)) {
            GameData.IncrementAchievementLevel();
            ShowMessages();
        }
    }

    private static void ShowMessages() {
        var messages = new List<string> {
            "Congratulations!",
            "You reached your balance target"
        };

        if (Current().UnlockableMechanic == Mechanic.News) {
            messages.AddRange(GetNewsMechanicUnlockedMessages());
        }
        else if (Current().UnlockableMechanic == Mechanic.Short) {
            messages.AddRange(GetShortMechanicUnlockedMessages());
        }

        MessageCentral.Instance.DisplayMessages("Message", messages.ToArray(), true);
    }

    private static string[] GetNewsMechanicUnlockedMessages() {
        return new string[] {
            "You have unlocked the news system",
            "News headlines will show up here",
            "They may impact stocks performance",
        };
    }

    private static string[] GetShortMechanicUnlockedMessages() {
        return new string[] {
            "You can now short stocks",
            "When a stock's trend is negative",
            "You can short and then buy it later",
            "Turning a profit in the process",
        };
    }

    public static bool IsMechanicUnlocked(Mechanic mechanic) {
        if (ForceUnlockMechanics) {
            return true;
        }
        return Array.IndexOf(Current().Mechanics, mechanic) != -1;
    }

}