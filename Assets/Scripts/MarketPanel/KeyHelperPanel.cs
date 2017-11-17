using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHelperPanel : MonoBehaviour {

    public KeyHelperItem KeyBindingItemPrefab;

    public List<KeyBinding> KeyBindings = new List<KeyBinding>();

    private void Awake() {
        KeyBindings.Add(HelpKeyBinding());
        KeyBindings.Add(BuyKeyBinding());
        KeyBindings.Add(SellKeyBinding());
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
                KeyHelperItem item = Instantiate(KeyBindingItemPrefab, transform, false);
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

}
