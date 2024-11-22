using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Method to load a scene by name
    public void LoadCpuScheduling()
    {
        SceneManager.LoadScene("CPU_Menu");
    }

    public void LoadProcessSync()
    {
        SceneManager.LoadScene("Process_Synchronisation");
    }
    public void LoadMmu()
    {
        SceneManager.LoadScene("MemoryManagement");
    }

    // Method to quit the application
    public void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Application Quit!"); // This won't appear in a built game but is useful for testing in the editor
    }
}
