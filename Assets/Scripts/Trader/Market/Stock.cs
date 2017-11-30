using System;
using System.Collections.Generic;
using System.Linq;

public class Stock {

    public event Action<Stock> OnProcess = delegate { };

    private const float CeilingMaxValue = 150f;
    private const float VolumeMaxValue = 100f;

    private const float AboveCeilingMargin = 10f; // maximum possible price is always Ceiling + this margin
    private const float CeilingProximityThreshold = 5f;
    private const float MinimumPrice = 1f;

    // the 'target approach delay' is the number of process cycles it 
    // should take for the price to approach the target value
    private const int TargetApproachDelayMin = 8;
    private const int TargetApproachDelayMax = 15;
    private int currentTargetApproachDelay = TargetApproachDelayMin;

    private const float TargetApproachMargin = 1f;
    private const float TargetApproachFluctuation = 0.1f;

    public readonly string Symbol;
    public readonly string CompanyName;
    public readonly Industry CompanyIndustry;

    public float Ceiling { get; private set; }
    
    public List<float> PriceHistory { get; private set; }
    public List<float> VolumeHistory { get; private set; }
    public List<float> TrendHistory { get; private set; }

    private float priceTarget;
    private float preMovementPrice; // remembers the price before a movement towards the target begins

    private float? lastPriceToTargetDistance = null; // keeps tracks of the last |Price - Target| value, to see if it is increasing (it should never increase!)

    private IRandomGenerator randomGenerator;

    private PriceEffect currentExternalEffect;

    public Stock(string symbol, string companyName, Industry industry, IRandomGenerator randomGenerator) {
        UnitySystemConsoleRedirector.Redirect(); // redirects System.Console output to Unity's console

        Symbol = symbol;
        CompanyName = companyName;
        CompanyIndustry = industry;

        this.randomGenerator = randomGenerator;

        SetInitialValues();
    }

    private void SetInitialValues() {
        PriceHistory = new List<float>();
        VolumeHistory = new List<float>();
        TrendHistory = new List<float>();
        SetCeiling();
        SetInitialPrice();
        SetNewTrendEffectAndPriceTarget();
    }

    private void SetCeiling() {
        Ceiling = randomGenerator.NextRandomFloat(CeilingMaxValue / 2f, CeilingMaxValue);
    }

    private void SetInitialPrice() {
        PriceHistory.Add(Ceiling - randomGenerator.NextRandomFloat(0f, Ceiling));
    }

    private void SetNewTrendEffectAndPriceTarget() {
        SetNewTrendEffect();
        SetNewPriceTarget();
    }

    private void SetNewTrendEffect() {
        preMovementPrice = CurrentPrice();
        SetTargetApproachDelay();
        GenerateNewVolume();
        GenerateNewTrend();
    }

    private void SetNewPriceTarget() {
        float trendEffect = CurrentVolume() * CurrentTrend();
        float priceTargetValue = CurrentPrice() + trendEffect;
        float maximumPrice = Ceiling + AboveCeilingMargin;
        priceTarget = (priceTargetValue <= 0f) ? MinimumPrice : (priceTargetValue >= maximumPrice) ? maximumPrice : priceTargetValue;
        lastPriceToTargetDistance = null; // reset last distance, since target changed
    }

    private void SetTargetApproachDelay() {
        currentTargetApproachDelay = (int)Math.Floor(
            randomGenerator.NextRandomFloat(TargetApproachDelayMin, TargetApproachDelayMax)
        );
    }

    private void GenerateNewVolume() {
        float newVolume = currentExternalEffect == null 
                        ? RandomVolume() 
                        : CalculateVolumeWithEffect();
        VolumeHistory.Add(newVolume);
    }

    private float RandomVolume() {
        return randomGenerator.NextRandomFloat(0f, VolumeMaxValue);
    }

    private float CalculateVolumeWithEffect() {
        float maxVolume = VolumeMaxValue;
        float halfMaxVolume = VolumeMaxValue / 2;
        float oneThirdMaxVolume = VolumeMaxValue / 3;

        float volume;

        if (currentExternalEffect.Strength == EffectStrength.Strong) {
            volume = randomGenerator.NextRandomFloat(halfMaxVolume, maxVolume);
        }
        else {
            volume = randomGenerator.NextRandomFloat(oneThirdMaxVolume, halfMaxVolume);
        }

        return volume;
    }

