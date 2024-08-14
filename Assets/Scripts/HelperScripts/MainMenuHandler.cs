/// |-----------------------------------------Settings Handler-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the main menu screen
/// |-------------------------------------------------------------------------------------------------------------|
/// 
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour {

    public void LoadGame() {
        SceneManager.LoadScene("RAF_Scene", LoadSceneMode.Single);
    }

    public void CloseGame() {
        Application.Quit();
    }
}
