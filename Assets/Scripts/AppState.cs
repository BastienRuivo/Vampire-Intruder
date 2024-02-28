using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AppState : Singleton<AppState>
{

    /////////////////////////
    /////// VARIABLES ///////
    /////////////////////////

    // App State variables
    private int princeMercy = 3;                        // Number of Prince Mercies left (player's life number)
    private int mainObjectiveSkip = 2;                  // Number of main objectives skip (can skip only 2 main objectives)
    private int levelNumber = 0;                       // The level number in the run
    private int runNumber = 0;                          // The run number
    private int gameSeed;

    // The abilities and their acquisition
    private SortedDictionary<string, bool> abilities = new SortedDictionary<string, bool>
    {
        {"Teleportation", false }, {"Decieving", false}, {"Cataract", false}, {"Invisibility", false}
    };
    // The items and their acquire numbers
    private SortedDictionary<string, int> items = new SortedDictionary<string, int>
    {
        {"Sedative", 0}, {"BloodPouch", 0 }
    };

    // Story parameters
    private int JessikaLove = 0;
    private int ElrikLove = 0;

    private bool hasMainObjectiveFailed = false;
    private bool hasTimeFailed = false;
    private bool hasGuardFailed = false;
    private bool hasBloodFailed = false;

    private int guardKilled = 0;
    private int totalGuards = 0;
    private int secondaryObjectivesAchieved = 0;
    private int totalSecondaryObjectives = 0;

    private bool hasAlreadyKilled = false;
    private bool hasAlreadySecondary = false;

    //CurrentScene parameters
    private int guardKilledInCurrentScene = 0; 
    private int totalGuardsInCurrentScene = 0; 
    private int secondaryObjectivesAchievedInCurrentScene = 0;  
    private int totalSecondaryObjectivesInCurrentScene = 0; 

    // Scene parameters
    private string currentDialogName = null;
    private int currentLineNumber = 0;
    private bool isFromSave = false;
    private bool isGameScene = false;
    private int currentBackground;
    private int currentFrame;
    private char currentTextBox;
    private bool[] currentCharacters;
    private int[] currentExpressions;
    private float[] currentPositions;

    [System.Serializable]
    public class Level
    {
        public string mainRef;
        public string mainPhrase;
        public float startingEnergy;
        public int nbObjectives;
        public string seed;
        public Level(float startingEnergy, int nbObjectives, string mainRef, string mainPhrase, string seed = null)
        {
            this.startingEnergy = startingEnergy;
            this.mainRef = mainRef;
            this.mainPhrase = mainPhrase;
            this.nbObjectives = nbObjectives;
            if(string.IsNullOrEmpty(seed))
            {
                int r = UnityEngine.Random.Range(0, 6942666);
                this.seed = r.ToString();
            }
            else
            {
                this.seed = seed;
            }
        }

        public override string ToString()
        {
            return $"Level : [energy : { startingEnergy }, nbObjectives : {nbObjectives}, mainRef : {mainRef}, mainPhrase : {mainPhrase}, seed : {seed}]";
        }
    }

    public Level[] levelData =
    {
        // RUN 0
        new Level(0.1f, 0, "_DisplaySword", "Crochetez la vitrine contenant l'épée marquée", "Tartanpion"),
        new Level(0.39f, 0, "_StoleScroll", "Volez la lettre du Cardinal de Rohan au Roi", "Tagazok"),
        // RUN 1
        new Level(0.41f, 1, "_StealPriestNote", "Lisez le journal intime du Cardinal de Rohan", "Et"),
        new Level(0.59f, 2, "_ChestToOpen", "Volez l'argent du coffre du Comte de la Motte", "Puis"),
        // RUN 2
        new Level(0.61f, 3, "_StoleScroll", "Volez une lettre portant la signature de la Reine", "Baston"),
        new Level(0.79f, 4, "_SuckBloodBed", "Assassinez l'amant de la Baronne d'Oliva", "ceci est une seed"),
        // Run 3
        new Level(0.81f, 4, "_Vampire_Hunt_Bible", "Lisez les notes de la Comtesse de la Motte", "Rabibocher"),
        new Level(0.99f, 5, "_SuckBloodBed", "Assassinez Louis Marc Antoine Rétaux de Villette", "Technoblade"),
        // Run 4
        new Level(1.01f, 5, "_ChestToOpen", "Volez les diamants restants", "J3551K4"),
        new Level(1.19f, 6, "_SuckBloodBed", "Assassinez Boehmer", "MordMoiLNoeud"),

    };
    private static int lvlPerRun = 2;

    //////////////////////////
    /////// INITIALIZE ///////
    //////////////////////////

    override public void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        // First initialization
        if (currentCharacters == null)
        {
            currentCharacters = new bool[4];
            currentExpressions = new int[4];
            currentPositions = new float[4];
        }

        gameSeed = UnityEngine.Random.Range(0, 9999999);
        UnityEngine.Random.InitState(gameSeed);

        foreach(Level lvl in levelData)
        {
            lvl.seed = UnityEngine.Random.Range(0, 9999999).ToString();
        }
    }

    public void Initialize()
    {
        // App State
        princeMercy = 3;
        mainObjectiveSkip = 2;
        levelNumber = 0;
        runNumber = 0;

        // Abilities
        abilities["Teleportation"] = false;
        abilities["Decieving"] = false;
        abilities["Cataract"] = false;
        abilities["Invisibility"] = false;
        items["Sedative"] = 0;
        items["BloodPouch"] = 0;

        // Story
        JessikaLove = 0;
        ElrikLove = 0;
        hasMainObjectiveFailed = false;
        hasTimeFailed = false;
        hasGuardFailed = false;
        guardKilled = 0;
        secondaryObjectivesAchieved = 0;
        totalGuards = 0;
        totalSecondaryObjectives = 0;
        hasAlreadyKilled = false;
        hasAlreadySecondary = false;

        // Scene parameters
        currentDialogName = null;
        currentLineNumber = 0;
        isFromSave = false;
        isGameScene = false;
}

    /////////////////////////////////
    /////// LEVEL MANAGEMENTS ///////
    /////////////////////////////////

    // New run
    /// <summary>
    /// Launch a new run, reset the level and give a new ability
    /// </summary>
    private void NewRun()
    {
        runNumber++;
        levelNumber = 0;

        // Add new abilities at the end of each run
        switch(runNumber)
        {
            case 1:
                abilities["Teleportation"] = true; break;
            case 2:
                abilities["Decieving"] = true; break;
            case 3:
                abilities["Cataract"] = true; break;
            case 4:
                abilities["Invisibility"] = true; break;
        }
    }

    // End of infiltration level
    /// <summary>
    /// End a level, getting the new state and giving new items
    /// </summary>
    /// <param name="mainObjectiveAchieved">If the main objective was complete</param>
    /// <param name="timeFailed">If the player lost because of the time</param>
    /// <param name="guardFailed">If the player lost because of a guard</param>
    /// <param name="bloodFailed">If the player lost because of a blood</param>
    public void endLevel(bool mainObjectiveAchieved, bool timeFailed, bool guardFailed, bool bloodFailed)
    {
        // Scene management
        isGameScene = false;
        isFromSave = false;

        // Main objective
        hasMainObjectiveFailed = !mainObjectiveAchieved;

        // Prince Mercies
        if (timeFailed || guardFailed || bloodFailed)
        {
            hasMainObjectiveFailed = false;
            decreasePrinceMercy();
        }
        else
        {
            if (!mainObjectiveAchieved)
            {
                decreaseMainObjectiveSkip();
            }
        }
        hasTimeFailed = timeFailed;
        hasGuardFailed = guardFailed;
        hasBloodFailed = bloodFailed;
        
        // Guards kills
        guardKilled += guardKilledInCurrentScene;
        totalGuards += totalGuardsInCurrentScene;

        // Secondary achievements and items
        for (int i = 0; i < secondaryObjectivesAchievedInCurrentScene; i++)
        {
            System.Random rand = new System.Random();
            int randomItem = rand.Next(0, 2);
            if (randomItem == 0)
            {
                items["Sedative"]++;
            }
            else
            {
                items["BloodPouch"]++;
            }
        }
        secondaryObjectivesAchieved = secondaryObjectivesAchievedInCurrentScene;
        totalSecondaryObjectives = totalSecondaryObjectivesInCurrentScene;

        // Reset the current scene parameters
        guardKilledInCurrentScene = 0;
        totalGuardsInCurrentScene = 0;
        secondaryObjectivesAchievedInCurrentScene = 0;
        totalSecondaryObjectivesInCurrentScene = 0;

        levelNumber++;
        if (levelNumber == lvlPerRun)
        {
            NewRun();
        }
    }

    ///////////////////////////
    /////// GAME STATUS ///////
    ///////////////////////////

    // Prince Mercy
    public int getPrinceMercy()
    {
        return princeMercy;
    }
    private void decreasePrinceMercy()
    {
        princeMercy--;
        if (princeMercy < 0 )
        {
            princeMercy = 0;
        }
    }

    // Main Objective
    public int getMainObjectiveSkip()
    {
        return mainObjectiveSkip;
    }

    private void decreaseMainObjectiveSkip()
    {
        mainObjectiveSkip--;
        if (mainObjectiveSkip < 0 )
        {
            mainObjectiveSkip = 0;
        }
    }

    // Level
    public int getLevelNumber()
    {
        return levelNumber;
    }

    // Run
    public int getRunNumber()
    {
        return runNumber;
    }

    // guardKilledInCurrentScene
    public int getGuardKilledInCurrentScene()
    {
        return guardKilledInCurrentScene;
    }

    public void addGuardKilledInCurrentScene()
    {
        guardKilledInCurrentScene++;
    }

    // totalGuardsInCurrentScene
    public int getTotalGuardsInCurrentScene()
    {
        return totalGuardsInCurrentScene;
    }

    public void setTotalGuardsInCurrentScene(int total)
    {
        totalGuardsInCurrentScene = total;
    }

    // secondaryObjectivesAchievedInCurrentScene
    public int getSecondaryObjectivesAchievedInCurrentScene()
    {
        return secondaryObjectivesAchievedInCurrentScene;
    }
    
    public void setSecondaryObjectivesAchievedInCurrentScene(int total)
    {
        secondaryObjectivesAchievedInCurrentScene = total;
    }

    // totalSecondaryObjectivesInCurrentScene
    public int getTotalSecondaryObjectivesInCurrentScene()
    {
        return totalSecondaryObjectivesInCurrentScene;
    }

    public void setTotalSecondaryObjectivesInCurrentScene(int total)
    {
        totalSecondaryObjectivesInCurrentScene = total;
    }

    ///////////////////////////////////
    /////// ABILITIES AND ITEMS ///////
    ///////////////////////////////////

    public SortedDictionary<string, bool> getAbilities()
    {
        return abilities;
    }

    public SortedDictionary<string, int> getItems()
    {
        return items;
    }

    ////////////////////////////////
    /////// STORY PARAMETERS ///////
    ////////////////////////////////
    public int getJessikaLove()
    {
        return JessikaLove;
    }

    public void changeJessikaLove(int love)
    {
        JessikaLove += love;
    }

    public int getElrikLove()
    {
        return ElrikLove;
    }

    public void changeElrikLove(int love)
    {
        ElrikLove += love;
    }

    public bool getMainObjectiveFailed()
    {
        return hasMainObjectiveFailed;
    }

    public bool getTimeFailed()
    {
        return hasTimeFailed;
    }
    
    public bool getGuardFailed()
    {
        return hasGuardFailed;
    }

    public bool getBloodFailed()
    {
        return hasBloodFailed;
    }

    public float getGuardsKilledPercent()
    {
        return guardKilled / (float) totalGuards;
    }

    public float getSecondaryObjectivesAchievedPercent()
    {
        return secondaryObjectivesAchieved / (float) totalSecondaryObjectives;
    }

    public bool getAlreadyKilled()
    {
        return hasAlreadyKilled;
    }

    public void setAlreadyKilled ()
    {
        hasAlreadyKilled = true;
    }

    public bool getAlreadySecondary()
    {
        return hasAlreadySecondary;
    }

    public void setAlreadySecondary()
    {
        hasAlreadySecondary = true;
    }

    /////////////////////
    /////// SAVES ///////
    /////////////////////
    public void Save(int saveNumber, string dialogName, int lineNumber, bool gameScene,
        int currentBackground, int currentFrame, char currentTextBox,
        bool[] currentCharacters, int[] currentExpressions, float[] currentPositions)
    {

        // Dialog scene stats
        PlayerPrefs.SetString("dialogName" + saveNumber, dialogName);
        PlayerPrefs.SetInt("lineNumber" + saveNumber, lineNumber);
        PlayerPrefs.SetInt("gameScene" + saveNumber, gameScene ? 1 : 0);
        PlayerPrefs.SetInt("background" + saveNumber, currentBackground);
        PlayerPrefs.SetInt("frame" + saveNumber, currentFrame);
        PlayerPrefs.SetString("textBox" + saveNumber, currentTextBox.ToString());
        for (int i = 0; i < 4; i++)
        {
            PlayerPrefs.SetInt("character" + i + saveNumber, currentCharacters[i] ? 1 : 0);
            PlayerPrefs.SetInt("expression" + i + saveNumber, currentExpressions[i]);
            PlayerPrefs.SetFloat("position" + i + saveNumber, currentPositions[i]);
        }
             
        // App State variables
        PlayerPrefs.SetInt("princeMercy" + saveNumber, princeMercy);
        PlayerPrefs.SetInt("mainObjectiveSkip" + saveNumber, mainObjectiveSkip);
        PlayerPrefs.SetInt("levelNumber" + saveNumber, levelNumber);
        PlayerPrefs.SetInt("runNumber" + saveNumber, runNumber);

        // Items
        PlayerPrefs.SetInt("sedatives" + saveNumber, items["Sedative"]);
        PlayerPrefs.SetInt("bloodPouch" + saveNumber, items["BloodPouch"]);

        // Story parameters
        PlayerPrefs.SetInt("JessikaLove" + saveNumber, JessikaLove);
        PlayerPrefs.SetInt("ElrikLove" + saveNumber, ElrikLove);
        PlayerPrefs.SetInt("guardKilled" + saveNumber, guardKilled);
        PlayerPrefs.SetInt("totalGuards" + saveNumber, totalGuards);
        PlayerPrefs.SetInt("totalSecondaryObjectives" + saveNumber, totalSecondaryObjectives);
        PlayerPrefs.SetInt("hasAlreadyKilled" + saveNumber, hasAlreadyKilled ? 1 : 0);
        PlayerPrefs.SetInt("hasAlreadySecondary" + saveNumber, hasAlreadySecondary ? 1 : 0);

        // Date and time
        DateTime dt = DateTime.Now;
        PlayerPrefs.SetInt("year" + saveNumber, dt.Year);
        PlayerPrefs.SetInt("month" + saveNumber, dt.Month);
        PlayerPrefs.SetInt("day" + saveNumber, dt.Day);
        PlayerPrefs.SetInt("hour" + saveNumber, dt.Hour);
        PlayerPrefs.SetInt("minute" + saveNumber, dt.Minute);
        PlayerPrefs.SetInt("second" + saveNumber, dt.Second);

        
    }

    public void Load(int saveNumber)
    {
        if (PlayerPrefs.HasKey("dialogName" + saveNumber))
        {
            // Dialog scene stats
            currentDialogName = PlayerPrefs.GetString("dialogName" + saveNumber);
            currentLineNumber = PlayerPrefs.GetInt("lineNumber" + saveNumber);
            isGameScene = PlayerPrefs.GetInt("gameScene" + saveNumber) == 1;
            currentBackground = PlayerPrefs.GetInt("background" + saveNumber);
            currentFrame = PlayerPrefs.GetInt("frame" + saveNumber);
            currentTextBox = PlayerPrefs.GetString("textBox" + saveNumber)[0];
            for (int i = 0; i < 4; i++)
            {
                currentCharacters[i] = PlayerPrefs.GetInt("character" + i + saveNumber) == 1;
                currentExpressions[i] = PlayerPrefs.GetInt("expression" + i + saveNumber);
                currentPositions[i] = PlayerPrefs.GetFloat("position" + i + saveNumber);
            }

            // App State variables
            princeMercy = PlayerPrefs.GetInt("princeMercy" + saveNumber);
            mainObjectiveSkip = PlayerPrefs.GetInt("mainObjectiveSkip" + saveNumber);
            levelNumber = PlayerPrefs.GetInt("levelNumber" + saveNumber);
            runNumber = PlayerPrefs.GetInt("runNumber" + saveNumber);

            // Abilities
            switch (runNumber)
            {
                case 1:
                    abilities["Teleportation"] = true; break;
                case 2:
                    abilities["Teleportation"] = true;
                    abilities["Decieving"] = true; break;
                case 3:
                    abilities["Teleportation"] = true;
                    abilities["Decieving"] = true;
                    abilities["Cataract"] = true; break;
                case 4:
                    abilities["Teleportation"] = true;
                    abilities["Decieving"] = true;
                    abilities["Cataract"] = true;
                    abilities["Invisibility"] = true; break;
            }

            // Items
            items["Sedative"] = PlayerPrefs.GetInt("sedatives" + saveNumber);
            items["BloodPouch"] = PlayerPrefs.GetInt("bloodPouch" + saveNumber);

            // Story parameters
            JessikaLove = PlayerPrefs.GetInt("JessikaLove" + saveNumber);
            ElrikLove = PlayerPrefs.GetInt("ElrikLove" + saveNumber);
            guardKilled = PlayerPrefs.GetInt("guardKilled" + saveNumber);
            totalGuards = PlayerPrefs.GetInt("totalGuards" + saveNumber);
            totalSecondaryObjectives = PlayerPrefs.GetInt("totalSecondaryObjectives" + saveNumber);
            hasAlreadyKilled = PlayerPrefs.GetInt("hasAlreadyKilled" + saveNumber) == 1;
            hasAlreadySecondary = PlayerPrefs.GetInt("hasAlreadySecondary") == 1;

            // Scene parameters
            isFromSave = true;
        }
        else
        {
            Initialize();
        }
    }
    
    public string getDialogName()
    {
        return currentDialogName;
    }

    public int getLineNumber()
    {
        return currentLineNumber;
    }

    public bool getFromSave()
    {
        return isFromSave;
    }

    public bool getGameScene()
    {
        return isGameScene;
    }

    public int getCurrentBackground()
    {
        return currentBackground;
    }

    public int getCurrentFrame()
    {
        return currentFrame;
    }

    public char getCurrentTextBox()
    {
        return currentTextBox;
    }

    public bool[] getCurrentCharacters()
    {
        return currentCharacters;
    }

    public int[] getCurrentExpressions()
    {
        return currentExpressions;
    }

    public float[] getCurrentPositions()
    {
        return currentPositions;
    }

    public Level GetLevelData()
    {
        return levelData[runNumber * lvlPerRun + levelNumber];
    }
}
