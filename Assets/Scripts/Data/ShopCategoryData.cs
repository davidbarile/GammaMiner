using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ShopCategoryData", menuName = "Data/ShopCategoryData", order = 5)]
public class ShopCategoryData : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public ShopItemConfig[] ItemDatas;
}