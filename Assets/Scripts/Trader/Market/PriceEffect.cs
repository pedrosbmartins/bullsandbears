using System;

public enum EffectDirection { Positive, Negative };
public enum EffectStrength { Strong, Weak };

public class PriceEffect {
    public Industry AffectedIndustry { get; private set; }
    public EffectDirection Direction { get; private set; }
    public EffectStrength Strength { get; private set; }

    public PriceEffect(Industry industry, EffectDirection direction, EffectStrength strength) {
        AffectedIndustry = industry;
        Direction = direction;
        Strength = strength;
    }
}
