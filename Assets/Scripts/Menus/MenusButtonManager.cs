using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Manage the story mode buttons
/// </summary>
public class MenusButtonManager : MonoBehaviour
{
	// Tap effect
	public Image[] tap;             // The tap effect images
	public GameObject savesMenu;    // The saves menu to open or close
	public GameObject[] characters;	// The characters to make appear
	public GameObject title;        // The title to make appear
	public GameObject explanation;  // The explanation to make appear
	public GameObject[] buttons;	// The buttons to make appear

	void Awake()
	{
		for (int i = 0; i < characters.Length; i++)
		{
			characters[i].GetComponent<MenusFadeInOut>().SetOpacity(0f);
		}
		title.GetComponent<MenusFadeInOut>().SetOpacity(0f);
		title.GetComponent<MenusFadeInOut>().SetSpeed(2.0f);
        if (explanation != null)
        {
            explanation.GetComponent<MenusFadeInOut>().SetOpacity(0f);
            explanation.GetComponent<MenusFadeInOut>().SetSpeed(4.0f);
        }
        for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].GetComponent<MenusFadeInOut>().SetOpacity(0f);
			buttons[i].GetComponent<MenusFadeInOut>().SetSpeed(2.0f);
            buttons[i].GetComponent<Button>().interactable = false;
        }

	}

    void Start()
    {
	    if (characters.Length > 0)
        {
            characters[0].GetComponent<MenusFadeInOut>().LaunchFadeIn();
        }
    }

    /// <summary>
	/// Manage the tap effect when the mouse click
	/// </summary>
    void FixedUpdate()
    {
		// Objects Fade in out launching animations

		for (int i = 1; i < characters.Length;i++)
		{
			if (characters[i - 1].GetComponent<MenusFadeInOut>().GetOpacity() > 0.999f &&
                characters[i].GetComponent<MenusFadeInOut>().GetOpacity() < 0.001f)
			{
                characters[i].GetComponent<MenusFadeInOut>().LaunchFadeIn();
            }
		}

		if ((characters.Length == 0 ||
            characters[characters.Length - 1].GetComponent<MenusFadeInOut>().GetOpacity() > 0.999f)
			&& title.GetComponent<MenusFadeInOut>().GetOpacity() < 0.001f)
		{
			title.GetComponent<MenusFadeInOut>().LaunchFadeIn();
        }

        if (explanation != null && title.GetComponent<MenusFadeInOut>().GetOpacity() > 0.999f
            && explanation.GetComponent<MenusFadeInOut>().GetOpacity() < 0.001f)
        {
            explanation.GetComponent<MenusFadeInOut>().LaunchFadeIn();
        }

        if (((explanation != null && explanation.GetComponent<MenusFadeInOut>().GetOpacity() > 0.999f)
            || (explanation == null && title.GetComponent<MenusFadeInOut>().GetOpacity() > 0.999f))
            && buttons[0].GetComponent<MenusFadeInOut>().GetOpacity() < 0.001f)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GetComponent<MenusFadeInOut>().LaunchFadeIn();
                buttons[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    void Update()
    {
        // Tap effect when the mouse click
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < tap.Length; i++)
            {
                // Get the mouse position
                tap[i].GetComponent<Transform>().position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                // Launch the animation
                tap[i].GetComponent<Animator>().SetTrigger("tap");
            }
        }
    }

    /// <summary>
    /// Launch the game from the start
    /// </summary>
    public void OnLaunchClicked()
	{
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Initialize();
        SceneManager.LoadSceneAsync("Story");
    }

	/// <summary>
	/// Open the saves menu
	/// </summary>
	public void OnSavesClick()
    {
        savesMenu.GetComponent<LoadSaves>().ReloadInformations();
        savesMenu.SetActive(true);
	}

	/// <summary>
	/// Charge the first save
	/// </summary>
	public void OnLoad1Click()
	{
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Load(1);
        SceneManager.LoadSceneAsync("Story");
    }

    /// <summary>
    /// Charge the second save
    /// </summary>
    public void OnLoad2Click()
    {
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Load(2);
        SceneManager.LoadSceneAsync("Story");
    }

    /// <summary>
    /// Charge the third save
    /// </summary>
    public void OnLoad3Click()
    {
        AppState appState = GameObject.Find("AppState").GetComponent<AppState>();
        appState.Load(3);
        SceneManager.LoadSceneAsync("Story");
    }

    /// <summary>
    /// Close the saves menu
    /// </summary>
    public void OnSaveBackClick()
    {
		savesMenu.SetActive(false);
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void OnQuitClicked()
	{
		#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
		#endif
        Application.Quit();
    }
}
