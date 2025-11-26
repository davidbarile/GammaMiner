using System;
using UnityEngine;

[Serializable]
public class CrystalData
{
    [Flags]
    public enum ECrystalType
    {
        //TODO: change to names like Torazine, Phazon, Kryaline, Xinubus, Galaxium, Antimatter
        //Xenonite, Plutron, Zetium, Quarkon, Nebulon, Kryonite, Xyron, Gamanite-0, Gamanite-3, Gamanite-5

        None = 0,
        Crystal1 = 1, //Green
        Crystal2 = 2, //Blue
        Crystal3 = 4, //Red
        Crystal4 = 8, //Purple
        Crystal5 = 16, //Black
        Crystal6 = 32, //White
        Crystal7 = 64, //Orange
        Crystal8 = 128 //Yellow
    }

    public string Name;
    public ECrystalType CrystalType;
    public Color CrystalColor = Color.white;
    [Range(0f, 1f)] public float RadioactivityLevel;
}