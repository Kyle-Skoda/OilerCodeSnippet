using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField]
    private DeckSO deck;
    [SerializeField]
    private StatTrackerObject statTracker;

    public static SaveManager Instance;

    //Singleton to make saving easier on specific events
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
            Destroy(this);

        //Load stats and cards when the menu is loaded
        LoadGame();
    }

    public void SaveGame()
    {
        //Get the card names in the players deck
        List<string> cardNamesLocal = new List<string>();
        for (int i = 0; i < deck.MaxCardsInDeck; i++)
        {
            if (deck.cardsInDeck.Count > i)
                cardNamesLocal.Add(deck.cardsInDeck[i].title);
            else
                cardNamesLocal.Add("Null");
        }

        List<StatTrackerSaveObject> statContent = new List<StatTrackerSaveObject>();
        //If the player is opening for the first time add all stats to save.json with the value 0, prevents null refrences.
        if (statTracker.statTrackingList.Count <= 0)
        {
            //Get the last enum in TrackableStats and add 1 to iterate through all possible stats
            for (int i = 0; i < (int)TrackableStats.Winrate + 1; i++)
            {
                StatTrackerSaveObject temp = new StatTrackerSaveObject();
                temp.statType = i;
                temp.statValue = 0;
                statContent.Add(temp);
            }
        }
        //Get the players current stats from games played
        else
        {
            for (int i = 0; i < (int)TrackableStats.Winrate + 1; i++)
            {
                StatTrackerSaveObject temp = new StatTrackerSaveObject();
                temp.statType = i;
                temp.statValue = statTracker.statTrackingList[i].value;
                statContent.Add(temp);
            }
        }

        string[] content = cardNamesLocal.ToArray();

        //Save the players deck and stats into the JSON file
        SaveObject saveOjbect = new SaveObject
        {
            deckSave = new SaveCardObject{ cardNames = content },
            statSave = statContent.ToArray()
        };

        string json = JsonUtility.ToJson(saveOjbect);

        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/save.json"))
        {
            //Ensure player deck is empty and all stats are at 0 to prevent overlapping when loading
            deck.cardsInDeck.Clear();
            statTracker.ResetStats();
            //Get the JSON file to load
            string saveString = File.ReadAllText(Application.persistentDataPath + "/save.json");
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            //Load the players stats
            for (int i = 0; i < saveObject.statSave.Length; i++)
                statTracker.Load(saveObject.statSave[i]);

            //Load the players cards into their deck
            for (int i = 0; i < saveObject.deckSave.cardNames.Length; i++)
                for (int x = 0; x < deck.allCards.Count; x++)
                    if (saveObject.deckSave.cardNames[i] == deck.allCards[x].title)
                        deck.AddCard(deck.allCards[x]);
        }
        else
        {
            //Create an empty save if no save.json file exists
            for (int i = 0; i < (int)TrackableStats.Winrate + 1; i++)
            {
                StatTrackerSaveObject tempSave = new StatTrackerSaveObject();
                tempSave.statType = i;
                tempSave.statValue = 0;
                statTracker.Load(tempSave);
            }
            SaveGame();
        }
    }
}

//Base class which is being saved
public class SaveObject
{
    public SaveCardObject deckSave;
    public StatTrackerSaveObject[] statSave;
}

[Serializable]
public class SaveCardObject
{
    //Array defaults to 30 as 30 is max deck size, empty slots are set to "Null"
    public string[] cardNames = new string[30];
}

[Serializable]
public class StatTrackerSaveObject
{
    public int statType;
    public float statValue;
}
