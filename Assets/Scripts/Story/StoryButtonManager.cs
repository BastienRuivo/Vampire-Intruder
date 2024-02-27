using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Manage the story mode buttons
/// </summary>
public class StoryButtonManager : MonoBehaviour
{
	// Auto button
	public Button auto;				// The button to play auto or not
	public Sprite spriteAutoOn;		// The sprite for the auto button on
	public Sprite spriteAutoOff;	// The sprite for the auto button off
	private bool isAuto = false;	// Indicates if the auto mode is on (for the ReadText script)

	// Log button
	public GameObject log;			// The button to open the log screen
	public GameObject logScroll;	// The object containing all the logs
	private bool isLog = false;     // Indicates if the log is printed or not (for the ReadText script)

	// Save button
	public GameObject savesMenu;		// The saves menu to open or close
	private bool isSave = false;		// Indicates if the saves menu is printed or not (for the ReadText script)
	public GameObject confirmMenu;		// The confirmation menu to open or close
	public GameObject[] savesConfirms;	// The confirmation menu for the saves
	public GameObject[] loadConfirms;	// The confirmation menu for the loads

    // Tap effect
    public Image[] tap;				// The tap effect images

    /// <summary>
	/// Manage the tap effect when the mouse click
	/// </summary>
    void Update()
    {
		
		// Tap effect when the mouse click
        if(Input.GetMouseButtonDown(0))
		{
            for (int i = 0; i < tap.Length ; i++)
			{
				// Get the mouse position
				tap[i].GetComponent<Transform>().position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
				// Launch the animation
				tap[i].GetComponent<Animator>().SetTrigger("tap");
			}
		}
		
    }

	/// <summary>
	/// Trigger on or off the auto button
	/// </summary>
	public void OnAutoClicked()
	{
		// If auto is off, put the auto on
		if(!isAuto)
		{
			auto.GetComponent<Image>().sprite = spriteAutoOn;
			isAuto = true;
		}
		// If the auto is on, put the auto off
		else
		{
			auto.GetComponent<Image>().sprite = spriteAutoOff;
			isAuto = false;
		}
    }

	/// <summary>
	/// Trigger on or off the auto button
	/// </summary>
	/// <param name="auto">If the auto mode must be on or off</param>
	public void setAuto(bool makeAuto)
	{
		if (makeAuto)
		{
            auto.GetComponent<Image>().sprite = spriteAutoOn;
            isAuto = true;
        }
		else
		{
            auto.GetComponent<Image>().sprite = spriteAutoOff;
            isAuto = false;
        }
	}

	/// <summary>
	/// Trigger the skip button
	/// </summary>
	public void OnSkipClicked()
	{
        // Goes to the dialog end
        int[] nextSceneAndLevel = GetComponent<ReadText>().getNextScene();
        StopAllCoroutines();
        StartCoroutine(GetComponent<ReadText>().ChangeScene(nextSceneAndLevel[0], nextSceneAndLevel[1]));
	}

	/// <summary>
	/// Active the log pannel
	/// </summary>
	public void OnLogClicked()
	{
		// Active the log pannel
        log.SetActive(true);
		isLog = true;
	}

	/// <summary>
	/// Disable the log pannel
	/// </summary>
	public void OnCloseClicked()
	{
		// Reset the scrolling
        logScroll.GetComponent<ScrollLog>().Reset();
		// Disable the log pannel
		log.SetActive(false);
		isLog = false;
	}

	/// <summary>
	/// Open the saves menu
	/// </summary>
	public void OnSaveClicked()
	{
		savesMenu.GetComponent<LoadSaves>().ReloadInformations();
		savesMenu.SetActive(true);
		isSave = true;
	}

	/// <summary>
	/// Close the saves menu
	/// </summary>
	public void OnBackClicked()
	{
        savesMenu.SetActive(false);
		isSave = false;
    }

