using UnityEngine;

/// <summary>
/// Make the log screen scroll
/// </summary>
public class ScrollLog : MonoBehaviour
{
	// Public variables
	public GameObject StoryUI;								// The story UI

	// Private code variables
	private float targetPosition = 0.0f;                    // The scroll height target
    private Vector2 position = new Vector2(0.0f, 0.0f);     // The scroll position
    private bool goesUp = false;							// Indicates if the log should go up
	private bool goesUpPart1 = false;						// Indicates if this is the first up part (goes up)
    private bool goesUpPart2 = false;						// Indicates if this is the second up part (goes a bit back down)
    private bool goesDown = false;							// Indicates if the log should go down
    private bool goesDownPart1 = false;						// Indicates if this is the first down part (goes down)
    private bool goesDownPart2 = false;						// Indicates if this is the second down part (goes a bit back up)

    /// <summary>
	/// Actually updates the log position
	/// </summary>
    void Update()
    {
		gameObject.GetComponent<RectTransform>().anchoredPosition = position;
	}

	/// <summary>
	/// Update the log position
	/// </summary>
	private void FixedUpdate()
	{
        float screenSize = Screen.height;
		float scrollSpeed = screenSize * 0.05f;

        // Not action but still needs to be put up or down
        if (!goesUp && !goesDown)
		{
			// Needs to be put a bit up
			if (position.y < targetPosition - scrollSpeed)
			{
				position.y += scrollSpeed;
			}
			// End to be put up
			else if (position.y < targetPosition)
			{
				position.y = targetPosition;
			}
			// Needs to be put a bit down
			else if (position.y > targetPosition + scrollSpeed)
			{
				position.y -= scrollSpeed;
			}
			// End to be put down
			else if (position.y > targetPosition)
			{
				position.y = targetPosition;
			}
		}
		// Needs to be put up
		else if (goesUp)
		{
			// Part 1 : do up
			if (goesUpPart1)
			{
				position.y += scrollSpeed;
			}

			// Part 1 end
			if(position.y > screenSize * 0.3f && goesUpPart1)
			{
				goesUpPart1 = false;
				goesUpPart2 = true;
			}

			// Part 2 : go back a bit down
			if (goesUpPart2)
			{
				position.y -= scrollSpeed * 0.5f;
			}

			// Part 2 end
			if(position.y < 0.0f && goesUpPart2)
			{
				position.y = 0.0f;
				goesUpPart2 = false;
				goesUp = false;
			}
		}
		// Needs to be put down
		else if (goesDown)
		{
			// The current log height
			float height = StoryUI.GetComponent<ReadText>().getLogHeight();

			// Part 1 : go down
			if (goesDownPart1)
			{
				position.y -= scrollSpeed;
			}
			
			// Part 1 end
			if ((height <= screenSize * 0.2f && position.y < -screenSize * 0.2f) || (height > screenSize * 0.2f && position.y < -(height - screenSize * 0.2f)) && goesDownPart1)
			{
				goesDownPart1 = false;
				goesDownPart2 = true;
			}

			// Part 2 : go back a bit up
			if (goesDownPart2)
			{
				position.y += scrollSpeed * 0.5f;
			}

			// Part 2 end
			if ((height <= screenSize * 0.5f && position.y > 0.0f) || (height > screenSize * 0.5f && position.y > -(height - screenSize * 0.5f)) && goesDownPart2)
			{
				if (height <= screenSize * 0.5f)
				{
					position.y = 0.0f;
				}
				else
				{
					position.y = -(height - screenSize * 0.5f);
				}

				goesDownPart2 = false;
				goesDown = false;
			}
		}
	}

	/// <summary>
	/// Updates the log position thanks to the scroll
	/// </summary>
	void OnGUI()
	{
		float screenSize = Screen.height;

		// Update the log target position thanks to the scroll
		if (StoryUI.GetComponent<StoryButtonManager>().getLog())
		{
			targetPosition += -Input.mouseScrollDelta.y * screenSize / 10.0f;
		}

		// The current log height
		float height = StoryUI.GetComponent<ReadText>().getLogHeight();

		// Goes up if the target is up the actual position
		if (targetPosition < -(height - screenSize * 0.5f) && height > screenSize * 0.5f)
		{
			if(!goesUp && !goesDown)
			{
				goesDown = true;
				goesDownPart1 = true;
			}
			targetPosition = -(height - screenSize * 0.5f);
		}
        // Goes up if the target is up the actual position
        else if (height <= screenSize * 0.5f && targetPosition < 0.0f)
		{
			if (!goesUp && !goesDown)
			{
				goesDown = true;
				goesDownPart1 = true;
			}
			targetPosition = 0.0f;
		}

		// Goes down if the target is down
		if (targetPosition > 0.0f)
		{
			if(!goesUp && !goesDown)
			{
				goesUp = true;
				goesUpPart1 = true;
			}
			targetPosition = 0.0f;
		}
	}

	/// <summary>
	/// Reset the scroll log position to the bottom
	/// </summary>
	public void Reset()
	{
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
		targetPosition = 0.0f;
		position = new Vector2(0.0f, 0.0f);
	}
}
