using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindingPanel : MonoBehaviour {

    [SerializeField] private KeyBindingItem keyBindingItemPrefab;

    public List<KeyBinding> KeyBindings = new List<KeyBinding>();

    private void Awake() {
        KeyBindings.Add(HelpKeyBinding());
        KeyBindings.Add(BuyKeyBinding());
        KeyBindings.Add(SellKeyBinding());
        
        if (GameAchievements.IsMechanicUnlocked(Mechanic.Short)) {
            KeyBindings.Add(ShortKeyBinding());
        }
    }

    private void Update() {
        if (KeyBindings.Find(binding => binding.Action == KeyBindingAction.Short) == null 
         && GameAchievements.IsMechanicUnlocked(Mechanic.Short)) {
            KeyBindings.Add(ShortKeyBinding());
        }
    }

    public void Render(MarketPanelContext currentContext) {
        DeleteAllItems();
        RenderItemsByContext(currentContext);
    }

    private void DeleteAllItems() {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void RenderItemsByContext(MarketPanelContext context) {
        KeyBindings.ForEach(binding => {
            if (binding.Contexts.Contains(context)) {
                KeyBindingItem item = Instantiate(keyBindingItemPrefab, transform, false);
                item.SetBinding(binding);
            }
        });
    }

    private KeyBinding HelpKeyBinding() {
        return new KeyBinding(
            KeyBindingAction.Help,
            KeyCode.F1,
            new List<MarketPanelContext>() {
                MarketPanelContext.Idle,
                MarketPanelContext.RowSelected,
                MarketPanelContext.RowSelectedAndMarketActive
            }
        );
    }

    private KeyBinding BuyKeyBinding() {
        return new KeyBinding(
            KeyBindingAction.Buy,
            KeyCode.F2,
            new List<MarketPanelContext>() { MarketPanelContext.RowSelectedAndMarketActive }
        );
    }

    private KeyBinding SellKeyBinding() {
        return new KeyBinding(
            KeyBindingAction.Sell,
            KeyCode.F3,
            new List<MarketPanelContext>() { MarketPanelContext.RowSelectedAndMarketActive }
        );
    }

    private KeyBinding ShortKeyBinding() {
        return new KeyBinding(
            KeyBindingAction.Short,
            KeyCode.F4,
            new List<MarketPanelContext>() { MarketPanelContext.RowSelectedAndMarketActive }
        );
    }

}
