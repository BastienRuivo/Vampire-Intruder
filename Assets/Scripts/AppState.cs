using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AppState : Singleton<AppState>
{
    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }
    /////////////////////////
    /////// VARIABLES ///////
    /////////////////////////

    // App State variables
    private int princeMercy = 3;                        // Number of Prince Mercies left (player's life number)
    private int mainObjectiveSkip = 2;                  // Number of main objectives skip (can skip only 2 main objectives)
    private int levelNumber = 0;                        // The level number in the run
    private int runNumber = 0;                          // The run number

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

    //LevelGenerator parameters
    private int startingEnergy;

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

        Debug.Log("End of level " + levelNumber + " with guardKilledInCurrentScene: " + guardKilledInCurrentScene + " and totalGuardsInCurrentScene: " + totalGuardsInCurrentScene + " and secondaryObjectivesAchievedInCurrentScene: " + secondaryObjectivesAchievedInCurrentScene + " and totalSecondaryObjectivesInCurrentScene: " + totalSecondaryObjectivesInCurrentScene);
        // Prince Mercies
        if (timeFailed || guardFailed || bloodFailed)
        {
            decreasePrinceMercy();
        }
        else
        {
            if (mainObjectiveAchieved)
            {
                decreasePrinceMercy();
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
        secondaryObjectivesAchieved = secondaryObjectivesAchievedInCurrentScene; //+= ici non ?
        totalSecondaryObjectives = totalSecondaryObjectivesInCurrentScene;  //pareil

        // Reset the current scene parameters
        guardKilledInCurrentScene = 0;
        totalGuardsInCurrentScene = 0;
        secondaryObjectivesAchievedInCurrentScene = 0;
        totalSecondaryObjectivesInCurrentScene = 0;

        levelNumber++;
        if (levelNumber == 2)
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

}
