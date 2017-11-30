using System;

public class News {

    public Industry Industry { get; private set; }
    public string Headline { get; private set; }
    public EffectDirection PriceEffectDirection { get; private set; }
    public EffectStrength PriceEffectStrength { get; private set; }

    public News(Industry industry, string headline, EffectDirection direction, EffectStrength strength) {
        Industry = industry;
        Headline = headline;
        PriceEffectDirection = direction;
        PriceEffectStrength = strength;
    }

}
