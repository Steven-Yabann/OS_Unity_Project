using UnityEngine;
using UnityEngine.SceneManagement;

public class CPUSchedulingNav : MonoBehaviour
{
    public void LoadFCFS() 
    {
        SceneManager.LoadScene("FcfsScene");
    }

    public void LoadSJF()
    {
        SceneManager.LoadScene("SjfScene");
    }

    public void LoadCPUMenu()
    {
        SceneManager.LoadScene("CPU_Menu");
    }
}
