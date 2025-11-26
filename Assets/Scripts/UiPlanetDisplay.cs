using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UiPlanetDisplay : UiExpandableDisplay
{
    [SerializeField] private Planet planet;
    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI countText;
    [Space, SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;

    [Space, SerializeField] private GameObject addToFavoritesButton;
    [SerializeField] private GameObject removeFromFavoritesButton;

    [SerializeField] private List<string> planetSeedNames = new();

    private int currentPlanetIndex = 0;

    protected override void Start()
    {
        base.Start();

        var gm = FindFirstObjectByType<GameManager>();

        if (gm == null)
        {
            Application.targetFrameRate = 60;
            Init();//when in Planet Scene, GameManager is not present
        }
    }

    public void Init()
    {
        for (int i = 0; i < PlayerData.Data.FavoritePlanetSeeds.Count; ++i)
        {
            this.planetSeedNames.Add(PlayerData.Data.FavoritePlanetSeeds[i]);
        }

        if (this.planetSeedNames.Count == 0)
        {
            var rndString = PlanetManager.GetRandomSeedString();
            this.planet.SetRandomSeed(rndString);

            //this.planet.GenerateRandomPlanet();
            this.planetSeedNames.Add(this.planet.PlanetData.RandomSeedString);
            HandleLeftArrowClick();
        }
        else
        {
            this.currentPlanetIndex = this.planetSeedNames.Count - 1;
        }

        DisplayPlanetAtIndex(this.currentPlanetIndex);
    }

    private void DisplayPlanetAtIndex(int index)
    {
        var planetSeed = this.planetSeedNames[index];
        this.planet.ConfigureFromSeedName(planetSeed);

        this.planetNameText.text = $"{planetSeed}";
        this.countText.text = $"{index + 1}/{this.planetSeedNames.Count}";

        this.addToFavoritesButton.SetActive(!PlayerData.Data.FavoritePlanetSeeds.Contains(planetSeed));
        this.removeFromFavoritesButton.SetActive(PlayerData.Data.FavoritePlanetSeeds.Contains(planetSeed));
    }

    public void HandleLeftArrowClick()
    {
        if (this.currentPlanetIndex > 0)
        {
            --this.currentPlanetIndex;
            DisplayPlanetAtIndex(this.currentPlanetIndex);
        }

        this.leftArrow.interactable = this.currentPlanetIndex > 0;
    }

    public void HandleRightArrowClick()
    {
        if (this.currentPlanetIndex >= this.planetSeedNames.Count - 1)
        {
            this.planet.GenerateRandomPlanet();
            this.planetSeedNames.Add(this.planet.PlanetData.RandomSeedString);
        }

        ++this.currentPlanetIndex;
        this.leftArrow.interactable = true;
        DisplayPlanetAtIndex(this.currentPlanetIndex);
    }

    public void HandleAddToFavoritesButtonClick()
    {
        PlayerData.Data.AddFavoritePlanetSeed(this.planet.PlanetData.RandomSeedString);
        DisplayPlanetAtIndex(this.currentPlanetIndex);
    }

    public void HandleRemoveFromFavoritesButtonClick()
    {
        PlayerData.Data.RemoveFavoritePlanetSeed(this.planet.PlanetData.RandomSeedString);

        DisplayPlanetAtIndex(this.currentPlanetIndex);
    }
}