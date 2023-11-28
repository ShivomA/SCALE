using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    public GameObject mainMenuPanel;
    public GameObject levelSelectionMenuPanel;
    public GameObject contolsMenuPanel;
    public GameObject hintsMenuPanel;

    private void Start() {
        levelSelectionMenuPanel.SetActive(false);
        contolsMenuPanel.SetActive(false);
        hintsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void LoadScene(int index) {
        SceneManager.LoadScene(index);
    }

    public void Quit() {
        Application.Quit();
    }

    public void ChangePanel(int activeMenu) {
        levelSelectionMenuPanel.SetActive(false);
        contolsMenuPanel.SetActive(false);
        hintsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(false);

        if (activeMenu == 1) {
            levelSelectionMenuPanel.SetActive(true);
        }else if (activeMenu == 2) {
            contolsMenuPanel.SetActive(true);
        } else if (activeMenu == 3) {
            hintsMenuPanel.SetActive(true);
        } else {
            mainMenuPanel.SetActive(true);
        }
    }
}
