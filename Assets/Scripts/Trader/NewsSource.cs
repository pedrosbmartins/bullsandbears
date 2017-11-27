using System;
using System.Collections;
using UnityEngine;

public class NewsSource : MonoBehaviour {

    private const float FIRST_NEWS_GAP = 10f;
    private const float NEWS_GAP_MIN = 20f;
    private const float NEWS_GAP_MAX = 25f;

    private StockMarket market;

    private void Awake() {
        market = GetComponent<StockMarket>();
        if (GameAchievements.IsMechanicUnlocked(Mechanic.News)) {
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
        yield return new WaitForSeconds(FIRST_NEWS_GAP);
        while (true) {
            CreateRandomNewsStory();
            yield return new WaitForSeconds(UnityEngine.Random.Range(NEWS_GAP_MIN, NEWS_GAP_MAX));
        }
    }

    private void CreateRandomNewsStory() {
        var randomIndex = Mathf.FloorToInt((market.StockList.Count - 1) * UnityEngine.Random.value);
        var industry = market.StockList[randomIndex].CompanyIndustry;
        var direction = UnityEngine.Random.value < 0.5f ? EffectDirection.Positive : EffectDirection.Negative;
        var strength = UnityEngine.Random.value < 0.5f ? EffectStrength.Strong: EffectStrength.Weak;

        market.SetNewPriceEffect(new PriceEffect(industry, direction, strength));

        var newsStrength = strength == EffectStrength.Strong ? "really " : "";
        var newsType = direction == EffectDirection.Positive ? "good" : "bad";
        var message = string.Format("Some {0}{1} news for {2}", newsStrength, newsType, industry);
        MessageCentral.Instance.DisplayMessage("News", message);
    }

}
