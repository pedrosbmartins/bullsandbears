using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Stock {

    public delegate void StockProcessedHandler(Stock stock);
    public event StockProcessedHandler OnProcessed = delegate {};

    private const float CEILING_MAX_INITIAL_VALUE = 150f;
    private const float VOLUME_MAX_INITIAL_VALUE = 100f;

    private const float ABOVE_CEILING_MARGIN = 10f;
    private const float CEILING_PROXIMITY_THRESHOLD = 5f;
    private const float MINIMUM_PRICE = 1f;

    private const int TARGET_APPROACH_DELAY_MIN = 8;
    private const int TARGET_APPROACH_DELAY_MAX = 15;

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

    private float? lastPriceToTargetDistance = null; // keeps tracks of the last |Price - Target| value, basically to see if it is increasing (it shouldn't!)
    private int currentTargetApproachDelay = TARGET_APPROACH_DELAY_MIN; // basically how many "process" cycles it should take for the price to approach the target value

    private IRandomGenerator randomGenerator;

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
        GenerateInitialPrice();
        SetNewTrendEffectAndPriceTarget();
    }

    private void SetCeiling() {
        Ceiling = randomGenerator.NextRandomFloat(CEILING_MAX_INITIAL_VALUE / 2f, CEILING_MAX_INITIAL_VALUE);
    }

    private void GenerateInitialPrice() {
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
        VolumeHistory.Add(randomGenerator.NextRandomFloat(0f, VOLUME_MAX_INITIAL_VALUE));
    }

    private void GenerateNewTrend() {
        float newTrend;
        float ceilingDistance = Ceiling - CurrentPrice();

        if (TrendHistory.Count == 0) {
            TrendHistory.Add(randomGenerator.NextRandomFloat(-1f, 1f));
            return;
        }

        if (ceilingDistance <= CEILING_PROXIMITY_THRESHOLD) {
            newTrend = randomGenerator.NextRandomFloat(-1f, -0.75f); // price is too close to ceiling, force it down
            currentTargetApproachDelay = TARGET_APPROACH_DELAY_MAX;
        }
        else {
            float currentAbsoluteTrend = Math.Abs(CurrentTrend());
            if (currentAbsoluteTrend >= 0.75f || currentAbsoluteTrend <= 0.25f) {
                // trend is too strong or too weak, so new effect is random
                newTrend = randomGenerator.NextRandomFloat(-1f, 1f);
            }
            else {
                // trend is in mid-range (between 0.25 and 0.75), so new effect is similar
                newTrend = CurrentTrend() + randomGenerator.NextRandomFloat(0f, 0.2f);
            }
        }

        TrendHistory.Add(newTrend);
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
