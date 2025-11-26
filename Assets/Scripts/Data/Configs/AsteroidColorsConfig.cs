using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidColorsConfig", menuName = "Data/Planets/AsteroidColorsConfig", order = 5)]
public class AsteroidColorsConfig : ScriptableObject
{
    [Header("Asteroid Colors")]
    public List<Color> Outline1Colors = new();
    public List<Color> Outline2Colors = new();
    public List<Color> Fill1Colors = new();
    public List<Color> Fill2Colors = new();
}