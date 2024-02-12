using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndingManager : MonoBehaviour
{
    public enum GameOverState{
        Caught,
        Dead,
        TimeEnd,
        ObjectifFailled
    }

    public GameObject victoryUI;
    public GameObject gameOverUI;
    public AudioClip _bell;
    public Text text;
    public string successMessage = "Mission réussite";
    public string failureMessage = "Mission échouée \n Lord Jasper est déçu de vous";
    
    
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
        } else if(gameOverState == GameOverState.ObjectifFailled){
            text.text = "You failed your objective!";
        }
        gameOverUI.SetActive(true);
        Time.timeScale = 0;
        AppState.GetInstance().vie--;
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
