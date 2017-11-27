using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum Mechanic { News, Short }

public class Achievement {

    public Mechanic[] Mechanics { get; private set; }
    public Mechanic? UnlockableMechanic { get; private set; }
    public float BalanceTarget { get; private set; }

    public Achievement(Mechanic[] mechanics, float balanceTarget) {
        Mechanics = mechanics;
        UnlockableMechanic = mechanics.Length > 0 ? (Mechanic?)mechanics[mechanics.Length - 1] : null;
        BalanceTarget = balanceTarget;
    }

}

public class GameAchievements {

    public static readonly IList<Achievement> Levels = new ReadOnlyCollection<Achievement>
        (new List<Achievement>() {
            new Achievement(new Mechanic[] { }, 10000),
            new Achievement(new Mechanic[] { Mechanic.News }, 50000),
            new Achievement(new Mechanic[] { Mechanic.News, Mechanic.Short }, 0),
        });

    public static Achievement Current() {
        int levelIndex = GameData.GetAchievementLevel();
        return Levels[levelIndex];
    }

    public static bool IsInLastAchievementLevel() {
        int levelIndex = GameData.GetAchievementLevel();
        return levelIndex == Levels.Count - 1;
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
            messages.AddRange(new string[] {
                "You have unlocked the news system",
                "News headlines will show up here",
                "They may impact stocks performance",
            });
        }
        else if (Current().UnlockableMechanic == Mechanic.Short) {
            messages.AddRange(new string[] {
                "You can now short stocks",
                "When a stock's trend is negative",
                "You can short and then buy it later",
                "Turning a profit in the process",
            });
        }

        MessageCentral.Instance.DisplayMessages("Message", messages.ToArray(), true);
    }

    public static bool IsMechanicUnlocked(Mechanic mechanic) {
        return Array.IndexOf(GameAchievements.Current().Mechanics, mechanic) != -1;
    }

}
