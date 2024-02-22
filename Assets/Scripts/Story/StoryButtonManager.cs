using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

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
	private bool isLog = false;		// Indicates if the log is printed or not (for the ReadText script)

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
		StartCoroutine(GetComponent<ReadText>().End());
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
}
