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
    public string successMessage = "Mission réussite";
    public string failureMessage = "Mission échou�e \n Lord Jasper est déçu de vous";
    
    
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
        AudioManager.GetInstance().playClip(_bell, transform.position);
        if(main == null){
            Debug.LogError("No main objective found");
            return;
        }
        if (main.state == GameController.ObjectiveState.DONE)
        {
            AppState.GetInstance().endLevel(true,false,false,false);
        }
        else
        {
            AppState.GetInstance().endLevel(false,false,false,false);
        }
        Time.timeScale = 0;

    }

    public void onPlayerDeath(GameOverState gameOverState){
        if(gameOverState == GameOverState.Caught)
        {
            AppState.GetInstance().endLevel(false,true,false,false);
        } 
        else if(gameOverState == GameOverState.Dead)
        {
            AppState.GetInstance().endLevel(false,false,false,true);
        } 
        else if(gameOverState == GameOverState.TimeEnd)
        {
            AppState.GetInstance().endLevel(false,false,true,false);
        }
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
