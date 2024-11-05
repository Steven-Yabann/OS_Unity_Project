using UnityEngine;
using System.Collections;

public class HorizontalSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;        // Reference to the Prefab to spawn
    public float spacing = 1.5f;            // Distance between squares in a row
    private int spawnCount = 0;             // Tracks the number of objects spawned in the current row
    private int rowCount = 0;               // Tracks the number of rows spawned
    public float moveSpeed = 2f;            // Speed at which squares move up
    private bool isSpawningActive = false;  // Flag to check if spawning is active

    private float stopHeight;                // Height at which squares stop moving
    private float lastRowStopY = 0f;         // Y position of the last row's stop height
    private float initialSpawnY = 0f;        // Y position to spawn the first row

    private Color[] colors = { Color.red, Color.green, Color.blue }; // Colors for each row
    private int currentColorIndex = 0;      // Index to track the current color

    void Start()
    {
        // Calculate stopping height based on camera size
        float screenHeight = Camera.main.orthographicSize * 2; // Total height of the screen
        stopHeight = screenHeight * 0.35f; // Adjust this value to set the stop height
        
        // Set the initial spawn Y position based on the screen bottom
        initialSpawnY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y + 1f;
    }

    void Update()
    {
        // Check for Space key press to spawn squares
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSingleSquare();
        }

        // Check for Up Arrow key press to start moving squares
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isSpawningActive = true; // Activate movement
        }

        // Move squares up the screen if spawning is active
        if (isSpawningActive)
        {
            MoveSquaresUp();
        }
    }

    void SpawnSingleSquare()
    {
        // Limit the total squares in one row to 5
        if (spawnCount >= 5)
        {
            // Reset the spawn count for a new row and increment row count
            spawnCount = 0;
            rowCount++;
            // Cycle to the next color in the array
            currentColorIndex = (currentColorIndex + 1) % colors.Length;
        }

        // Calculate the y position based on the number of rows and spacing
        float spawnY = initialSpawnY + (rowCount * spacing);

        // Calculate the x position based on the current spawn count
        float spawnX = spawnCount * spacing;

        // Spawn the object at the calculated position
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        GameObject newSquare = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

        // Assign the color to the square's material
        Renderer renderer = newSquare.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = colors[currentColorIndex];
        }

        // Increment the spawn count
        spawnCount++;
    }

    void MoveSquaresUp()
    {
        // Find all objects of the prefab type with the "Square" tag and move them up
        GameObject[] squares = GameObject.FindGameObjectsWithTag("Square"); // Use "Square" tag directly

        foreach (GameObject square in squares)
        {
            // Move each square upwards at the specified speed
            float newYPosition = square.transform.position.y + (moveSpeed * Time.deltaTime);

            // Stop moving if the new position exceeds the stop height
            if (newYPosition >= stopHeight)
            {
                newYPosition = stopHeight; // Set to stop height
            }

            // Update the square's position
            square.transform.position = new Vector3(square.transform.position.x, newYPosition, square.transform.position.z);
        }

        // Start vanishing squares once they reach the stop height
        if (squares.Length > 0 && squares[0].transform.position.y >= stopHeight)
        {
            StartCoroutine(VanishSquares(squares));
        }
    }

    private IEnumerator VanishSquares(GameObject[] squares)
    {
        foreach (GameObject square in squares)
        {
            // Wait for 1.5 seconds before destroying the square
            yield return new WaitForSeconds(1.5f);
            Destroy(square); // Remove the square from the scene
        }
    }
}
