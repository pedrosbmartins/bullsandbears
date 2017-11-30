using System;

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
