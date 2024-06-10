using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public MultiplayerGameManager gameManager;
    public int[] tileNumArrays; // Array to store the tile numbers for the ship
    private bool isHit = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("MultiplayerGameManager").GetComponent<MultiplayerGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle mouse clicks on the tile
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        if (Physics.Raycast(ray, out RaycastHit hit)) // Use RaycastHit here
        {
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == gameObject.name)
            {
                if (!isHit) 
                {
                    gameManager.TileClicked(gameObject);
                    isHit = true;
                }
            }
        }
    }

    // Function to set the tile's color
    public void SetTileColor(int index, Color32 color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    // Function to switch between colors
    public void SwitchColors(int colorIndex)
    {
        
        if (colorIndex == 0) 
        {
            GetComponent<Renderer>().material.color = new Color32(38, 57, 76, 255); // Example color
        } 
        else if (colorIndex == 1) 
        {
            GetComponent<Renderer>().material.color = Color.red; // Example color
        }
    }
}
  
