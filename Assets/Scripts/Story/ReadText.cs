using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Read and print a story dialog
/// </summary>
public class ReadText : Singleton<ReadText>
{
    /////////////////////////
    /////// VARIABLES ///////
    /////////////////////////

    // Public scene objects
    public GameObject[] charactersObjects;          // The different characters	in the scene
    public GameObject[] originalCharactersObjects;  // The different characters	in the scene
    public Sprite[] backgrounds;					// All the different backgrounds
    public Text textBoxPhraseObject;				// Phrase printed in the textbox
	public Text textBoxNameObject;					// Name printed in the textbox
	public GameObject textBoxObject;				// The textbox object
	public Sprite[] textBoxSprites;					// The different textbox sprites
	public GameObject textBoxArrowObject;			// The textbox arrow
	public GameObject[] logTextBoxesObjects;		// The different log possible textBoxes
	public GameObject logScrollObject;				// The GameObject containing all the logs to scroll
	public Image backgroundObject;					// The background Image object
	public Image blackObject;						// The black Image object (to make the fadings)s
	public GameObject narrationObject;				// The narration object
	public GameObject narrationPhraseObject;		// One phrase printed in the narration
	public GameObject narrationLinesObject;			// The different lines printed in the narration
	public GameObject[] framesObjects;              // The different frames effects we can put on the background (Black = 0, Sepia = 1);
	public GameObject choicePanelObject;			// The GameObject containing the different choices
	public GameObject choiceObject;					// The choice button object
    public GameObject saveQuestionObject;           // The saveQuestion to make appear
    public GameObject continueObject;               // The continue button to make appear
    public GameObject backObject;                   // The back button to make disappear

    // Private dialog objects
    private string dialogName;                      // The current dialogName
	private string[] lines;							// All the different printed phrases
	private int[][] characters;						// All the different printed characters in the dialog				
	private int[][] expressions;					// All the different printed character's expressions
	private bool[][] isSpeaking;					// All the different printed character's speaking state
	private string[] names;							// All the different printed names
	private char[] textBoxes;						// All the different printed textboxes
	private List<GameObject> logTextBoxes;			// All the different printed log textboxes
	private GameObject[] narrationPhrases;          // All the different printed narration phrases
	private GameObject[] choices;					// All the different printed choices
    private bool isGameScene;                       // Indicates if a game scene is about to be launched
	
	// Possible objects
	private SortedDictionary<string, int> possibleCharacters;	// All the possible characters
	private SortedDictionary<string, int> possibleExpressions;	// All the possible expressions
	private SortedDictionary<string, int> possibleBackgrounds;	// All the possible backgrounds
	private SortedDictionary<char, int> possibleTextBoxes;	// All the possible text boxes

    // Private code variables
    private int linesNumber = 0;					// The number of dialog lines
	private int currentLineNumber = -1;             // The current line number

	private float printingTime = 0.0f;				// The printing time for each line
	private float startTime = 0.0f;					// The line printing starting time		
	private float logHeight = 0.0f;					// The log screen height

	private bool hasDisappeared = false;			// Indicates if the character has finished disappearing
	private bool hasAppeared = false;               // Indicates if the character has finished appearing
	private bool fadingIn = false;					// Indicates if the black screen is fading in
	private bool fadingOut = false;                 // Indicates if the black screen is fading out
    private bool fadingProtect = false;				// Protect the fading animation from being stopped

    private bool isNarration = false;               // Indicated if a narration is printed
    private Coroutine narrationCoroutine = null;    // The narration coroutine
	
	// Choices parameters
	private bool isChoice = false;
	private string[] choicePhrase;
	private int[] JessikaLove;
	private int[] ElrikLove;
	private string[] nextDialog;
	private bool autoMode = false;

    // Save parameters
    private int currentBackground = 0;
    private int currentFrame = -1;
    private char currentTextBox = 'L';
    private bool[] currentCharacters;
    private int[] currentExpressions;
    private float[] currentPositions;

    // Skip parameters
    private int nextSceneNumber = 0;
    private int nextLevelNumber = 0;
    private string nextDialogFile = "null";

    /////////////////////
    /////// START ///////
    /////////////////////

    /// <summary>
	/// Starting the dialog
	/// </summary>
    void Start()
	{
        // All the possible objects
        possibleCharacters = new SortedDictionary<string, int>
        {
            {"Jessika", 0}, {"Elrik", 1}, {"Jasper", 2}, {"Ellena", 3}
        };
        possibleExpressions = new SortedDictionary<string, int>
        {
            {"Neutral", 0}, {"Smile", 1}, {"Laugh", 2}, {"Upset", 3}, {"Angry", 4},
            {"Surprise", 5}, {"Fear", 6}, {"Tears", 7}, {"Cry", 8}, {"Flush", 9}
        };
        possibleBackgrounds = new SortedDictionary<string, int>
        {
            {"Narration", 0}, {"Day", 1}, {"Evening", 2}, {"Night", 3},
            {"Alley1", 4}, {"Alley2", 5}, {"Antechamber", 6}, {"Corridor", 7},
            {"EllenaBedroom", 8}, {"ElrikBedroom", 9}, {"JessikaBedroom", 10}, {"LivingRoom", 11},
            {"Manor1", 12}, {"Manor2", 13}, {"Manor3", 14}, {"Manor4", 15},
            {"Manor5", 16}, {"Manor6", 17}, {"Manor7", 18}, {"Manor8", 19},
            {"Manor9", 20}, {"ManorHall1", 21}, {"ManorHall2", 22}, {"ManorHall3", 23},
            {"Office", 24}, {"Prison1", 25}, {"Prison2", 26}, {"Prison3", 27},
            {"ReceptionHall", 28}, {"SecretRoom", 29}, {"Versailles1", 30}, {"Versailles2", 31}
        };
        possibleTextBoxes = new SortedDictionary<char, int>
        {
            {'L', 0}, {'R', 1}, {'C', 2}, {'S', 3}, {'T', 4 }, {'J',  5}
        };

		// Make all the characters invisible
        for (int i = 0; i < charactersObjects.Length; i++)
		{
			charactersObjects[i].GetComponent<FadeInOut>().SetOpacity(0.0f);
		}

		logTextBoxes = new List<GameObject>();

        // Save parameters
        currentBackground = 0;
        currentFrame = -1;
        currentTextBox = 'L';
        currentCharacters = new bool[4];
        currentExpressions = new int[4];
        currentPositions = new float[4];
        for (int i = 0; i < 4; i++)
        {
            currentCharacters[i] = false;
            currentExpressions[i] = 0;
            currentPositions[i] = 0f;
        }

		chooseDialog();
	}

