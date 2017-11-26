using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Stock {

    public event Action<Stock> OnProcessed = delegate { };

    private const float CEILING_MAX_VALUE = 150f;
    private const float VOLUME_MAX_VALUE = 100f;

    private const float ABOVE_CEILING_MARGIN = 10f; // maximum possible price is always Ceiling + this margin
    private const float CEILING_PROXIMITY_THRESHOLD = 5f;
    private const float MINIMUM_PRICE = 1f;

    // the 'target approach delay' is the number of process cycles it 
    // should take for the price to approach the target value
    private const int TARGET_APPROACH_DELAY_MIN = 8;
    private const int TARGET_APPROACH_DELAY_MAX = 15;
    private int currentTargetApproachDelay = TARGET_APPROACH_DELAY_MIN;

    private const float TARGET_APPROACH_MARGIN = 1f;
    private const float TARGET_APPROACH_FLUCTUATION = 0.1f;

    public readonly string Symbol;
    public readonly string CompanyName;
    public readonly Industry CompanyIndustry;

    public float Ceiling { get; private set; }
    
    public List<float> PriceHistory { get; private set; }
    public List<float> VolumeHistory { get; private set; }
    public List<float> TrendHistory { get; private set; }

    private float PriceTarget;
    private float PreMovementPrice; // remembers the price before a movement towards the target begins

    private float? lastPriceToTargetDistance = null; // keeps tracks of the last |Price - Target| value, to see if it is increasing (it never should increase!)

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
        Ceiling = randomGenerator.NextRandomFloat(CEILING_MAX_VALUE / 2f, CEILING_MAX_VALUE);
    }

    private void SetInitialPrice() {
        PriceHistory.Add(Ceiling - randomGenerator.NextRandomFloat(0f, Ceiling));
    }

    private void SetNewTrendEffectAndPriceTarget() {
        SetNewTrendEffect();
        SetNewPriceTarget();
    }

    private void SetNewTrendEffect() {
        PreMovementPrice = CurrentPrice();
        SetTargetApproachDelay();
        GenerateNewVolume();
        GenerateNewTrend();
    }

    private void SetNewPriceTarget() {
        float trendEffect = CurrentVolume() * CurrentTrend();
        float priceTarget = CurrentPrice() + trendEffect;
        float maximumPrice = Ceiling + ABOVE_CEILING_MARGIN;
        PriceTarget = (priceTarget <= 0f) ? MINIMUM_PRICE : (priceTarget >= maximumPrice) ? maximumPrice : priceTarget;
        lastPriceToTargetDistance = null; // reset last distance, since target changed
    }

    private void SetTargetApproachDelay() {
        currentTargetApproachDelay = (int)Math.Floor(
            randomGenerator.NextRandomFloat(TARGET_APPROACH_DELAY_MIN, TARGET_APPROACH_DELAY_MAX)
        );
    }

    private void GenerateNewVolume() {
        float newVolume = currentExternalEffect == null 
                        ? RandomVolume() 
                        : CalculateVolumeWithEffect();
        VolumeHistory.Add(newVolume);
    }

    private float RandomVolume() {
        return randomGenerator.NextRandomFloat(0f, VOLUME_MAX_VALUE);
    }

    private float CalculateVolumeWithEffect() {
        float maxVolume = VOLUME_MAX_VALUE;
        float halfMaxVolume = VOLUME_MAX_VALUE / 2;
        float thirdMaxVolume = VOLUME_MAX_VALUE / 3;

        float volume;

        if (currentExternalEffect.Strength == EffectStrength.Strong) {
            volume = randomGenerator.NextRandomFloat(halfMaxVolume, maxVolume);
        }
        else {
            volume = randomGenerator.NextRandomFloat(thirdMaxVolume, halfMaxVolume);
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

        if (PriceTooCloseToCeiling()) {
            trend = randomGenerator.NextRandomFloat(-1f, -0.75f); // force price down
            currentTargetApproachDelay = TARGET_APPROACH_DELAY_MAX;
        }
        else {
            if (CurrentTrendTooStrongOrTooWeak()) {
                trend = randomGenerator.NextRandomFloat(-1f, 1f); // new trend is random
            }
            else {
                trend = CurrentTrend() + randomGenerator.NextRandomFloat(0f, 0.2f); // new trend is similar to current
            }
        }

        return trend;
    }

    private float CalculateTrendWithEffect() {
        var signal = currentExternalEffect.Direction == EffectDirection.Positive ? 1 : -1;
        float trend;

        if (currentExternalEffect.Strength == EffectStrength.Strong) {
            trend = signal * randomGenerator.NextRandomFloat(0.75f, 1f);
        }
        else {
            trend = signal * randomGenerator.NextRandomFloat(0.25f, 0.75f);
        }

        return trend;
    }

    private bool PriceTooCloseToCeiling() {
        float ceilingDistance = Ceiling - CurrentPrice();
        return ceilingDistance <= CEILING_PROXIMITY_THRESHOLD;
    }

    private bool CurrentTrendTooStrongOrTooWeak() {
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
            lastPriceToTargetDistance = PriceToTargetDistance();
        }

        OnProcessed(this);
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
        float approach = (PriceTarget - PreMovementPrice) * (1f / currentTargetApproachDelay);
        float fluctuation = randomGenerator.NextRandomFloat(0f, TARGET_APPROACH_FLUCTUATION);
        return approach + fluctuation;
    }

    private bool IsPriceCloseToTarget() {
        return PriceToTargetDistance() <= TARGET_APPROACH_MARGIN;
    }

    private bool IsPriceToTargetDistanceIncreasing() {
        // if this happens, the Price Target Approach failed (i.e. the approach margin was too small)
        return (
            lastPriceToTargetDistance != null && 
            PriceToTargetDistance() > lastPriceToTargetDistance
        );
    }

    private float PriceToTargetDistance() {
        return Math.Abs(CurrentPrice() - PriceTarget);
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
