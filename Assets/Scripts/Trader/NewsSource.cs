using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsSource : MonoBehaviour {

    [SerializeField] private bool forceMechanic = false;

    [SerializeField] private float firstNewsDelay = 10f;
    [SerializeField] private float newsGapMin = 20f;
    [SerializeField] private float newsGapMax = 25f;

    private StockMarket market;

    private void Awake() {
        market = GetComponent<StockMarket>();
        if (forceMechanic || GameAchievements.IsMechanicUnlocked(Mechanic.News)) {
            market.OnDayStarted += Initialize;
        }
    }

    public void Initialize() {
        StartCoroutine(GenerateNews());
    }

    public void Stop() {
        StopAllCoroutines();
    }

    private IEnumerator GenerateNews() {
        yield return new WaitForSeconds(firstNewsDelay);
        while (true) {
            CreateRandomNewsStory();
            yield return new WaitForSeconds(UnityEngine.Random.Range(newsGapMin, newsGapMax));
        }
    }

    private void CreateRandomNewsStory() {
        var randomIndex = Mathf.FloorToInt((newsList.Count - 1) * UnityEngine.Random.value);
        var news = newsList[randomIndex];

        var industry = news.Industry;
        var direction = news.PriceEffectDirection;
        var strength = news.PriceEffectStrength;
        market.SetPriceEffect(new PriceEffect(industry, direction, strength));

        MessageCentral.Instance.DisplayMessage("News", news.Headline);
    }

    private List<News> newsList = new List<News>() {
        new News(Industry.Technology,      "Everybody Mad at Tech Giants (Again)", EffectDirection.Negative, EffectStrength.Strong),
        new News(Industry.Technology,      "Investors Delighted with Tech Giants", EffectDirection.Positive, EffectStrength.Strong),
        new News(Industry.BanksAndFinance, "Credit Fraud Shakes Finance World",    EffectDirection.Negative, EffectStrength.Strong),
        new News(Industry.BanksAndFinance, "Banks: Best Quarter In Recent Times",  EffectDirection.Positive, EffectStrength.Strong),
        new News(Industry.OilAndGas,       "Environment Disaster, Oil To Blame",   EffectDirection.Negative, EffectStrength.Strong),
        new News(Industry.OilAndGas,       "Oil Hits New Record High Prices",      EffectDirection.Positive, EffectStrength.Strong),
        new News(Industry.Automotive,      "Nobody Wants To Own Cars Anymore",     EffectDirection.Negative, EffectStrength.Strong),
        new News(Industry.Automotive,      "Investors Bet in Automotive Industry", EffectDirection.Positive, EffectStrength.Strong),
    };

}

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
