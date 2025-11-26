using UnityEngine;

[CreateAssetMenu(fileName = "MoonColorsConfig", menuName = "Data/Planets/MoonColorsConfig", order = 5)]
public class MoonColorsConfig : ScriptableObject
{
    [Header("Moon Colors")]
    public Color OutlineColor1 = Color.black;
    public Color FillColor1 = Color.white;

    [Header("Modify these manually to give a random range of colors for the moon.")]
    public Color OutlineColor2 = Color.black;
    public Color FillColor2 = Color.white;
}