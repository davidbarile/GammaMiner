using System;
using System.Collections.Generic;
using UnityEngine;
using static CrystalData;

[CreateAssetMenu(fileName = "CrystalTradePricesConfig", menuName = "Data/CrystalTradePricesConfig", order = 5)]
public class CrystalTradePricesConfig : ScriptableObject
{
    [Range(1, 8), SerializeField] private int[] minOffersPerWeekDay = new int[7] { 1, 1, 1, 1, 1, 1, 1 };
    [Range(1, 8), SerializeField] private int[] maxOffersPerWeekDay = new int[7] { 6, 6, 6, 6, 6, 6, 6 };
    [Range(0f, 1f), SerializeField] private float[] weekWeightFactor = new float[7] { .5f, .5f, .5f, .5f, .5f, .5f, .5f };
    [Space, SerializeField] private CrystalTradePriceData[] crystalTradePriceDatas;

    public List<CrystalTradePriceData> GenerateNewPrices()
    {
        var originalSeed = UnityEngine.Random.state;
        UnityEngine.Random.InitState(PlayerData.Data.PriceRandomSeed);

        var weekDayNum = PlayerData.Data.PriceDayOfWeek;

        var offersPerDay = WeightedRandom.GetWeightedRandomInt(
            minOffersPerWeekDay[weekDayNum],
            maxOffersPerWeekDay[weekDayNum],
            weekWeightFactor[weekDayNum]
        );

        var offersList = new List<CrystalTradePriceData>();

        foreach (var priceData in this.crystalTradePriceDatas)
        {
            // if (UnityEngine.Random.value > priceData.ChanceToAppearDayOfWeek[weekDayNum])
            //     continue;

            var percentModifier = WeightedRandom.GetWeightedRandomInt(
                priceData.MinPercentPerDayOfWeek[weekDayNum],
                priceData.MaxPercentPerDayOfWeek[weekDayNum],
                priceData.WeightFactor[weekDayNum]
            );

            var newPriceData = new CrystalTradePriceData
            {
                CrystalType = priceData.CrystalType,
                BasePrice = priceData.BasePrice,
                PercentModifier = percentModifier
            };

            offersList.Add(newPriceData);
        }

        while (offersList.Count > offersPerDay)
        {
            var removeIndex = WeightedRandom.GetWeightedRandomInt(0, offersList.Count - 1, 0.7f);//favor removing higher index items
            offersList.RemoveAt(removeIndex);
        }

        UnityEngine.Random.state = originalSeed;

        return offersList;
    }
}

[Serializable]
public class CrystalTradePriceData
{
    public ECrystalType CrystalType;
    [HideInInspector] public int PercentModifier;//calculated at runtime
    public int BasePrice;

    [Range(0f, 1f)] public float[] ChanceToAppearDayOfWeek = new float[7] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
    [Range(-100, 100)] public int[] MinPercentPerDayOfWeek = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
    [Range(-100, 100)] public int[] MaxPercentPerDayOfWeek = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
    [Range(0f, 1f)] public float[] WeightFactor = new float[7] { .5f, .5f, .5f, .5f, .5f, .5f, .5f };
}