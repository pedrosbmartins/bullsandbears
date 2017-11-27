using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum KeyBindingAction { Help, Buy, Sell, Short }

public class KeyBinding {

    public KeyBindingAction Action { get; private set; }
    public KeyCode Key { get; private set; }
    public List<MarketPanelContext> Contexts { get; private set; }

    public KeyBinding(KeyBindingAction action, KeyCode key, List<MarketPanelContext> contexts) {
        Action = action;
        Key = key;
        Contexts = contexts;
    }

}