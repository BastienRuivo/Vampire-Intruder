using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndingManager : MonoBehaviour
{
    public enum GameOverState{
        Caught,
        Dead,
        TimeEnd,
    }

    public GameObject victoryUI;
    public GameObject gameOverUI;
    public AudioClip _bell;
    public Text text;
    public string successMessage = "Mission r�ussite";
    public string failureMessage = "Mission �chou�e \n Lord Jasper est d��u de vous";
    
    
    public static GameEndingManager instance;

    private void Awake(){
        if(instance != null){
            Debug.LogWarning("Multiple GameEndingManager");
            return;
        }
        instance = this;
    }

    public void onPlayerExtraction(){
        var main = GameController.GetInstance().objectivesToComplete.Find(o => o.isMain);
        if(main == null){
            Debug.LogError("No main objective found");
            return;
        }
        var textObj = victoryUI.GetComponentInChildren<Text>();
        if (main.state == GameController.ObjectiveState.DONE)
        {
            textObj.text = successMessage;
            textObj.color = Color.green;
        }
        else
        {
            textObj.text = failureMessage;
            textObj.color = Color.red;
            AppState.GetInstance().nbObjectifSkip++;
        }
        victoryUI.SetActive(true);
        Time.timeScale = 0;
        AudioManager.GetInstance().playClip(_bell, transform.position);
    }

    public void onPlayerDeath(GameOverState gameOverState){
        if(gameOverState == GameOverState.Caught){
            text.text = "You have been caught!";
        } else if(gameOverState == GameOverState.Dead){
            text.text = "You are dead!";
        } else if(gameOverState == GameOverState.TimeEnd){
            text.text = "Time is over!";
        }

        gameOverUI.SetActive(true);
        Time.timeScale = 0;
        AppState.GetInstance().vie--;
    }

    public void nextLevelButton(){
        victoryUI.SetActive(false);
        Time.timeScale = 1;
        Debug.Log("Not implemented yet. a modifier dans GameEndingManager.cs ligne 68");
        //SceneManager.LoadScene("Nom de la scene suivante");
    }

    public void retryButtonGO(){
        gameOverUI.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void mainMenuButton(){
        //SceneManager.LoadScene("MainMenu");
        Debug.Log("Not implemented yet.");
    }

    public void quitButton(){
        Application.Quit();
    }

}
