using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Data/ShopConfig", order = 4)]
public class ShopConfig : ScriptableObject
{
    public string Name;
    public ShopCategoryData[] categoryDatas;
}