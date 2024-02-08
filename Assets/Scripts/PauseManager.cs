using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI;

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(isPaused){
                resume();
            }else{
                pause();
            }
        }
        
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
