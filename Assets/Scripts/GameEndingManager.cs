using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameEndingManager;


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

        var seconds = GameController.GetInstance().objectivesToComplete.FindAll(o => !o.isMain);
        int totalSeconds = seconds.Count;
        int achievedSeconds = 0;
        for(int i = 0; i < totalSeconds; i++)
        {
            if(seconds[i].state == GameController.ObjectiveState.DONE)
            {
                achievedSeconds++;
            }
        }

        Debug.Log("Total secondary objectives: " + totalSeconds);
        AppState.GetInstance().setTotalSecondaryObjectivesInCurrentScene(totalSeconds);
        AppState.GetInstance().setSecondaryObjectivesAchievedInCurrentScene(achievedSeconds);

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
        
        SceneManager.LoadScene("Story");
    }

    public void onPlayerDeath(GameOverState gameOverState)
    {
        StartCoroutine(DeathCoroutine(gameOverState));
    }

    public IEnumerator DeathCoroutine(GameOverState gameOverState)
    {
        while (!GameController.GetInstance().endFadeIn)
        {
            yield return null;
        }
        if (gameOverState == GameOverState.Caught)
        {
            AppState.GetInstance().endLevel(false, false, true, false);
        }
        else if (gameOverState == GameOverState.Dead)
        {
            AppState.GetInstance().endLevel(false, false, false, true);
        }
        else if (gameOverState == GameOverState.TimeEnd)
        {
            AppState.GetInstance().endLevel(false, true, false, false);
        }



        SceneManager.LoadScene("Story");
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