	/// <summary>
	/// Save on the first save
	/// </summary>
	public void OnSave1Clicked()
	{
		savesMenu.SetActive(false);
		confirmMenu.SetActive(true);
		savesConfirms[0].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

	/// <summary>
	/// Save on the second save
	/// </summary>
	public void OnSave2Clicked()
	{
        savesMenu.SetActive(false);
        confirmMenu.SetActive(true);
        savesConfirms[1].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

    /// <summary>
    /// Save on the third save
    /// </summary>
    public void OnSave3Clicked()
    {
        savesMenu.SetActive(false);
        confirmMenu.SetActive(true);
        savesConfirms[2].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

	/// <summary>
	/// Load the first save
	/// </summary>
	public void OnLoad1Clicked()
	{
        savesMenu.SetActive(false);
        confirmMenu.SetActive(true);
        loadConfirms[0].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

    /// <summary>
    /// Load the second save
    /// </summary>
    public void OnLoad2Clicked()
    {
        savesMenu.SetActive(false);
        confirmMenu.SetActive(true);
        loadConfirms[1].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

    /// <summary>
    /// Load the third save
    /// </summary>
    public void OnLoad3Clicked()
    {
        savesMenu.SetActive(false);
        confirmMenu.SetActive(true);
        loadConfirms[2].SetActive(true);

        // Print
        confirmMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

	private IEnumerator waitForCapture(int saveNumber)
	{
		yield return null;
        savesConfirms[1].SetActive(false);
        confirmMenu.SetActive(false);
        savesMenu.SetActive(false);

        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Application.dataPath +
			"/Resources/Graphics/Menus/Screenshot" + saveNumber + ".png");

        confirmMenu.SetActive(false);
        savesMenu.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        savesMenu.GetComponent<LoadSaves>().ReloadInformations();
    }

    /// <summary>
    /// Confirm the save on the first save
    /// </summary>
    public void OnConfirmSave1Clicked()
    {
        // Make and save the screenshot
		StartCoroutine(waitForCapture(1));

        // Save on the first
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
		string dialogName = GetComponent<ReadText>().getDialogName();
		int currentLine = GetComponent<ReadText>().getLineNumber();
		bool isGameScene = GetComponent<ReadText>().getGameScene();
		int currentBackground = GetComponent<ReadText>().getCurrentBackground();
        int currentFrame = GetComponent<ReadText>().getCurrentFrame();
        char currentTextBox = GetComponent<ReadText>().getCurrentTextBox();
        bool[] currentCharacters = GetComponent<ReadText>().getCurrentCharacters();
		int[] currentExpressions = GetComponent<ReadText>().getCurrentExpressions();
		float[] currentPositions = GetComponent<ReadText>().getCurrentPositions();
        appState.Save(1, dialogName, currentLine, isGameScene,
            currentBackground, currentFrame, currentTextBox,
            currentCharacters, currentExpressions, currentPositions);
    }

    /// <summary>
    /// Confirm the save on the second save
    /// </summary>
    public void OnConfirmSave2Clicked()
	{
        // Make and save the screenshot
        StartCoroutine(waitForCapture(2));
        
        // Save on the second
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        string dialogName = GetComponent<ReadText>().getDialogName();
        int currentLine = GetComponent<ReadText>().getLineNumber();
        bool isGameScene = GetComponent<ReadText>().getGameScene();
        int currentBackground = GetComponent<ReadText>().getCurrentBackground();
        int currentFrame = GetComponent<ReadText>().getCurrentFrame();
        char currentTextBox = GetComponent<ReadText>().getCurrentTextBox();
        bool[] currentCharacters = GetComponent<ReadText>().getCurrentCharacters();
        int[] currentExpressions = GetComponent<ReadText>().getCurrentExpressions();
        float[] currentPositions = GetComponent<ReadText>().getCurrentPositions();
        appState.Save(2, dialogName, currentLine, isGameScene,
            currentBackground, currentFrame, currentTextBox,
            currentCharacters, currentExpressions, currentPositions); ;
    }

    /// <summary>
    /// Confirm the save on the third save
    /// </summary>
    public void OnConfirmSave3Clicked()
    {
        // Make and save the screenshot
        StartCoroutine(waitForCapture(3));

        // Save on the third
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        string dialogName = GetComponent<ReadText>().getDialogName();
        int currentLine = GetComponent<ReadText>().getLineNumber();
        bool isGameScene = GetComponent<ReadText>().getGameScene();
        int currentBackground = GetComponent<ReadText>().getCurrentBackground();
        int currentFrame = GetComponent<ReadText>().getCurrentFrame();
        char currentTextBox = GetComponent<ReadText>().getCurrentTextBox();
        bool[] currentCharacters = GetComponent<ReadText>().getCurrentCharacters();
        int[] currentExpressions = GetComponent<ReadText>().getCurrentExpressions();
        float[] currentPositions = GetComponent<ReadText>().getCurrentPositions();
        appState.Save(3, dialogName, currentLine, isGameScene,
            currentBackground, currentFrame, currentTextBox,
            currentCharacters, currentExpressions, currentPositions); ;
    }

    /// <summary>
    /// Confirm the load of the first save
    /// </summary>
    public void OnConfirmLoad1Clicked()
	{

        loadConfirms[0].SetActive(false);
        confirmMenu.SetActive(false);

        // Load the first
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
		appState.Load(1);
        SceneManager.LoadSceneAsync("Story");
    }

    /// <summary>
    /// Confirm the load of the second save
    /// </summary>
    public void OnConfirmLoad2Clicked()
    {
        loadConfirms[1].SetActive(false);
        confirmMenu.SetActive(false);

        // Load the second
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Load(2);
        SceneManager.LoadSceneAsync("Story");
    }

    /// <summary>
    /// Confirm the load of the third save
    /// </summary>
    public void OnConfirmLoad3Clicked()
    {
        loadConfirms[2].SetActive(false);
        confirmMenu.SetActive(false);

        // Load the third
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Load(3);
        SceneManager.LoadSceneAsync("Story");
    }

    public void OnCloseConfirmClicked()
	{
		for (int i = 0; i < 3; i++)
		{
			savesConfirms[i].SetActive(false);
			loadConfirms[i].SetActive(false);
		}
		confirmMenu.SetActive(false);
		savesMenu.SetActive(true);
	}

    /// <summary>
    /// Tells if the auto mode is on
    /// </summary>
    /// <returns>The auto mode</returns>
    public bool getAuto()
	{
		return isAuto;
	}

	/// <summary>
	/// Tells if the log is open
	/// </summary>
	/// <returns>The log state</returns>
	public bool getLog()
	{
		return isLog;
	}

	/// <summary>
	/// Tells if the saves menu is open
	/// </summary>
	/// <returns>The saves menu state</returns>
	public bool getSave()
	{
		return isSave;
	}
}
