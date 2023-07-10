using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GoToMainScene() {
        SceneManager.LoadScene("MainScene");
    }
    public void GoToCellularAutomataScene() {
        SceneManager.LoadScene("CellularAutomataScene");
    }
}
