using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public static DrawerController Instance; // Singleton instance

    public bool[] Flag = new bool[3]; // Flags for all three tellers
    public int Turn; // Shared turn variable

    private void Awake()
    {
        // Ensure only one instance of DrawerController exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
