using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Make an Image Game Object fade in or out
/// </summary>
public class MenusFadeInOut : MonoBehaviour {

	// Public variable
    public float fadeSpeed = 0.5f;		// The Fade In / Out speed

    // Private variables
    [Range(0.0f, 1.0f)]
    private float opacity = 1.0f;		// The current object opacity
	private bool animationOn;			// Indicates if the Fade In / Out animation is ongoing
	private bool fadeIn;                // Indicates if the animation is Fade In or Fade Out

    /// <summary>
    /// Update the object opacity
    /// </summary>
    void Update () {

		// Change the opacity if the animation is ongoing
		if (animationOn)
		{
			// Fade In option
			if (fadeIn)
			{
				// Up the opacity
				opacity += Time.deltaTime * fadeSpeed;

                // Stops the animation when the opacity is at its maximum
                if (opacity >= 1.0f)
				{
					opacity = 1.0f;
					animationOn = false;
				}
			}
			// Fade Out option
			else
			{
				// Down the opacity
				opacity -= Time.deltaTime * fadeSpeed;

				// Stops the animation when the opacity is at its minimum
				if (opacity <= 0.0f)
				{
					opacity = 0.0f;
					animationOn = false;
				}
			}

            // Set the opacity
            gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, opacity);

        }
	}

	public void LaunchFadeIn()
	{
		fadeIn = true;
		opacity = 0.0f;
		animationOn = true;
	}

	public void LaunchFadeOut()
	{
		fadeIn = false;
		opacity = 1.0f;
		animationOn = true;
	}

	/// <summary>
	/// Tells if the animation is ongoing
	/// </summary>
	/// <returns></returns>
	public bool GetAnim()
	{
		return animationOn;
	}

	/// <summary>
	/// Set the opacity to a desired value
	/// </summary>
	/// <param name="op">The desired opacity value</param>
	public void SetOpacity(float op)
	{
		// Verify the desired opacity values (0.0f - 1.0f)
		if (op > 1.0f)
		{
			op = 1.0f;
		}
		else if (op < 0.0f)
		{
			op = 0.0f;
		}
		
		// Set the opacity
		opacity = op;
        gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, opacity);
    }

	/// <summary>
	/// Returns the current opacity value
	/// </summary>
	/// <returns>The current opacity value</returns>
	public float GetOpacity()
	{
		return opacity;
	}

	public void SetSpeed(float speed)
	{
		fadeSpeed = speed;
	}
}