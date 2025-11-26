using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetColorsConfig", menuName = "Data/Planets/PlanetColorsConfig", order = 4)]
public class PlanetColorsConfig : ScriptableObject
{
    [Header("Planet Colors")]
    public Color BaseColor1 = Color.white;
    public Color BaseColor2 = Color.white;
    public Color OutlineColor1 = Color.black;
    public Color OutlineColor2 = Color.black;
    public List<Color> RingColors = new();
    public List<Color> CloudOrStripeColors = new();
    public List<Color> DustColors = new();
}