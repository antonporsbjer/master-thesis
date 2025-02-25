using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaitingArea : MonoBehaviour
{
    // Grid based on number of rows and columns
    public int rows = 3;
    public int columns = 5;
    public bool debug = false;
    public CustomNode goal;

    public List<Vector3> waitingSpots;
    public List<bool> isOccupied;

    private int mapIndex;

    void OnEnable()
    {
        GenerateRowColumnWaitingSpots();
    }

    void GenerateRowColumnWaitingSpots()
    {
        waitingSpots = new List<Vector3>();
        isOccupied = new List<bool>(new bool[columns*rows]);

        Renderer renderer = transform.Find("Area").GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        Vector3 corner = new Vector3(bounds.min.x, transform.position.y, bounds.min.z);
        Vector3 size = bounds.size;

        float cellWidth = size.x / columns;
        float cellHeight = size.z / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = corner.x + cellWidth * 0.5f + col*cellWidth;
                float zPos = corner.z + cellHeight * 0.5f + row*cellHeight;
                Vector3 spotPosition = new Vector3(xPos, transform.position.y, zPos);
                waitingSpots.Add(spotPosition);

                isOccupied[col + row * columns] = false;

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

    public (int index, Vector3? position) getWaitingSpot()
    {
        // If there are available spots
        if(!isOccupied.All(spot => spot == true))
        {
            for(int i = 0; i < waitingSpots.Count; i++)
            {
                if(!isOccupied[i])
                {
                    isOccupied[i] = true;
                    return (mapIndex, waitingSpots[i]);
                }
            }
        }

        // No available spots
        return (-1, null);
    }

}
