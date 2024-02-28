using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            GameController.GetGameMode().MessageToUser(new GameController.UserMessageData(
                GameController.UserMessageData.MessageToUserSenderType.Player,
                "Je n'ai pas le temps de faire de pause...",
                1.5f,
                priority:GameController.UserMessageData.MessageToUserScheduleType.ImportanceOnReadability));
    }

    private void pause(){
        //PlayerMovement.instance.enabled = false;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void resume(){
        //PlayerMovement.instance.enabled = true;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
}
