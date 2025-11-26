using System;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static int NOW
    {
        get { return (int)(DateTime.UtcNow - Utils.EpochStart).TotalSeconds; }
    }

    public static DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const int SECONDS_PER_HOUR = 3600;

    public static int SelectWeightedRandomInt(int[] inRandomWeightsArray)
    {
        int totalWeight = 0;

        List<int> allIndices = new List<int>();

        //Debug.Log("GetWeightedRandomInt() ------------------------");

        for (int i = 0; i < inRandomWeightsArray.Length; ++i)
        {
            //Debug.Log("inRandomWeightsArray["+i+"] = " + inRandomWeightsArray[i]);
            int weightAmount = inRandomWeightsArray[i];

            if (weightAmount > 0)
            {
                for (int j = 0; j < weightAmount; ++j)
                {
                    allIndices.Add(i);
                }

                totalWeight += weightAmount;
            }

            /*
            allIndices.Add(weightAmount);
            totalWeight += weightAmount;
            */
        }

        if (allIndices.Count == 0)
        {
            //Debug.Log("GetWeightedRandomInt()  count = 0");
            return -1;
        }

        int selectedIndex = totalWeight <= 0 ? 0 : UnityEngine.Random.Range(0, totalWeight);

        return allIndices[selectedIndex];
    }

    public static int GetWeightedRandomInt(List<int> inRandomWeightsList)
    {
        List<int> allIndices = new List<int>();

        //inRandomWeightsList = 5,5,0,0,0,0,0

        //Debug.Log("GetWeightedRandomInt() ------------------------");

        foreach (int index in inRandomWeightsList)
        {
            allIndices.Add(index);
            //Debug.Log("index = " + index);
        }

        if (allIndices.Count == 0)
        {
            Debug.Log("GetWeightedRandomInt()  count = 0");
            return -1;
        }

        int selectedIndex = UnityEngine.Random.Range(0, allIndices.Count);

        return allIndices[selectedIndex];
    }

    public static Vector2Int ReduceProperFraction(int inNumerator, int inDenominator)
    {
        float numerator = inNumerator;
        float denom = inDenominator;

        int divisor = 2;

        while (divisor <= numerator && divisor <= denom / 2)
        {
            while (true)
            {
                if (numerator % divisor < Mathf.Epsilon && denom % divisor < Mathf.Epsilon)
                {
                    numerator /= divisor;
                    denom /= divisor;
                }
                else
                    break;
            }

            ++divisor;

            while (!Utils.CalcIsPrime(divisor))
            {
                ++divisor;
            }
        }

        return new Vector2Int(Mathf.RoundToInt(numerator), Mathf.RoundToInt(denom));
    }

    public static bool CalcIsPrime(int inNumber)
    {
        if (inNumber == 1) return false;
        if (inNumber == 2) return true;

        if (inNumber % 2 == 0) return false; // Even number     

        for (int i = 2; i < Mathf.Ceil((float)inNumber / 2); i++)
        {
            // Advance from two to include correct calculation for '4'
            if (inNumber % i == 0) return false;
        }

        return true;
    }

    public static string GetFormattedTime(float inTime)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(inTime);
        return string.Format("{0:D1}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string GetFormattedTimerString(int inTimeInt, bool inShouldBreakLineIfOverHour = false)
    {
        int seconds = Mathf.FloorToInt((float)inTimeInt % 60);

        int minutes = Mathf.FloorToInt((inTimeInt - ((float)inTimeInt % 60)) / 60);

        int hours = Mathf.FloorToInt((inTimeInt - ((float)inTimeInt % 3600)) / 3600);

        minutes -= (hours * 60);

        string timerString = string.Format("{0:D1}:{1:D2}", minutes, seconds);

        if (hours > 0)
        {
            if(inShouldBreakLineIfOverHour)
                timerString = string.Format("{0:D1}:{1:D2}\n:{2:D2}", hours, minutes, seconds);
            else
                timerString = string.Format("{0:D1}:{1:D2}:{2:D2}", hours, minutes, seconds);
        }

        return timerString;
    }
}
