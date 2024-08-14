/// |-----------------------------------------Settings Handler-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the settings screen to modify specific settings for users
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour {
    // Accessible Properties
    [SerializeField] InputActionProperty pauseKey;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject worldSpaceUI;

    // Private Variables
    private bool gamePaused = false;

    // Start is called before the first frame update
    void Start() {
        // Set up the action behaviors
        pauseKey.action.Enable();
    }

    // Update is called once per frame
    void Update() {
        if(pauseKey.action.triggered) { TogglePauseGame(!gamePaused); }
    }

    public void TogglePauseGame (bool pause) {
        if (pause) {
            Time.timeScale = 0;
            gamePaused = true;
            enableCursor();
            OpenMenu();
        } else {
            Time.timeScale = 1;
            gamePaused = false;
            disableCursor();
            CloseMenu();
        }
    }

    void OpenMenu()  { settingsMenu.SetActive(true); worldSpaceUI.SetActive(false); }
    void CloseMenu() { settingsMenu.SetActive(false); worldSpaceUI.SetActive(true); }
    void enableCursor() {
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.None;
    }
    void disableCursor() {
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResetScene() {
        TogglePauseGame(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CloseGame() {
        Application.Quit();
    }
}