    private void GenerateNewTrend() {
        float newTrend = currentExternalEffect == null 
                       ? CalculateTrend() 
                       : CalculateTrendWithEffect();
        TrendHistory.Add(newTrend);
    }

    private float CalculateTrend() {
        float trend;

        if (TrendHistory.Count == 0) {
            return randomGenerator.NextRandomFloat(-1f, 1f);
        }

        if (IsPriceTooCloseToCeiling()) {
            trend = randomGenerator.NextRandomFloat(-1f, -0.75f); // force price down
            currentTargetApproachDelay = TargetApproachDelayMax;
        }
        else {
            if (IsCurrentTrendTooStrongOrTooWeak()) {
                trend = randomGenerator.NextRandomFloat(-1f, 1f); // new trend is random
            }
            else {
                trend = CurrentTrend() + randomGenerator.NextRandomFloat(0f, 0.2f); // new trend is similar to current one
            }
        }

        return trend;
    }

    private float CalculateTrendWithEffect() {
        var direction = currentExternalEffect.Direction;
        var strength = currentExternalEffect.Strength;

        var signal = direction == EffectDirection.Positive ? 1 : -1;
        float trend;

        if (strength == EffectStrength.Strong) {
            trend = signal * randomGenerator.NextRandomFloat(0.75f, 1f);
        }
        else {
            trend = signal * randomGenerator.NextRandomFloat(0.25f, 0.75f);
        }

        return trend;
    }

    private bool IsPriceTooCloseToCeiling() {
        float ceilingDistance = Ceiling - CurrentPrice();
        return ceilingDistance <= CeilingProximityThreshold;
    }

    private bool IsCurrentTrendTooStrongOrTooWeak() {
        float currentAbsoluteTrend = Math.Abs(CurrentTrend());
        return currentAbsoluteTrend >= 0.75f || currentAbsoluteTrend <= 0.25f;
    }

    public void Process() {
        float priceTargetApproach = CalculatePriceTargetApproach();
        float newPrice = CurrentPrice() + priceTargetApproach;
        PriceHistory.Add(newPrice);

        if (IsPriceCloseToTarget() || IsPriceToTargetDistanceIncreasing()) {
            SetNewTrendEffectAndPriceTarget();
        }
        else {
            lastPriceToTargetDistance = GetPriceToTargetDistance();
        }

        OnProcess(this);
    }

    public void SetExternalEffect(PriceEffect effect) {
        currentExternalEffect = effect;
        SetNewTrendEffectAndPriceTarget();
    }

    public void ClearExternalEffect() {
        currentExternalEffect = null;
        SetNewTrendEffectAndPriceTarget();
    }

    private float CalculatePriceTargetApproach() {
        float approach = (priceTarget - preMovementPrice) * (1f / currentTargetApproachDelay);
        float fluctuation = randomGenerator.NextRandomFloat(0f, TargetApproachFluctuation);
        return approach + fluctuation;
    }

    private bool IsPriceCloseToTarget() {
        return GetPriceToTargetDistance() <= TargetApproachMargin;
    }

    private bool IsPriceToTargetDistanceIncreasing() {
        // if this happens, the Price Target Approach failed (i.e. the approach margin was too small)
        return (
            lastPriceToTargetDistance != null && 
            GetPriceToTargetDistance() > lastPriceToTargetDistance
        );
    }

    private float GetPriceToTargetDistance() {
        return Math.Abs(CurrentPrice() - priceTarget);
    }

    public float CurrentPrice() {
        return PriceHistory.Last();
    }

    public float CurrentPriceChange() {
        int priceCount = PriceHistory.Count;
        return priceCount <= 1 ? 0f : CurrentPrice() - PriceHistory[priceCount - 2];
    }

    public float CurrentVolume() {
        return VolumeHistory.Last();
    }

    public float CurrentTrend() {
        return TrendHistory.Last();
    }

}