    /// <summary>
    /// Computes whoch dialog scene should be played thanks to the AppState
    /// </summary>
	private void chooseDialog()
	{
		dialogName = "0";

		AppState appState = GameObject.Find("AppState").GetComponent<AppState>();

        // Check if its from a save
        bool isFromSave = appState.getFromSave();
        if (isFromSave)
        {

            // Check if we must load the next game scene
            bool isScene = appState.getGameScene();
            if (isScene)
            {
                Debug.Log("Here I am");
                SceneManager.LoadSceneAsync("LevelGenTest");
                return;
            }

            // Elseway, we must load a dialog and a current line
            dialogName = appState.getDialogName();
            currentLineNumber = appState.getLineNumber();
            currentBackground = appState.getCurrentBackground();
            currentFrame = appState.getCurrentFrame();
            currentTextBox = appState.getCurrentTextBox();
            currentCharacters = appState.getCurrentCharacters();
            currentExpressions = appState.getCurrentExpressions();
            currentPositions = appState.getCurrentPositions();

            Initialize(dialogName, currentLineNumber - 1);

            return;
        }

        // Elseway, must choose the right dialog
		int runNumber = appState.getRunNumber();
		int levelNumber = appState.getLevelNumber();
		int princeMercy = appState.getPrinceMercy();
		int mainObjectiveSkip = appState.getMainObjectiveSkip();
		int currentJessikaLove = appState.getJessikaLove();
		int currentElrikLove = appState.getElrikLove();
		bool hasMainObjectiveFailed = appState.getMainObjectiveFailed();
		bool hasTimeFailed = appState.getTimeFailed();
		bool hasGuardFailed = appState.getGuardFailed();
		bool hasBloodFailed = appState.getBloodFailed();
		float guardKilledPercent = appState.getGuardsKilledPercent();
		float secondaryObjectivesPercent = appState.getSecondaryObjectivesAchievedPercent();
		bool hasAlreadyKilled = appState.getAlreadyKilled();
		bool hasAlreadySecondary = appState.getAlreadySecondary();

		// Run 0
		if (runNumber == 0)
		{
			// Before Level 0
			if (levelNumber == 0)
			{
                dialogName = "Run0/Start"; // -> Run 0 Level 0 Scene
            }

			// Before Level 1 (level 0 ended)
			else
			{
				// Fail
				if (hasMainObjectiveFailed)
				{
					dialogName = "Run0/Level0/FailObjective"; // -> Run0/GameOver -> GameOver Scene
				}
				else if (hasTimeFailed)
				{
                    dialogName = "Run0/Level0/FailTime"; // -> Run0/GameOver -> GameOver Scene
                }
				else if (hasGuardFailed)
				{
					dialogName = "Run0/Level0/FailGuard"; // -> Run0/GameOver -> GameOver Scene
                }
                else if (hasBloodFailed)
                {
                    dialogName = "Run0/Level0/FailBlood"; // -> Run0/GameOver -> GameOver Scene
                }

                // Success
                else
				{
					if (guardKilledPercent > 0.0f)
					{
						dialogName = "Run0/Level0/Killed"; // -> Run0/Level0/End -> Run0 Level1 Scene
                        appState.setAlreadyKilled();
                    }
					else
					{
						dialogName = "Run0/Level0/Success"; // -> Run0/Level0/End -> Run0 Level1 Scene
                    }
				}
			}
		}

		// End Run 0 + Start Run 1
		else if (runNumber == 1)
		{
			// End Run 0 Level 1
			if (levelNumber == 0)
			{
                // Fail
                if (hasMainObjectiveFailed)
                {
                    dialogName = "Run0/Level1/FailObjective"; // -> Run0/GameOver -> GameOver Scene
                }
                else if (hasTimeFailed)
                {
                    dialogName = "Run0/Level1/FailTime"; // -> Run0/GameOver -> GameOver Scene
                }
                else if (hasGuardFailed)
                {
                    dialogName = "Run0/Level1/FailGuard"; // -> Run0/GameOver -> GameOver Scene
                }
                else if (hasBloodFailed)
                {
                    dialogName = "Run0/Level1/FailBlood"; // -> Run0/GameOver -> GameOver Scene
                }

                // Success
                else
                {
                    if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                    {
                        dialogName = "Run0/Level1/FirstKilled"; // -> Run0/Level1/End -> Run1/Start -> Run 1 Level 0 Scene
                        appState.setAlreadyKilled();

                    }
                    else if (guardKilledPercent > 0.5f)
                    {
                        dialogName = "Run0/Level1/Killed"; // -> Run0/Level1/End -> Run1/Start -> Run 1 Level 0 Scene
                    }
                    else
                    {
                        dialogName = "Run0/Level1/Success"; // -> Run0/Level1/End -> Run1/Start -> Run 1 Level 0 Scene
                    }
                }
            }

			// End Run 1 Level 0
			else
			{
                // Fail
                if (hasMainObjectiveFailed)
                {
                    dialogName = "Run1/Level0/FailObjective"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                }
                else if (hasTimeFailed)
                {
                    dialogName = "Run1/Level0/FailTime"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                }
                else if (hasGuardFailed)
                {
                    dialogName = "Run1/Level0/FailGuard"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                }
                else if (hasBloodFailed)
                {
                    dialogName = "Run1/Level0/FailBlood"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                }

                // Success
                else
                {
					// First secondary objectives
					if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
					{
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run1/Level0/FirstKilledFirstObjectives"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run1/Level0/KilledFirstObjectives"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run1/Level0/StealthObjectives"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run1/Level0/SuccessFirstObjectives"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
                        }
						appState.setAlreadySecondary();
                    }
                    
					// No secondary objectives
                    else
                    {
						if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
						{
							dialogName = "Run1/Level0/FirstKilled"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
							appState.setAlreadyKilled();

						}
						else if (guardKilledPercent >= 0.5f)
						{
							dialogName = "Run1/Level0/Killed"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
						}
						else if (guardKilledPercent <= 0.1f)
						{
							dialogName = "Run1/Level0/Stealth"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
						}
						else
						{
							dialogName = "Run1/Level0/Success"; // -> Run1/Level0/End -> Run 1 Level 1 Scene
						}
                    }
                }
            }
		}

