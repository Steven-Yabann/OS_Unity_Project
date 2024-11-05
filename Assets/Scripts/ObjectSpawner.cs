using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HorizontalSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float spacing = 1.5f;
    private int spawnCount = 0;
    private List<RowData> rows = new List<RowData>(); // List to store each row's data
    public float moveSpeed = 2f;
    private bool isSpawningActive = false;

    private float stopHeight;
    private float initialSpawnY = 0f;

    private bool isFCFS = true; // True for FCFS, False for SRTF

    public Button fcfsButton; // Button to select FCFS
    public Button srtfButton; // Button to select SRTF
    public Button startButton; // Button to start the simulation

    void Start()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        stopHeight = screenHeight * 0.35f;
        initialSpawnY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y + 1f;

        // Add listeners to buttons
        fcfsButton.onClick.AddListener(SelectFCFS);
        srtfButton.onClick.AddListener(SelectSRTF);
        startButton.onClick.AddListener(StartSimulation);
    }

    void Update()
    {
        // Space to spawn squares in a row
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSingleSquare();
        }

        // Enter to start a new row
        if (Input.GetKeyDown(KeyCode.Return) && spawnCount > 0)
        {
            StartNewRow();
        }

        // Up Arrow to start moving squares
        if (Input.GetKeyDown(KeyCode.UpArrow) && isSpawningActive)
        {
            if (isFCFS)
                ProcessFCFS();
            else
                ProcessSRTF();
        }
    }

    void SelectFCFS()
    {
        isFCFS = true;
        Debug.Log("Selected FCFS scheduling");
    }

    void SelectSRTF()
    {
        isFCFS = false;
        Debug.Log("Selected SRTF scheduling");
    }

    void StartSimulation()
    {
        isSpawningActive = true;
        Debug.Log("Simulation started");
    }

    void SpawnSingleSquare()
    {
        if (spawnCount >= 5)
        {
            StartNewRow();
        }

        float spawnY = initialSpawnY + (rows.Count * spacing);
        float spawnX = spawnCount * spacing;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        GameObject square = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

        if (rows.Count == 0 || spawnCount == 0)
            rows.Add(new RowData { squares = new List<GameObject>(), spawnTime = Time.time });

        rows[rows.Count - 1].squares.Add(square);
        spawnCount++;
    }

    void StartNewRow()
    {
        spawnCount = 0;
    }

    void ProcessFCFS()
    {
        if (rows.Count > 0)
        {
            RowData currentRow = rows[0];

            if (currentRow.IsAtStopHeight(stopHeight))
            {
                StartCoroutine(VanishRow(currentRow));
                rows.RemoveAt(0);
            }
            else
            {
                foreach (var square in currentRow.squares)
                {
                    float newYPosition = square.transform.position.y + (moveSpeed * Time.deltaTime);
                    if (newYPosition > stopHeight) newYPosition = stopHeight;
                    square.transform.position = new Vector3(square.transform.position.x, newYPosition, 0);
                }
            }
        }
    }

    void ProcessSRTF()
    {
        if (rows.Count > 0)
        {
            RowData shortestRow = null;
            float shortestRemainingTime = Mathf.Infinity;

            foreach (var row in rows)
            {
                float remainingTime = row.GetRemainingTime(stopHeight, moveSpeed);
                if (remainingTime < shortestRemainingTime)
                {
                    shortestRemainingTime = remainingTime;
                    shortestRow = row;
                }
            }

            if (shortestRow != null)
            {
                foreach (var square in shortestRow.squares)
                {
                    float newYPosition = square.transform.position.y + (moveSpeed * Time.deltaTime);
                    if (newYPosition > stopHeight) newYPosition = stopHeight;
                    square.transform.position = new Vector3(square.transform.position.x, newYPosition, 0);
                }

                if (shortestRow.IsAtStopHeight(stopHeight))
                {
                    StartCoroutine(VanishRow(shortestRow));
                    rows.Remove(shortestRow);
                }
            }
        }
    }

    IEnumerator VanishRow(RowData row)
    {
        float vanishDelay = 1.5f;
        row.turnaroundTime = Time.time - row.spawnTime;

        foreach (GameObject square in row.squares)
        {
            yield return new WaitForSeconds(vanishDelay);
            Destroy(square);
        }

        Debug.Log("Row Waiting Time: " + row.GetWaitingTime() + ", Turnaround Time: " + row.turnaroundTime);

        if (rows.Count == 0) CalculateAverageTimes();
    }

    void CalculateAverageTimes()
    {
        float totalWaitingTime = 0f;
        float totalTurnaroundTime = 0f;

        foreach (RowData row in rows)
        {
            totalWaitingTime += row.GetWaitingTime();
            totalTurnaroundTime += row.turnaroundTime;
        }

        float avgWaitingTime = totalWaitingTime / rows.Count;
        float avgTurnaroundTime = totalTurnaroundTime / rows.Count;

        Debug.Log("Average Waiting Time: " + avgWaitingTime);
        Debug.Log("Average Turnaround Time: " + avgTurnaroundTime);
    }
}

public class RowData
{
    public List<GameObject> squares = new List<GameObject>();
    public float spawnTime;
    public float turnaroundTime;

    public bool IsAtStopHeight(float stopHeight)
    {
        return squares.TrueForAll(square => square.transform.position.y >= stopHeight);
    }

    public float GetRemainingTime(float stopHeight, float moveSpeed)
    {
        float maxDistance = Mathf.Max(squares.ConvertAll(square => stopHeight - square.transform.position.y).ToArray());
        return maxDistance / moveSpeed;
    }

    public float GetWaitingTime()
    {
        return turnaroundTime - (squares.Count * 1.5f);
    }
}
