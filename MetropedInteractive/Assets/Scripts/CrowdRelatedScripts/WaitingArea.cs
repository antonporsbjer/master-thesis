using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingArea : MonoBehaviour
{
    // Grid based on number of rows and columns
    public int rows = 3;
    public int columns = 5;
    public bool debug = false;

    private Vector3[,] waitingSpots;
    private bool[,] isOccupied;

    private int mapIndex;

    void Start()
    {
        GenerateRowColumnWaitingSpots();
    }

    void GenerateRowColumnWaitingSpots()
    {
        waitingSpots = new Vector3[rows, columns];
        isOccupied = new bool[rows, columns];

        Renderer renderer = transform.Find("Area").GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        Vector3 corner = new Vector3(bounds.min.x, transform.position.y, bounds.min.z);
        Vector3 size = bounds.size;

        float cellWidth = size.x / columns;
        float cellHeight = size.z / rows;

        Debug.Log(cellWidth + " " + cellHeight);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = corner.x + cellWidth * 0.5f + col*cellWidth;
                float zPos = corner.z + cellHeight * 0.5f + row*cellHeight;
                Vector3 spotPosition = new Vector3(xPos, transform.position.y, zPos);
                waitingSpots[row,col] = spotPosition;

                isOccupied[row,col] = false;

                if(debug)
                {
                    Debug.DrawLine(spotPosition, spotPosition + Vector3.up * 0.5f, Color.red, 10f);
                }
                
            }
        }
    }

    public void setMapIndex(int index)
    {
        mapIndex = index;
    }
}