        // End Run 1 + Start Run 2
        else if (runNumber == 2)
        {
            // End Run 1 Level 1
            if (levelNumber == 0)
            {
				// Fail

				// Main objective
				if (hasMainObjectiveFailed)
				{
					if (mainObjectiveSkip == 2)
					{
						dialogName = "Run1/Level1/FailObjective2"; // -> GameOver Scene
					}
					else
					{
						dialogName = "Run1/Level1/FailObjective"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                    }
				}

				else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
				{

					// Second fail
					if (princeMercy == 1)
					{
						if (hasTimeFailed)
						{
							dialogName = "Run1/Level1/FailTime2"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
						else if (hasGuardFailed)
						{
							dialogName = "Run1/Level1/FailGuard2"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run1/Level1/FailBlood2"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                    }

					// First fail
					else
					{
						if (hasTimeFailed)
						{
							dialogName = "Run1/Level1/FailTime"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
						else if (hasGuardFailed)
						{
							dialogName = "Run1/Level1/FailGuard"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run1/Level1/FailBlood"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                    }
				}


				// Success
				else
				{
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run1/Level1/FirstKilledFirstObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run1/Level1/KilledFirstObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run1/Level1/StealthFirstObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run1/Level1/SuccessFirstObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        appState.setAlreadySecondary();
                    }

					// Good secondary objectives
					else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
					{
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run1/Level1/FirstKilledObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run1/Level1/KilledObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run1/Level1/StealthObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run1/Level1/SuccessObjectives"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run1/Level1/FirstKilled"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run1/Level1/Killed"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run1/Level1/Stealth"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run1/Level1/Success"; // -> Run1/Level1/End -> Run2/Start -> Run 2 Level 0 Scene
                        }
                    }
                }
            }

            // End Run 2 Level 0
            else
            {
                // Fail

                // Main objective
                if (hasMainObjectiveFailed)
                {
                    if (mainObjectiveSkip == 2)
                    {
                        dialogName = "Run2/Level0/FailObjective2"; // -> Run2/GameOver -> GameOver Scene
                    }
                    else
                    {
                        dialogName = "Run2/Level0/FailObjective"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                    }
                }

                else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
                {

                    // Third fail
                    if (princeMercy == 0)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level0/FailTime3"; // -> Run2/GameOver -> GameOver Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level0/FailGuard3"; // -> Run2/GameOver -> GameOver Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level0/FailBlood3"; // -> Run2/GameOver -> GameOver Scene
                        }
                    }

                    // Second fail
                    else if (princeMercy == 1)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level0/FailTime2"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level0/FailGuard2"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level0/FailBlood2"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                    }

                    // First fail
                    else
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level0/FailTime"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level0/FailGuard"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level0/FailBlood"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                    }
                }

                // Success
                else
                {
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level0/FirstKilledFirstObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level0/KilledFirstObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level0/StealthFirstObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level0/SuccessFirstObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        appState.setAlreadySecondary();
                    }

                    // Good secondary objectives
                    else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level0/FirstKilledObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level0/KilledObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level0/StealthObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level0/SuccessObjectives"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level0/FirstKilled"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level0/Killed"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level0/Stealth"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level0/Success"; // -> Run2/Level0/End -> Run 2 Level 1 Scene
                        }
                    }
                }
            }
        }

        // End Run 2 + Start Run 3
        else if (runNumber == 3)
        {
            // End Run 2 Level 1
            if (levelNumber == 0)
            {
                // Fail

                // Main objective
                if (hasMainObjectiveFailed)
                {
                    if (mainObjectiveSkip == 2)
                    {
                        dialogName = "Run2/Level1/FailObjective2"; // -> Run2/GameOver -> GameOver Scene
                    }
                    else
                    {
                        dialogName = "Run2/Level1/FailObjective"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                    }
                }

                else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
                {

                    // Third fail
                    if (princeMercy == 0)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level1/FailTime3"; // -> Run2/GameOver -> GameOver Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level1/FailGuard3"; // -> Run2/GameOver -> GameOver Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level1/FailBlood3"; // -> Run2/GameOver -> GameOver Scene
                        }
                    }

                    // Second fail
                    else if (princeMercy == 1)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level1/FailTime2"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level1/FailGuard2"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level1/FailBlood2"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                    }

                    // First fail
                    else
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run2/Level1/FailTime"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run2/Level1/FailGuard"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run2/Level1/FailBlood"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                    }
                }


                // Success
                else
                {
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level1/FirstKilledFirstObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level1/KilledFirstObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level1/StealthFirstObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level1/SuccessFirstObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        appState.setAlreadySecondary();
                    }

                    // Good secondary objectives
                    else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level1/FirstKilledObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level1/KilledObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level1/StealthObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level1/SuccessObjectives"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run2/Level1/FirstKilled"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run2/Level1/Killed"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run2/Level1/Stealth"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level1/Success"; // -> Run2/Level1/End -> Run3/Start -> Run 3 Level 0 Scene
                        }
                    }
                }
            }

            // End Run 3 Level 0
            else
            {
                // Fail

                // Main objective
                if (hasMainObjectiveFailed)
                {
                    if (mainObjectiveSkip == 2)
                    {
                        dialogName = "Run3/Level0/FailObjective2"; // -> Run3/GameOver -> GameOver Scene
                    }
                    else
                    {
                        dialogName = "Run3/Level0/FailObjective"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                    }
                }

                else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
                {

                    // Third fail
                    if (princeMercy == 0)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level0/FailTime3"; // -> Run3/GameOver -> GameOver Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level0/FailGuard3"; // -> Run3/GameOver -> GameOver Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level0/FailBlood3"; // -> Run3/GameOver -> GameOver Scene
                        }
                    }

                    // Second fail
                    else if (princeMercy == 1)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level0/FailTime2"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level0/FailGuard2"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level0/FailBlood2"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                    }

                    // First fail
                    else
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level0/FailTime"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level0/FailGuard"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level0/FailBlood"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                    }
                }

                // Success
                else
                {
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level0/FirstKilledFirstObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level0/KilledFirstObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level0/StealthFirstObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run3/Level0/SuccessFirstObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        appState.setAlreadySecondary();
                    }

                    // Good secondary objectives
                    else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level0/FirstKilledObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level0/KilledObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level0/StealthObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run2/Level0/SuccessObjectives"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level0/FirstKilled"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level0/Killed"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level0/Stealth"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run3/Level0/Success"; // -> Run3/Level0/End -> Run 3 Level 1 Scene
                        }
                    }
                }
            }
        }

        // End Run 3 + Start Run 4
        else if (runNumber == 4)
        {
            // End Run 3 Level 1
            if (levelNumber == 0)
            {
                string storyPath = "Bad/";
                if (currentJessikaLove < 50)
                {
                    storyPath = "Neutral/";
                }

                // Fail

                // Main objective
                if (hasMainObjectiveFailed)
                {
                    if (mainObjectiveSkip == 2)
                    {
                        dialogName = "Run3/Level1/" + storyPath + "FailObjective2"; // -> Run3/GameOver -> GameOver Scene
                    }
                    else
                    {
                        dialogName = "Run3/Level1/" + storyPath + "FailObjective"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                }

                else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
                {

                    // Third fail
                    if (princeMercy == 0)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailTime3"; // -> Run3/GameOver -> GameOver Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailGuard3"; // -> Run3/GameOver -> GameOver Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailBlood3"; // -> Run3/GameOver -> GameOver Scene
                        }
                    }

                    // Second fail
                    else if (princeMercy == 1)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailTime2"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailGuard2"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailBlood2"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                    }

                    // First fail
                    else
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailTime"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailGuard"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FailBlood"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                    }
                }


                // Success
                else
                {
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FirstKilledFirstObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "KilledFirstObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "StealthFirstObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run3/Level1/" + storyPath + "SuccessFirstObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        appState.setAlreadySecondary();
                    }

                    // Good secondary objectives
                    else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FirstKilledObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "KilledObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "StealthObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run3/Level1/" + storyPath + "SuccessObjectives"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "FirstKilled"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "Killed"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run3/Level1/" + storyPath + "Stealth"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                        else
                        {
                            dialogName = "Run3/Level1/" + storyPath + "Success"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        }
                    }
                }
            }

            // End Run 4 Level 0
            else
            {
                // Fail

                // Main objective
                if (hasMainObjectiveFailed)
                {
                    if (mainObjectiveSkip == 2)
                    {
                        dialogName = "Run4/Level0/FailObjective2"; // -> Run4/GameOver -> GameOver Scene
                    }
                    else
                    {
                        dialogName = "Run4/Level0/FailObjective"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                    }
                }

                else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
                {

                    // Third fail
                    if (princeMercy == 0)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run4/Level0/FailTime3"; // -> Run4/GameOver -> GameOver Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run4/Level0/FailGuard3"; // -> Run4/GameOver -> GameOver Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run4/Level0/FailBlood3"; // -> Run4/GameOver -> GameOver Scene
                        }
                    }

                    // Second fail
                    else if (princeMercy == 1)
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run4/Level0/FailTime2"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run4/Level0/FailGuard2"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run4/Level0/FailBlood2"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                    }

                    // First fail
                    else
                    {
                        if (hasTimeFailed)
                        {
                            dialogName = "Run4/Level0/FailTime"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (hasGuardFailed)
                        {
                            dialogName = "Run4/Level0/FailGuard"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (hasBloodFailed)
                        {
                            dialogName = "Run4/Level0/FailBlood"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                    }
                }

                // Success
                else
                {
                    // First secondary objectives
                    if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run4/Level0/FirstKilledFirstObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run4/Level0/KilledFirstObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run4/Level0/StealthFirstObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run4/Level0/SuccessFirstObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        appState.setAlreadySecondary();
                    }

                    // Good secondary objectives
                    else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run4/Level0/FirstKilledObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run4/Level0/KilledObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run4/Level0/StealthObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run4/Level0/SuccessObjectives"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                    }

                    // Neutral secondary objectives
                    else
                    {
                        if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                        {
                            dialogName = "Run4/Level0/FirstKilled"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                            appState.setAlreadyKilled();

                        }
                        else if (guardKilledPercent >= 0.5f)
                        {
                            dialogName = "Run4/Level0/Killed"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else if (guardKilledPercent <= 0.1f)
                        {
                            dialogName = "Run4/Level0/Stealth"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                        else
                        {
                            dialogName = "Run4/Level0/Success"; // -> Run4/Level0/End -> Run 4 Level 1 Scene
                        }
                    }
                }
            }
        }

        // End Run 4 + End Game
        else if (runNumber == 5)
        {
            string storyPath = "Neutral1/";
            if (currentJessikaLove > 50)
            {
                storyPath = "Bad/";
            }
            else if (currentElrikLove > 50)
            {
                if (guardKilledPercent > 0.5f)
                {
                    storyPath = "Good2/";
                }
                else if (guardKilledPercent < 0.1f)
                {
                    storyPath = "Good1/";
                }
                else
                {
                    storyPath = "Neutral2/";
                }
            }

            // Fail

            // Main objective
            if (hasMainObjectiveFailed)
            {
                if (mainObjectiveSkip == 2)
                {
                    dialogName = "Run4/Level1/" + storyPath + "FailObjective2"; // -> Run4/GameOver GameOver Scene
                }
                else
                {
                    dialogName = "Run4/Level1/" + storyPath + "FailObjective"; // -> Run3/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                }
            }

            else if (hasTimeFailed || hasGuardFailed || hasBloodFailed)
            {

                // Third fail
                if (princeMercy == 0)
                {
                    if (hasTimeFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailTime3"; // -> Run4/GameOver -> GameOver Scene
                    }
                    else if (hasGuardFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailGuard3"; // -> Run4/GameOver -> GameOver Scene
                    }
                    else if (hasBloodFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailBlood3"; // -> Run4/GameOver -> GameOver Scene
                    }
                }

                // Second fail
                else if (princeMercy == 1)
                {
                    if (hasTimeFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailTime2"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (hasGuardFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailGuard2"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (hasBloodFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailBlood2"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                }

                // First fail
                else
                {
                    if (hasTimeFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailTime"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (hasGuardFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailGuard"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (hasBloodFailed)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FailBlood"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                }
            }


            // Success
            else
            {
                // First secondary objectives
                if (secondaryObjectivesPercent > 0.0f && !hasAlreadySecondary)
                {
                    if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FirstKilledFirstObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        appState.setAlreadyKilled();

                    }
                    else if (guardKilledPercent >= 0.5f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "KilledFirstObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (guardKilledPercent <= 0.1f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "StealthFirstObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else
                    {
                        dialogName = "Run4/Level1/" + storyPath + "SuccessFirstObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    appState.setAlreadySecondary();
                }

                // Good secondary objectives
                else if (secondaryObjectivesPercent > 0.5f && hasAlreadySecondary)
                {
                    if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FirstKilledObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        appState.setAlreadyKilled();

                    }
                    else if (guardKilledPercent >= 0.5f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "KilledObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (guardKilledPercent <= 0.1f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "StealthObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else
                    {
                        dialogName = "Run4/Level1/" + storyPath + "SuccessObjectives"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                }

                // Neutral secondary objectives
                else
                {
                    if (guardKilledPercent > 0.0f && !hasAlreadyKilled)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "FirstKilled"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                        appState.setAlreadyKilled();

                    }
                    else if (guardKilledPercent >= 0.5f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "Killed"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else if (guardKilledPercent <= 0.1f)
                    {
                        dialogName = "Run4/Level1/" + storyPath + "Stealth"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                    else
                    {
                        dialogName = "Run4/Level1/" + storyPath + "Success"; // -> Run4/Level1/End -> Run4/Start -> Run 4 Level 0 Scene
                    }
                }
            }
        }

        Initialize(dialogName, -1);
        //Initialize("Trailer", -1);
	}

	/// <summary>
	/// Initialize the dialog (delete everything but the log)
	/// </summary>
	/// <param name="dialog">The new dialog to load</param>
	private void Initialize(string dialog, int lineNumber) {

		// Reset the different variables
		if (lines != null) Array.Clear(lines, 0, lines.Length);
        if (characters != null) Array.Clear(characters, 0, characters.Length);
        if (expressions != null) Array.Clear(expressions, 0, expressions.Length);
        if (isSpeaking != null) Array.Clear(isSpeaking, 0, isSpeaking.Length);
        if (names != null) Array.Clear(names, 0, names.Length);
        if (textBoxes != null) Array.Clear(textBoxes, 0, textBoxes.Length);
        if (narrationPhrases != null) Array.Clear(narrationPhrases, 0, narrationPhrases.Length);
        if (choices != null) Array.Clear(choices, 0, choices.Length);
		linesNumber = 0;
		currentLineNumber = lineNumber;
		printingTime = 0.0f;
		startTime = 0.0f;
		hasDisappeared = false;
		hasAppeared = false;
		fadingIn = false;
		fadingOut = false;
		fadingProtect = false;
		isNarration = false;
		narrationCoroutine = null;
		isChoice = false;
        isGameScene = false;
        nextSceneNumber = 0;
        nextLevelNumber = 0;
        nextDialogFile = "null";

        // Load the dialog
        LoadDialog(dialog);

        // Set the printing time for the first line
        printingTime = 2.0f + lines[0].Length / 50.0f;

        // From a saving
        if (currentLineNumber > -1)
        {
            // Compute the scene to print
            int numberOfCharacters = 0;
            for (int i = 0; i < 4; i++)
            {
                if (currentCharacters[i])
                {
                    numberOfCharacters++;
                }
            }

            int[] charactersToPrint = new int[numberOfCharacters];
            float[] positionsToPrint = new float[numberOfCharacters];
            int[] expressionsToPrint = new int[numberOfCharacters];
            numberOfCharacters = 0;
            for (int i = 0; i < 4; i++)
            {
                if (currentCharacters[i])
                {
                    charactersToPrint[numberOfCharacters] = i;
                    positionsToPrint[numberOfCharacters] = currentPositions[i];
                    expressionsToPrint[numberOfCharacters] += currentExpressions[i];
                    numberOfCharacters++;
                }
            }

            StartCoroutine(ChangeBackground(currentBackground, currentFrame, currentTextBox,
                charactersToPrint, positionsToPrint, expressionsToPrint));
        }
        else
        {
            NextLine();
        }
    }

    //////////////////////
    /////// UPDATE ///////
    //////////////////////

    /// <summary>
    /// Compute the mouse actions, update the fading animations
    /// </summary>
    void Update()
    {

		/////////////////////////////
		/////// MOUSE ACTIONS ///////
		/////////////////////////////

		// Goes to the next line (if no choice)
		if (!isChoice)
		{
			// Conditions: mouse click or auto time exceeded, no log screen, no ongoing fading animations
			if ((Input.GetMouseButtonDown(0) || (Time.time - startTime > printingTime
				&& GetComponent<StoryButtonManager>().getAuto()))
				&& !GetComponent<StoryButtonManager>().getLog()
				&& !GetComponent<StoryButtonManager>().getSave() && !fadingProtect)
			{
				// If a narration is ongoing, destroy all the narration lines
				if (isNarration)
				{
					for (int i = 0; i < narrationPhrases.Length; i++)
					{
						Destroy(narrationPhrases[i]);
					}
					Array.Clear(narrationPhrases, 0, narrationPhrases.Length);
				}

				// If no buttons are clicked, go to the next line
				if (EventSystem.current.currentSelectedGameObject == null)
				{
					NextLine();
				}
				// If a button is clicked, checked which button it is
				else
				{
					// Get the button name
					string name = EventSystem.current.currentSelectedGameObject.name;

					// If it's the auto button, don't go to the next line and wait a bit instead
					if (name == "Auto")
					{
						// If the text printing as ended, wait for 2 seconds
						if (!textBoxPhraseObject.GetComponent<LoadText>().GetLoadText())
						{
							startTime = Time.time - 2.0f;
						}
						// Else, start waiting from now
						else
						{
							startTime = Time.time;
						}

						// Deselect the auto button
						EventSystem.current.SetSelectedGameObject(null);
					}
					// If it's not another button, go to the next line
					else if (name != "Log" && name != "Skip" && name != "Close" && name != "Saves" && name != "Back")
					{
						NextLine();
					}
				}
			}
			// If a narration is ongoing, print the whole narration directly
			else if (Input.GetMouseButtonDown(0) && isNarration && fadingProtect)
			{
				// Stop the fading
				fadingProtect = false;
				StopCoroutine(narrationCoroutine);

				// Print all the lines at their final position
				for (int i = 0; i < narrationPhrases.Length; i++)
				{
					narrationPhrases[i].transform.GetChild(0).GetComponent<Animator>().SetTrigger("StopLine");
				}

				// For the auto mode, wait for 1 second
				printingTime = 1.0f;
				startTime = Time.time;
			}
		}

        //////////////////////////////////////
        /////// CHARACTERS FADE IN OUT ///////
        //////////////////////////////////////

        // Check if all the characters has disappeared of not
        hasDisappeared = true;
		for (int i = 0; i < charactersObjects.Length; i++)
		{
			if (charactersObjects[i].GetComponent<FadeInOut>().getOpacity() > 0.001) hasDisappeared = false;
		}

		// Check if all the characters has appeared or not
		hasAppeared = true;
		for (int i = 0; i < charactersObjects.Length; i++)
		{
			if (charactersObjects[i].GetComponent<FadeInOut>().GetAnim()) hasAppeared = false;
		}

        //////////////////////////////////////
        /////// BACKGROUND FADE IN OUT ///////
        //////////////////////////////////////

		// Black background fading in animation
        if (fadingIn)
		{
			// Make the fading in animation
			blackObject.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, blackObject.GetComponent<Image>().color.a + Time.deltaTime);
			
			// Check if the fading in is finished
			if (blackObject.GetComponent<Image>().color.a >= 1.0f)
			{
				blackObject.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
				fadingIn = false;
			}
		}

		// Black background fading out animation
		if (fadingOut)
		{
			// Make the fading out animation
			blackObject.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, blackObject.GetComponent<Image>().color.a - Time.deltaTime);

			// Check if the fading out is finished
			if(blackObject.GetComponent<Image>().color.a <= 0.0f)
			{
				blackObject.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
				fadingOut = false;
			}
		}

	}

    /////////////////////////
    /////// NEXT LINE ///////
    /////////////////////////

    /// <summary>
    /// Load and print the next dialog line, or end the current dialog line if not finished
    /// </summary>
    private void NextLine()
	{
		// If the current line is finished, load the next line
		if (!textBoxPhraseObject.GetComponent<LoadText>().GetLoadText())
		{
			// Goes to the next line number
			currentLineNumber++;

			// If the dialog is finished, launch the ending animation
			if (currentLineNumber >= linesNumber)
			{
				StartCoroutine(End());
				return;
			}

            ////////////////////////
            /////// COMMANDS ///////
            ////////////////////////

            // Check if the current line is a command line
            if (lines[currentLineNumber].Length > 3 && lines[currentLineNumber].Substring(0, 3) == "@@@")
			{
				// Get the command words
				string[] mots = lines[currentLineNumber].Split(' ');
				isNarration = false;

				/////////////////////////////
				/////// SCENE CHANGES ///////
				/////////////////////////////

                // Scene change
                if (mots[0] == "@@@Scene")
                {
                    int scene = int.Parse(mots[1]);
                    int level = int.Parse(mots[2]);

                    StartCoroutine(ChangeScene(scene, level));
                }

				// Character disappearing command
				else if (mots[0] == "@@@Disappear")
				{
					string characterName = mots[1];
					CharacterDisappear(possibleCharacters[characterName]);
					NextLine();
				}

				// Character appearing command
				else if (mots[0] == "@@@Appear")
				{
					string characterName = mots[1];
					string characterExpression = mots[2];
					CharacterAppear(possibleCharacters[characterName],
						float.Parse(mots[3]), possibleExpressions[characterExpression]);
					NextLine();
				}

				else if (mots[0] == "@@@Next")
				{
					string nextDialog = mots[1];
					Initialize(nextDialog, -1);
				}

				// Background change (with new characters or not)
				else if (mots[0] == "@@@Background")
				{
					// Get the new background
					string backgroundName = mots[1];

					// Get the next textbox sprite
					char textBoxID = mots[2][0];

					// Get the new characters parameters
					int charactersNumber = int.Parse(mots[3]);
					int[] charactersID = new int[charactersNumber];
					float[] positions = new float[charactersNumber];
					int[] expressionsID = new int[charactersNumber];
					for (int i = 0; i < charactersNumber; i++)
					{
						string characterName = mots[4 + i * 3];
						charactersID[i] = possibleCharacters[characterName];
						string characterExpression = mots[5 + i * 3];
						expressionsID[i] = possibleExpressions[characterExpression];
						positions[i] = float.Parse(mots[6 + i * 3]);
					}

					// Check if an effect frame is added
					if (lines[currentLineNumber + 1].Substring(0, 11) == "@@@SetFrame")
					{
						// If yes, add the frame and skip the next line
						currentLineNumber++;
						StartCoroutine(ChangeBackground(possibleBackgrounds[backgroundName],
							lines[currentLineNumber][12] - '0', textBoxID, charactersID, positions, expressionsID));
					}
					else
					{
						// If no, make the frame to -1 (no effect frame)
						StartCoroutine(ChangeBackground(possibleBackgrounds[backgroundName],
							-1, textBoxID, charactersID, positions, expressionsID));
					}
				}

				//////////////////////
				/////// CHOICE ///////
				//////////////////////

				else if (mots[0] == "@@@Choice")
				{
					// Save the auto mode
					autoMode = GetComponent<StoryButtonManager>().getAuto();
					GetComponent<StoryButtonManager>().setAuto(false);
					isChoice = true;

					// Make the Jessika's thinking
					string phrase = mots[1];
					phrase = phrase.Replace('_', ' ');
                    isNarration = false;
                    // Fill the phrase and name
                    textBoxPhraseObject.text = phrase;
                    textBoxPhraseObject.fontStyle = FontStyle.Italic;
                    textBoxNameObject.text = "Jessika";

					CreateLog(null, "Jessika", phrase, 4);

                    startTime = Time.time;
                    printingTime = (float)(2.0f + lines[currentLineNumber].Length / 50.0);

                    // Reset the text printing script
                    textBoxPhraseObject.GetComponent<LoadText>().Reset();

                    // Change the character position
                    string characterName = mots[2];
					string characterExpression = mots[3];
					CharacterDisappear(possibleCharacters[characterName]);
					CharacterAppear(possibleCharacters[characterName],
						1.5f, possibleExpressions[characterExpression]);

					// Choices parameters
					int choiceNumber = int.Parse(mots[4]);
					nextDialog = new string[choiceNumber];
					JessikaLove = new int[choiceNumber];
					ElrikLove = new int[choiceNumber];
					choicePhrase = new string[choiceNumber];
					for (int i = 0; i < choiceNumber; i++)
					{
						nextDialog[i] = mots[5 + i * 4];
						JessikaLove[i] = int.Parse(mots[6 + i * 4]);
						ElrikLove[i] = int.Parse(mots[7 + i * 4]);
						string choice = mots[8 + i * 4];
						choice = choice.Replace('_', ' ');
						choicePhrase[i] = choice;
					}

					float panelSize = choicePanelObject.GetComponent<RectTransform>().rect.size.y;
					float choiceHeight = panelSize / (choiceNumber + 1);
					choices = new GameObject[choiceNumber];

					// Printing the choices
					for (int i = 0; i < choiceNumber; i++)
					{
						choices[i] = Instantiate(choiceObject, choicePanelObject.transform);
						choices[i].name = "Choice" + i;
						choices[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
								choices[i].GetComponent<RectTransform>().anchoredPosition.x,
								-choiceHeight * (i + 1) + panelSize / 2.0f);
						choices[i].transform.GetChild(0).GetComponent<Text>().text = choicePhrase[i];
						choices[i].GetComponent<Button>().onClick.AddListener(
							() => makeChoice());
						choices[i].SetActive(true);
                    }

					choicePanelObject.SetActive(true);
				}

				/////////////////////////
				/////// NARRATION ///////
				/////////////////////////

				// Narration command
				else if (mots[0] == "@@@Narration")
				{
					// Active the narration mode
					isNarration = true;

					// Disable the textbox
					textBoxObject.SetActive(false);

					// Read the narration lines
					string[] narrationLines;
					// Cut in two lines
					string[] lines1 = this.lines[++currentLineNumber].Split('\n');
					// If more than one part, cut the subparts
					if (lines1.Length > 1)
					{
						string[] lines2 = lines1[1].Split(new string[] { "\\n" }, 12, StringSplitOptions.None);
						narrationLines = new string[lines2.Length + 1];
						// Get the first part and all the next parts
						for (int i = 0; i < narrationLines.Length; i++)
						{
							if (i == 0) narrationLines[i] = lines1[0];
							else narrationLines[i] = lines2[i - 1];
						}
					}
					// If only one part, get this part
					else
					{
						narrationLines = lines1;
					}

					// Log
					CreateLog(narrationLines, names[currentLineNumber], "", 6);

					// Launch the narration animation
					narrationCoroutine = StartCoroutine(SetNarration(narrationLines));
				}
			}
			// No command, normal dialog line
			else
			{
                ///////////////////////////
                /////// DIALOG LINE ///////
                ///////////////////////////

				// No narration
                isNarration = false;

                textBoxArrowObject.SetActive(true);

                // Fill the phrase and name
                textBoxPhraseObject.text = lines[currentLineNumber];
                textBoxPhraseObject.fontStyle = FontStyle.Normal;
                textBoxNameObject.text = names[currentLineNumber];

				// Reset the text printing script
				textBoxPhraseObject.GetComponent<LoadText>().Reset();

				// Change the character emotion
				for (int j = 0; j < characters[currentLineNumber].Length; j++)
				{
					int characterID = characters[currentLineNumber][j];
					int expressionID = expressions[currentLineNumber][j];
					bool isCharacterSpeaking = isSpeaking[currentLineNumber][j];
					charactersObjects[characterID].GetComponent<ChangeExpressions>().ChangeExpression(expressionID, isCharacterSpeaking);

                    // Save parameter
                    currentExpressions[characterID] = expressionID;
				}

				// Log
				CreateLog(null, names[currentLineNumber],
					lines[currentLineNumber], possibleTextBoxes[textBoxes[currentLineNumber]]);

                startTime = Time.time;
                printingTime = (float)(2.0f + lines[currentLineNumber].Length / 50.0);
            }
		}
		// If the current line is not finished, end the current line
		else
		{
			textBoxPhraseObject.GetComponent<LoadText>().EndPhrase();
		}
	}

    ///////////////////
    /////// LOG ///////
    ///////////////////

    /// <summary>
    /// Create the log text box
    /// </summary>
    /// <param name="narrationLines">When it's a narration, the different narration lines</param>
    /// <param name="name">The character speaking name</param>
    /// <param name="phrase">The spoken phrase</param>
    /// <param name="textBoxID">The text box sprite ID</param>
    private void CreateLog(string[] narrationLines, string name, string phrase, int textBoxID)
	{

		float height = 0.0f;

		// Narration
		if (narrationLines != null)
		{
            // Instantiate a narration-type (5) log
            logTextBoxes.Add(Instantiate(logTextBoxesObjects[textBoxID],
                new Vector3(logTextBoxesObjects[textBoxID].transform.position.x,
                            logTextBoxesObjects[textBoxID].transform.position.y,
                            logTextBoxesObjects[textBoxID].transform.position.z),
                    Quaternion.identity, logScrollObject.transform));
            logTextBoxes.Last().SetActive(true);
            logTextBoxes.Last().transform.Find("Name").GetComponent<Text>().text = name;
            logTextBoxes.Last().transform.Find("Phrase").GetComponent<Text>().text = "";

            // Add all the narration lines in the log
            for (int i = 0; i < narrationLines.Length - 1; i++)
            {
                logTextBoxes.Last().transform.Find("Phrase").GetComponent<Text>().text += narrationLines[i] + '\n';
            }
            logTextBoxes.Last().transform.Find("Phrase").GetComponent<Text>().text +=
                narrationLines[narrationLines.Length - 1];

            // The log length (in lines)
            int logLength = narrationLines.Length;
            // If there is a name, add one line for the name
            if (names[currentLineNumber] != null)
            {
                logLength++;
            }

            // Get the sizes
            RectTransform centerTransform = logTextBoxes.Last().transform.Find("Center").GetComponent<RectTransform>();
            float currentCenterSize = centerTransform.rect.size.y;

            // Change the log center part and the whole phrase size
            SetHeight(logTextBoxes.Last().transform.Find("Center").GetComponent<RectTransform>(),
                currentCenterSize * logLength);
            SetHeight(logTextBoxes.Last().transform.Find("Phrase").GetComponent<RectTransform>(),
                currentCenterSize * logLength);

            // Change the top, phrase (and optionnaly the name) positions
            logTextBoxes.Last().transform.Find("Top").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                logTextBoxes.Last().transform.Find("Top").GetComponent<RectTransform>().anchoredPosition.x,
                logTextBoxes.Last().transform.Find("Top").GetComponent<RectTransform>().anchoredPosition.y
                + currentCenterSize * (logLength - 1));
            logTextBoxes.Last().transform.Find("Phrase").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                logTextBoxes.Last().transform.Find("Phrase").GetComponent<RectTransform>().anchoredPosition.x,
                logTextBoxes.Last().transform.Find("Phrase").GetComponent<RectTransform>().anchoredPosition.y
                - currentCenterSize);
            if (names.Last() != null)
            {
                logTextBoxes.Last().transform.Find("Name").GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    logTextBoxes.Last().transform.Find("Name").GetComponent<RectTransform>().anchoredPosition.x,
                    logTextBoxes.Last().transform.Find("Name").GetComponent<RectTransform>().anchoredPosition.y
                    + currentCenterSize * (logLength - 1));
            }

            // Compute this new log height
            height = (currentCenterSize * logLength
                + logTextBoxes.Last().transform.Find("Top").GetComponent<RectTransform>().rect.size.y * 2.0f) * 1.1f;
        }

		// Normal dialog or choice
		else
		{
            // Log textbox and textbox sprite depending on the type
            textBoxObject.GetComponent<Image>().sprite = textBoxSprites[textBoxID];
            if (textBoxID == 4) textBoxPhraseObject.fontStyle = FontStyle.Italic;

            // Instantiate the log textbox
            logTextBoxes.Add(Instantiate(logTextBoxesObjects[textBoxID],
                        new Vector3(logTextBoxesObjects[textBoxID].transform.position.x,
                                    logTextBoxesObjects[textBoxID].transform.position.y,
                                    logTextBoxesObjects[textBoxID].transform.position.z),
                        Quaternion.identity, logScrollObject.transform));
            // Log textbox filling
            logTextBoxes.Last().SetActive(true);
            logTextBoxes.Last().transform.Find("Name").GetComponent<Text>().text = name;
            logTextBoxes.Last().transform.Find("Phrase").GetComponent<Text>().text = phrase;
            if (textBoxID == 4) logTextBoxes.Last().transform.Find("Phrase").GetComponent<Text>().fontStyle = FontStyle.Italic;
            // New log textbox height
            height = logTextBoxes.Last().GetComponent<RectTransform>().rect.height;

            // Save parameters
            currentTextBox = textBoxes[currentLineNumber];
        }

        // Make all the previous log textbox up
        for (int i = 0; i < logTextBoxes.Count() - 1; i++)
        {
            if (logTextBoxes[i] != null)
            {
                logTextBoxes[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(logTextBoxes[i].GetComponent<RectTransform>().anchoredPosition.x,
                    logTextBoxes[i].GetComponent<RectTransform>().anchoredPosition.y + height);
            }
        }
        logHeight += height;
    }

	/////////////////////////////////////
	/////// CHARACTERS ANIMATIONS ///////
	/////////////////////////////////////

	/// <summary>
	/// Make the character disappear
	/// </summary>
	/// <param name="character">The character ID</param>
	private void CharacterDisappear(int character)
	{
		// Fade In Out animation
		charactersObjects[character].GetComponent<FadeInOut>().LaunchFadeOut();

        // Save parameter
        currentCharacters[character] = false;
	}

	/// <summary>
	/// Make the character appear
	/// </summary>
	/// <param name="character"></param>
	/// <param name="position"></param>
	/// <param name="idExpression"></param>
	private void CharacterAppear(int character, float position, int idExpression)
	{
		// Change the character position
		charactersObjects[character].transform.position = originalCharactersObjects[character].transform.position
			+ new Vector3(
			position * GetComponent<RectTransform>().rect.size.y * 0.3f,
			0.0f, 0.0f);

		// Fade In Out animation
		charactersObjects[character].GetComponent<FadeInOut>().LaunchFadeIn();

        // Save parameter
        currentCharacters[character] = true;
        currentExpressions[character] = idExpression;
        currentPositions[character] = position;
	}

    ////////////////////////////////////
    /////// BACKGROUND ANIMATION ///////
    ////////////////////////////////////

    /// <summary>
    /// Change the background and add (or not) new characters
    /// </summary>
    /// <param name="backgroundID">The new background ID (0 = narration background)</param>
    /// <param name="frameID">The effect frame ID (-1 = no effect frame)</param>
    /// <param name="charactersID">The new characters ID</param>
    /// <param name="positions">The new characters positions</param>
    /// <param name="expressionsID">The new characters expressions</param>
    /// <returns>Animation coroutine</returns>
    IEnumerator ChangeBackground(int backgroundID, int frameID, char textBoxID, int[] charactersID, float[] positions, int[] expressionsID)
	{
		// Start the animation
		fadingProtect = true;

		// Make the background get black
		fadingIn = true;
		yield return new WaitUntil(() => blackObject.GetComponent<Image>().color.a == 1.0f);
		fadingIn = false;

		// Wait a bit
        yield return new WaitForSeconds(0.5f);

		// Change the background
		backgroundObject.sprite = backgrounds[backgroundID];

		// Check if it's a narration
		if (backgroundID == 0)
		{
			backgroundObject.GetComponent<Image>().color = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
			narrationObject.SetActive(true);
            textBoxObject.SetActive(false);
        }
		else
		{
			backgroundObject.GetComponent<Image>().color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			narrationObject.SetActive(false);
            textBoxObject.SetActive(true);
        }

		// Disable the Arrow
		textBoxArrowObject.SetActive(false);

        // Change the textbox to the next textbox sprite
        switch (textBoxID)
        {
            case 'L':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[0];
                break;
            case 'R':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[4];
                break;
            case 'C':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[2];
                break;
            case 'S':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[6];
                break;
            case 'G':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[1];
                break;
            case 'D':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[5];
                break;
            case 'M':
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[3];
                break;
            default:
                textBoxObject.GetComponent<Image>().sprite = textBoxSprites[0];
                break;
        }

        // Put the effect frame
        if (frameID != -1)
		{
			framesObjects[frameID].SetActive(true);
		}
		// If not frame, disable all the frames
		else
		{
			for (int i = 0; i < framesObjects.Length; i++)
			{
				framesObjects[i].SetActive(false);
			}
		}

        // Make all the characters disappear
        for (int i = 0; i < charactersObjects.Length; i++)
        {
            if (charactersObjects[i].GetComponent<FadeInOut>().getOpacity() > 0.999f) CharacterDisappear(i);
        }
        yield return new WaitUntil(() => hasDisappeared == true);

        // Make all the new characters appear
        for (int i = 0; i < charactersID.Length; i++)
        {
            CharacterAppear(charactersID[i], positions[i], expressionsID[i]);
        }
        yield return new WaitUntil(() => hasAppeared == true);

        // Delete and empty the textbox
        //textBoxObject.SetActive(false);
		textBoxPhraseObject.text = "";
		textBoxNameObject.text = "";

		// Make the black screen disappear
        fadingOut = true;
		yield return new WaitUntil(() => blackObject.GetComponent<Image>().color.a == 0.0f);
		fadingOut = false;

		// End the fading animation
		fadingProtect = false;

        // Save parameter
        currentBackground = backgroundID;
        currentFrame = frameID;

		// Load the next line
		NextLine();
    }

    ///////////////////////////////////
    /////// NARRATION ANIMATION ///////
    ///////////////////////////////////

    /// <summary>
    /// Compute the narration animation
    /// </summary>
    /// <param name="lines">The narration lines</param>
    /// <returns>The animation coroutine</returns>
    private IEnumerator SetNarration(string[] lines)
	{
		// Launch the animation
		fadingProtect = true;

		// Create the narration lines game objects for each line
		narrationPhrases = new GameObject[lines.Length];
		for (int i = 0; i < lines.Length; i++)
		{
			// Create the game object
			narrationPhrases[i] = Instantiate(narrationPhraseObject, narrationLinesObject.transform);

			// Fill the text
			narrationPhrases[i].transform.GetChild(0).GetComponent<Text>().text = lines[i];

			// Place the line from the center, depending on the even line numbers or not
			float lineSize = narrationPhrases[0].GetComponent<RectTransform>().rect.size.y;
            if (lines.Length % 2 == 0)
			{
				narrationPhrases[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
					0.0f, 
					(float)(-lineSize / 2.0f + (lines.Length / 2) * lineSize - i * lineSize));
			}
			else
			{
				narrationPhrases[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(
					0.0f, 
					(float)((lines.Length - 1) / 2 * lineSize - i * lineSize));
			}
			
			// Enable the line
			narrationPhrases[i].SetActive(true);
		}

		// Launch the animation for each line
		for (int i = 0; i < lines.Length; i++)
		{
			narrationPhrases[i].transform.GetChild(0).GetComponent<Animator>().SetTrigger("LigneMove");
			yield return new WaitForSeconds(1.0f);
		}

		// Wait for one second after
		printingTime = 1.0f;
		startTime = Time.time;

		// Stop the animation
		fadingProtect = false;
	}

    //////////////////////////////
    /////// DIALOG LOADING ///////
    //////////////////////////////

    /// <summary>
    /// Load the dialog file
    /// </summary>
    private void LoadDialog(string filename)
	{
        // The dialog path
        string path = "Assets/Resources/Story/" + filename + ".txt";

		// The current line
		string line;

        ///////////////////////////
        /////// DIALOG SIZE ///////
        ///////////////////////////

        // The number of commands and narrations commands
        int commandsNumber = 0;
		int narrationsNumber = 0;
		int choiceNumber = 0;

		// Read the file once to find the commands and narrations
		StreamReader sr = new StreamReader(path);
		using (sr)
		{
			do
			{
				line = sr.ReadLine();
				if (line != null)
				{
					if (line.Length > 2 && line.Substring(0, 3) == "@@@") commandsNumber++;
					if(line.Length > 11 && line.Substring(0, 12) == "@@@Narration") narrationsNumber++;
					if(line.Length > 8 && line.Substring(0, 9) == "@@@Choice") choiceNumber++;
				}
			}
			while (line != null);
		}
		sr.Close();

		// Count the number of dialogue lines
		linesNumber = (File.ReadLines(@path).Count() - commandsNumber - narrationsNumber - choiceNumber) / 2
			+ commandsNumber + narrationsNumber + choiceNumber;

		// The dialog lists parameters to fill
		lines = new string[linesNumber];
		characters = new int[linesNumber][];
		expressions = new int[linesNumber][];
		isSpeaking = new bool[linesNumber][];
		names = new string[linesNumber];
		textBoxes = new char[linesNumber];

        // i: the dialog lines numbers (the two firsts lines are parameters)
        int i = 0;

        //////////////////////////////
        /////// DIALOG READING ///////
        //////////////////////////////

        // Read the file line by line
        sr = new StreamReader(path);
		using (sr) {
			do
			{
				// Read the line
				line = sr.ReadLine();

                //////////////////////////
                /////// PARAMETERS ///////
                //////////////////////////

                // If the line is not null
                if (line != null)
				{
                    ////////////////////////
                    /////// COMMANDS ///////
                    ////////////////////////

                    if (line.Length > 2 && line.Substring(0, 3) == "@@@")
					{
						// Put the command line in lines
						lines[i] = line;
						// No expression, no name
						expressions[i] = null;
						names[i] = "";

						// Narration
						if (line.Length > 11 && line.Substring(0, 12) == "@@@Narration")
						{
							// Get the narration name
							names[i + 1] = "";
							string[] words = line.Split(' ');
							string name = words[1];
							name = name.Replace('_', ' ');
							names[i + 1] = name;
						}
                        // Next scene
                        if (line.Length > 7 && line.Substring(0, 8) == "@@@Scene")
                        {
                            string[] mots = line.Split(' ');
                            nextSceneNumber = int.Parse(mots[1]);
                            nextLevelNumber = int.Parse(mots[2]);
                            nextDialogFile = "null";
                        }
                        // Next dialog
                        if (line.Length > 6 && line.Substring(0, 7) == "@@@Next")
                        {
                            string[] mots = line.Split(' ');
                            nextDialogFile = mots[1];
                            nextSceneNumber = -10;
                            nextLevelNumber = -10;
                        }

						// Default text box
						textBoxes[i] = '0';

						// Next line
						i++;
					}
					else
					{
                        ////////////////////////////
                        /////// DIALOG LINES ///////
                        ////////////////////////////
							
                        string[] words = line.Split(' ');
						int charactersNumber;
							
						// If the first word is an integer, this is a character description
						if (int.TryParse(words[0], out charactersNumber))
						{
							// Box type
							textBoxes[i] = words[1][0];

							// Name
							string name = words[2];
							name = name.Replace('_', ' ');
							names[i] = name;

							// Characters ID and expression
							characters[i] = new int[charactersNumber];
							expressions[i] = new int[charactersNumber];
							isSpeaking[i] = new bool[charactersNumber];

							for (int j = 0; j < charactersNumber; j++)
							{
								// Character
                                string characterName = words[3 + j * 3];
								characters[i][j] = possibleCharacters[characterName];
								
								// Expression
								string characterExpression = words[4 + j * 3];
								expressions[i][j] = possibleExpressions[characterExpression];
                               
								// Speaking
								isSpeaking[i][j] = int.Parse(words[5 + j * 3]) == 1;
							}
                        }
						// If the first word is not an integer, this is a dialog line
						else
						{
							// Get the line return (if there is one)
							int place = line.IndexOf(@"\n");
							if (place != -1)
							{
								// If there is one, put the line return in the line
								string part1 = line.Substring(0, place);
								string part2 = line.Substring(place + 2, line.Length - place - 2);
								lines[i] = part1 + "\n" + part2;
							}
							else
							{
								lines[i] = line;
							}

							// Next line
							i++;
						}
					}
				}
			}
			while (line != null);
		}
		sr.Close();
	}

    /////////////////////////////
    /////// CHOICE MAKING ///////
    /////////////////////////////

    /// <summary>
    /// Apply the parameters of the selected choice.
    /// </summary>
    /// <param name="JessikaLove">The quantity of Jessika's love of this choice</param>
    /// <param name="ErikLove">The quantity of Erik's love of this choice</param>
    /// <param name="nextDialog">The next dialog after this choice</param>
    /// <param name="autoMode">If the auto mode was on or not</param>
    private void makeChoice()
	{
		// Get the choice parameters
		string buttonName = EventSystem.current.currentSelectedGameObject.name;
		int choiceNumber = buttonName[6] - '0';

		// TO-DO : Jessika & Elrik Love

		CreateLog(null, "Jessika", choicePhrase[choiceNumber], 4);

		for (int i = 0; i < choices.Length; i++)
		{
			Destroy(choices[i]);
		}

		// Reset the story state
		choicePanelObject.SetActive(false);
        isChoice = false;
        GetComponent<StoryButtonManager>().setAuto(autoMode);

		// Go to the next dialog
		Initialize(nextDialog[choiceNumber], -1);
	}

    ///////////////////////
    /////// UTILITY ///////
    ///////////////////////

    /// <summary>
    /// End the dialog by making the black background fade in
    /// </summary>
    /// <returns>A fading in Coroutine</returns>
    public IEnumerator End()
    {
        fadingProtect = true;

        fadingIn = true;
        yield return new WaitUntil(() => blackObject.GetComponent<Image>().color.a == 1.0f);
        fadingIn = false;

        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

	/// <summary>
	/// Set a given size to a RectTransform
	/// </summary>
	/// <param name="trans">The RectTransform to give the size to</param>
	/// <param name="newSize">The new size to give</param>
    private void SetSize(RectTransform trans, Vector2 newSize)
	{
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		trans.offsetMax = trans.offsetMax + 2.0f * new Vector2(
			deltaSize.x * (1f - trans.pivot.x),
			deltaSize.y * (1f - trans.pivot.y));
	}

    /// <summary>
    /// Set a given height to a RectTransform
    /// </summary>
    /// <param name="trans">The RectTransform to give the size to</param>
    /// <param name="height">The new height to give</param>
    private void SetHeight(RectTransform trans, float height)
	{
		SetSize(trans, new Vector2(trans.rect.size.x, height));
	}

	/// <summary>
	/// Get the current log height
	/// </summary>
	/// <returns>The current log height</returns>
	public float getLogHeight()
	{
		return logHeight;
	}

    /// <summary>
    /// Before charging a new game scene, ask for the saving
    /// </summary>
    public IEnumerator ChangeScene(int scene, int level)
    {
        fadingProtect = true;

        fadingIn = true;
        yield return new WaitUntil(() => blackObject.GetComponent<Image>().color.a == 1.0f);
        fadingIn = false;

        // New infiltration game level
        if (scene != -1)
        {
            isGameScene = true;
            GetComponent<StoryButtonManager>().OnSaveClicked();
            saveQuestionObject.SetActive(true);
            continueObject.SetActive(true);
        }
        else
        {
            switch (level)
            {
                case 0:
                    SceneManager.LoadSceneAsync("GameOver");
                    break;
                case 1:
                    SceneManager.LoadSceneAsync("BadEnd");
                    break;
                case 2:
                    SceneManager.LoadSceneAsync("NeutralEnd1");
                    break;
                case 3:
                    SceneManager.LoadSceneAsync("NeutralEnd2");
                    break;
                case 4:
                    SceneManager.LoadSceneAsync("GoodEnd1");
                    break;
                case 5:
                    SceneManager.LoadSceneAsync("GoodEnd2");
                    break;
            }

        }
    }

    /// <summary>
    /// Continue on to the next scene
    /// </summary>
    public void OnContinueClicked()
    {
        saveQuestionObject.SetActive(false);
        continueObject.SetActive(false);
        GetComponent<StoryButtonManager>().OnBackClicked();

        SceneManager.LoadSceneAsync("LevelGenTest");
    }

    /// <summary>
    /// Returns the current dialog name
    /// </summary>
    /// <returns>The current dialog name</returns>
    public string getDialogName()
    {
        return dialogName;
    }

    /// <summary>
    /// Returns the current line number
    /// </summary>
    /// <returns>The current line number</returns>
    public int getLineNumber()
    {
        return currentLineNumber;
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

    public int[] getNextScene()
    {
        int[] nextSceneAndLevel = new int[2];
        while(nextSceneNumber == -10)
        {
            LoadDialog(nextDialogFile);
        }
        nextSceneAndLevel[0] = nextSceneNumber;
        nextSceneAndLevel[1] = nextLevelNumber;
        return nextSceneAndLevel;
    }
}
