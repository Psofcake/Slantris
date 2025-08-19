using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIButtonScript : MonoBehaviour,IPointerEnterHandler
{
    public GameObject uiButton;
    public GameObject pauseText;
    
    private AudioManager sound;
    private bool isPaused = false;
    private void Start()
    {
        sound = AudioManager.Instance;
    }

    public void LoadSlantris()
    {   
        Debug.Log("LoadSlantris");
        sound.PlayClickSound();
        SceneManager.LoadScene(1);
    }
    
    // public void RestartGame()
    // {
    //     Debug.Log("RestartGame");
    //     sound.PlayClickSound();
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    // }
    public void LoadTitleMenu()
    {
        Debug.Log("LoadTitleMenu");
        sound.PlayClickSound();
        SceneManager.LoadScene(0);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseText.gameObject.SetActive(true);
            for (int i = 0; i < uiButton.transform.childCount; i++)
            {
                uiButton.transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
        {
            Time.timeScale = 1f;
            pauseText.gameObject.SetActive(false);
            for (int i = 0; i < uiButton.transform.childCount; i++)
            {
                uiButton.transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }
    public void QuitGame()
    {
        Debug.Log("QuitGame");
        sound.PlayClickSound();
        Application.Quit();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Debug.Log("OnPointerEnter");
        sound.PlayButtonSound();
    }
}
