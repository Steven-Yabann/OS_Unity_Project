using UnityEngine;
using System.Collections;

public class HorizontalSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;        // Reference to the Prefab to spawn
    public float spacing = 1.5f;            // Distance between objects
    private int spawnCount = 0;             // Tracks the number of objects spawned
    public float moveSpeed = 2f;            // Speed at which squares move up
    private bool isSpawningActive = true;   // Flag to check if spawning is active

    void Start()
    {
        // Start the spawning process
        StartCoroutine(SpawnSquares());
    }

    void Update()
    {
        // If spawning is active, check for Space key press
        if (isSpawningActive && Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSingleSquare();
            spawnCount++;
        }

        // Move squares up the screen if spawning is not active
        if (!isSpawningActive)
        {
            MoveSquaresUp();
        }
    }

    IEnumerator SpawnSquares()
    {
        // Allow 10 seconds for the user to spawn squares
        yield return new WaitForSeconds(10f);

        // End initial spawning
        isSpawningActive = false;
    }

    void SpawnSingleSquare()
    {
        // Calculate the y position at the bottom of the screen
        float screenBottomY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y + 1f;

        // Calculate the x position based on the current spawn count
        float spawnX = spawnCount * spacing;

        // Spawn the object at the calculated position
        Vector3 spawnPosition = new Vector3(spawnX, screenBottomY, 0);
        GameObject newSquare = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        
        // Optionally, log to check if the square is spawned correctly
        Debug.Log("Spawned square: " + newSquare.name);
    }

    void MoveSquaresUp()
    {
        // Find all objects of the prefab type with the "Square" tag and move them up
        GameObject[] squares = GameObject.FindGameObjectsWithTag("Square"); // Use "Square" tag directly
        foreach (GameObject square in squares)
        {
            // Move each square upwards at the specified speed
            square.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
    }
}
