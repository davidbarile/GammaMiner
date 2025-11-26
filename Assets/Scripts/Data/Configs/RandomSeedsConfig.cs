using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomSeedsConfig", menuName = "Data/Planets/RandomSeedsConfig", order = 8)]
public class RandomSeedsConfig : ScriptableObject
{
    public List<string> SeedNames;
}