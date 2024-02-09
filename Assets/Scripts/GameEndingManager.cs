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
    
    
    public static GameEndingManager instance;

    private void Awake(){
        if(instance != null){
            Debug.LogWarning("Multiple GameEndingManager");
            return;
        }
        instance = this;
    }

    public void onPlayerVictory(){
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
