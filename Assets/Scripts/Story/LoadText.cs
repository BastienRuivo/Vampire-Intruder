using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Make the text load progressibely in the TextBox
/// </summary>
[RequireComponent(typeof(Text))]
public class LoadText : MonoBehaviour
{
	// Public Game Objects
	public Text textObject;					// The phrase text in the text box
	public Image arrowObject;				// The arrow image
	public Image shadowObject;              // The arrow's shadow image

	// Private code variables
	private string printedText;				// The text actually printed
	private string totalText;               // The total to print
	private bool coroutineProtect;			// Tells if the animation is still ongoing
	private bool printText;					// Tells if there is still text to print
	private int textSize;

	/// <summary>
	/// Initialize the text and printing text
	/// </summary>
	private void Awake()
	{
		totalText = null;
		textSize = 0;
		coroutineProtect = false;
		printText = false;
		printedText = null;
	}

	/// <summary>
	/// Make the text appear progressively
	/// </summary>
	private void FixedUpdate()
	{
		// If there is still text to print
		if (printText && coroutineProtect && textSize < totalText.Length)
		{
			// Print one more letter
			printedText += totalText[textSize++];
			textObject.text = printedText;
		}
	}

	private void Update()
	{
		// Launch the printing animation
		if (printText && !coroutineProtect)
		{
			textSize = 0;

			// Do not show the arrow while the text is still printing
			arrowObject.enabled = false;
			shadowObject.enabled = false;

			coroutineProtect = true;
		}
		// End the printing animation
		else if(printText && coroutineProtect && textSize == totalText.Length)
		{
			coroutineProtect = false;
			printText = false;

			// Show the arrow when the text has ended printing
			arrowObject.enabled = true;
			shadowObject.enabled = true;
		}
	}


	/// <summary>
	/// Reset the text to print and the animation
	/// </summary>
	public void Reset()
	{
		// Reset the text from the textbox text
		totalText = null;
		if (textObject.text != "")
		{
			totalText = textObject.text;
		}
		// Removes the text from the textbox
        textObject.text = null;

        // No printed text at the beginning
        printedText = null;

		// Launch the text loading if there is text to print
		if (totalText != null)
		{
			printText = true;
		}

		// No coroutine launch until the next update
        coroutineProtect = false;
    }
	
	/// <summary>
	/// Tells if the text is in a printing state
	/// </summary>
	/// <returns>The text print state</returns>
	public bool GetLoadText()
	{
		return printText;
	}

	/// <summary>
	/// Directly print all the text
	/// </summary>
	public void EndPhrase()
	{
		// End the animation
		coroutineProtect = false;
		printText = false;

		// Show the arrow
		arrowObject.enabled = true;
		shadowObject.enabled = true;

		// Put all the text
		textObject.text = totalText;
	}
}
