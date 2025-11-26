using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    public static Texture2D toTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;

        return tex;
    }

    public static void SetTextColor(this Toggle inToggle, Color inColor)
    {
        TextMeshProUGUI text = inToggle.GetComponentInChildren<TextMeshProUGUI>();

        if(text != null)
            text.color = inColor;
    }
    
    public static string ToHexString(this Color color)
    {
        return $"{(byte)(color.r * 255):X2}{(byte)(color.g * 255):X2}{(byte)(color.b * 255):X2}{(byte)(color.a * 255):X2}";
    }

    public static void RandomizeList<T>(this List<T> inList)
    {
        List<T> tempList = new List<T>();

        int rnd;

        while (inList.Count > 0)
        {
            rnd = UnityEngine.Random.Range(0, inList.Count);
            T obj = inList[rnd];
            tempList.Add(obj);
            inList.Remove(obj);
        }

        //inList = tempList;

        foreach (T obj in tempList)
            inList.Add(obj);
    }

    public static List<T> CreateRandomizedList<T>(this List<T> inList)
    {
        List<T> randomList = new List<T>();
        List<T> tempList = new List<T>();

        foreach (T obj in inList)
            tempList.Add(obj);

        int rnd;

        while (tempList.Count > 0)
        {
            rnd = UnityEngine.Random.Range(0, tempList.Count);
            T obj = tempList[rnd];
            randomList.Add(obj);
            tempList.Remove(obj);
        }

        return randomList;
    }

    public static List<T> CopyList<T>(this List<T> inList)
    {
        List<T> newList = new List<T>();

        foreach (T obj in inList)
            newList.Add(obj);

        return newList;
    }

    public static List<T> ConvertToList<T>(this T[] inArray)
    {
        List<T> newList = new List<T>();

        foreach (T obj in inArray)
            newList.Add(obj);

        return newList;
    }

    public static T[] ConvertToArray<T>(this List<T> inList)
    {
        T[] newArray = new T[inList.Count];

        for(int i = 0; i < inList.Count; ++i)
        {
            newArray[i] = inList[i];
        }

        return newArray;
    }

    public static Dictionary<TKey, TValue> CompressDictionary<TKey, TValue>(this Dictionary<TKey, TValue> inDict)
    {
        Dictionary<TKey, TValue> compressedDict = new Dictionary<TKey, TValue>();

        foreach (KeyValuePair<TKey, TValue> pair in inDict)
            compressedDict.Add(pair.Key, pair.Value);

        return compressedDict;
    }

    public static List<TValue> CreateList<TKey, TValue>(this Dictionary<TKey, TValue> inDict)
    {
        List<TValue> list = new List<TValue>();

        foreach (KeyValuePair<TKey, TValue> pair in inDict)
            list.Add(pair.Value);

        return list;
    }

    public static string RemoveSpacesFromBeginningAndEnd(this string inString)
    {
        while (inString.Substring(0, 1) == " ")
            inString = inString.Remove(0, 1);

        while (inString.Length > 1 && inString.Substring(inString.Length - 1, 1) == " ")
            inString = inString.Remove(inString.Length - 1, 1);

        return inString;
    }

    public static BoundsInt ToBoundsInt(this Bounds inBounds)
    {
        return new BoundsInt(Mathf.RoundToInt(inBounds.min.x), Mathf.RoundToInt(inBounds.min.y), Mathf.RoundToInt(inBounds.min.z), Mathf.RoundToInt(inBounds.size.x), Mathf.RoundToInt(inBounds.size.y), Mathf.RoundToInt(inBounds.size.z));
    }

    public static string ShowObjectPath(this Transform inTransform)
    {
        string pathString = inTransform.gameObject.name;

        Transform t = inTransform;

        while( t.parent != null)
        {
            pathString = t.parent.gameObject.name + "/" + pathString;
            t = t.parent;
        }

        return pathString;
    }

    public static EventTrigger AddTriggersEvents(this Selectable theSelectable, EventTriggerType eventTriggerType, Action<BaseEventData> onTriggerAction = null)
    {
        EventTrigger eventrTrigger = theSelectable.gameObject.AddComponent<EventTrigger>();
        if (onTriggerAction != null)
        {
            EventTrigger.Entry pointerEvent = new EventTrigger.Entry();
            pointerEvent.eventID = eventTriggerType;
            pointerEvent.callback.AddListener((x) => onTriggerAction(x));
            eventrTrigger.triggers.Add(pointerEvent);
        }
        return eventrTrigger;
    }

    
}
