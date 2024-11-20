using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public static DrawerController Instance;

    private bool[] flag = new bool[2] { false, false }; // Flags for Peterson’s Algorithm
    private int turn = 0; // Variable to determine whose turn it is

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool[] Flag => flag; // Accessor for the flag array
    public int Turn { get => turn; set => turn = value; } // Accessor for turn
}
