using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Stock {

    public delegate void StockProcessedHandler(Stock stock);
    public event StockProcessedHandler OnProcessed;

    private const int CEILING_MAX_INITIAL_VALUE = 150;
    private const int VOLUME_MAX_INITIAL_VALUE = 100;
    private const int TARGET_APPROACH_DELAY = 10;

    public readonly string Symbol;
    public readonly string CompanyName;
    public readonly Industry CompanyIndustry;

    public float Ceiling { get; private set; }
    
    public List<float> PriceHistory { get; private set; }
    public List<float> VolumeHistory { get; private set; }
    public List<float> TrendHistory { get; private set; }

    private float PriceTarget;
    private float PreMovementPrice;

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
        Ceiling = randomGenerator.NextRandomFloat(CEILING_MAX_INITIAL_VALUE / 2f, CEILING_MAX_INITIAL_VALUE);
        PriceHistory.Add(Ceiling - randomGenerator.NextRandomFloat(0f, Ceiling + 1));
        SetNewPriceTarget();
    }

    public float CurrentPrice() {
        return PriceHistory.Last();
    }

    public float CurrentPriceChange() {
        if (PriceHistory.Count <= 1) {
            return 0f;
        }
        else {
            return PriceHistory.Last() - PriceHistory[PriceHistory.Count - 2];
        }
    }

    public float CurrentVolume() {
        return VolumeHistory.Last();
    }

    public float CurrentTrend() {
        return TrendHistory.Last();
    }

    private void GenerateNewVolume() {
        VolumeHistory.Add(randomGenerator.NextRandomFloat(0f, VOLUME_MAX_INITIAL_VALUE));
    }

    private void GenerateNewTrend() {
        TrendHistory.Add(randomGenerator.NextRandomFloat(-1f, 1f));
    }

    private void SetNewPriceTarget() {
        PreMovementPrice = CurrentPrice();
        GenerateNewVolume();
        GenerateNewTrend();
        float target = CurrentPrice() + (CurrentVolume() * CurrentTrend());
        PriceTarget = (target <= 0f) ? 1f : (target >= Ceiling) ? Ceiling - 1f : target;
    }

    public void Process() {
        float priceTargetApproach = (PriceTarget - PreMovementPrice) * (1f / TARGET_APPROACH_DELAY);
        PriceHistory.Add(CurrentPrice() + priceTargetApproach);
        if (Math.Abs(CurrentPrice() - PriceTarget) <= 0.10f) {
            SetNewPriceTarget();
        }
        if (OnProcessed != null) {
            OnProcessed(this);
        }
    }

}
