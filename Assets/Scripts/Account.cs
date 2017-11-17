using System.Collections;
using System.Collections.Generic;

public class Account {

    public float InitialBalance { get; private set; }
    public float Balance { get; private set; }

    public Account(float initialBalance) {
        InitialBalance = initialBalance;
        Balance = InitialBalance;
    }

    public void Add(float amount) {
        Balance += amount;
    }

    public void Subtract(float amount) {
        Balance -= amount;
    }

}
