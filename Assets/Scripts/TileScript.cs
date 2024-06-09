using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    private bool isOccupied;
    private bool isAttacked;
    private bool isSunk;

    public bool IsOccupied { get { return isOccupied; } }
    public bool IsAttacked { get { return isAttacked; } }
    public bool IsSunk { get { return isSunk; } }

    // Function to mark the tile as occupied
    public void Occupied()
    {
        isOccupied = true;
    }

    // Function to mark the tile as attacked
    public void Attacked()
    {
        isAttacked = true;
        // Change the color of the tile to indicate it has been attacked
        GetComponent<Renderer>().material.color = Color.red; 
    }

    // Function to mark the tile as sunk
    public void Sink()
    {
        isSunk = true;
        // Change the color of the tile to indicate it is sunk
        GetComponent<Renderer>().material.color = Color.gray; 
    }
}
