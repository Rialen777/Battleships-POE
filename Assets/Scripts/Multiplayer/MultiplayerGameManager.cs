using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class MultiplayerGameManager : MonoBehaviour
{
    // Game Board Setup
    [Header("Board")]
    public GameObject tilePrefab;
    public int boardSize = 10; // 10x10 board
    public Transform boardParent; // Transform to parent the tiles to
    private GameObject[,] boardTiles; // 2D array to store tile gameObjects
    private TileScript[,] tileScripts; // 2D array to store tile scripts

    // Ship Prefabs
    [Header("Ships")]
    public GameObject cosmicCarrierPrefab;
    public GameObject starBattleshipPrefab;
    public GameObject nebulaCruiserPrefab;
    public GameObject solarSubmarinePrefab;
    public GameObject cometDestroyerPrefab;

    // Ship Data
    [Header("data")]
    private List<ShipData> player1Ships;
    private List<ShipData> player2Ships;

    // UI Elements
    [Header("UI Elements")]
    public TextMeshProUGUI playerTurnText;
    public Button readyButton;
    public Button nextShipButton;
    public Button rotateButton;
    public TextMeshProUGUI shipPlacementText;
    public TextMeshProUGUI gameOverText;

    // Game State
    [Header("Game State")]
    private bool player1Turn = true; // Track whose turn it is
    private bool setupComplete = false; // Flag to indicate if the setup phase is complete
    private bool player1Ready = false; // Flag for Player 1's readiness
    private bool player2Ready = false; // Flag for Player 2's readiness
    private int currentShipIndex = 0; // Index for the current ship being placed
    private bool shipRotation = false; // Flag for ship rotation

    // Missile Prefabs
    [Header("Missles")]
    public GameObject missilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize UI elements
        readyButton.onClick.AddListener(() => ReadyButtonClicked());
        nextShipButton.onClick.AddListener(() => NextShipClicked());
        rotateButton.onClick.AddListener(() => RotateShip());

        // Initialize ship data
        player1Ships = new List<ShipData>()
        {
            new ShipData(cosmicCarrierPrefab, 5),
            new ShipData(starBattleshipPrefab, 4),
            new ShipData(nebulaCruiserPrefab, 3),
            new ShipData(solarSubmarinePrefab, 3),
            new ShipData(cometDestroyerPrefab, 2)
        };
        player2Ships = new List<ShipData>()
        {
            new ShipData(cosmicCarrierPrefab, 5),
            new ShipData(starBattleshipPrefab, 4),
            new ShipData(nebulaCruiserPrefab, 3),
            new ShipData(solarSubmarinePrefab, 3),
            new ShipData(cometDestroyerPrefab, 2)
        };

        // Create game board
        CreateBoard();
    }

    // Create the game board
    private void CreateBoard()
    {
        boardTiles = new GameObject[boardSize, boardSize];
        tileScripts = new TileScript[boardSize, boardSize];

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(col, 0, row), Quaternion.identity, boardParent);
                boardTiles[row, col] = tile;
                tileScripts[row, col] = tile.GetComponent<TileScript>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (setupComplete && player1Turn)
        {
            // Handle player clicks during gameplay
            HandlePlayerClicks();
        }
        else if (!setupComplete)
        {
            // Handle ship placement
            HandleShipPlacement();
        }
    }

    // Handle player clicks during gameplay
    private void HandlePlayerClicks()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Check if clicked on a tile
                if (hit.collider.gameObject.CompareTag("Tile"))
                {
                    // Get the tile coordinates
                    int row = (int)hit.collider.gameObject.transform.position.z;
                    int col = (int)hit.collider.gameObject.transform.position.x;

                    // Handle the click
                    HandleTileClick(row, col);
                }
            }
        }
    }

    // Handle ship placement
    private void HandleShipPlacement()
    {
        // If not in setup phase, do nothing
        if (setupComplete) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Check if clicked on a tile
                if (hit.collider.gameObject.CompareTag("Tile"))
                {
                    // Get the tile coordinates
                    int row = (int)hit.collider.gameObject.transform.position.z;
                    int col = (int)hit.collider.gameObject.transform.position.x;

                    // Place the ship
                    PlaceShip(row, col);
                }
            }
        }
    }

    // Handle tile clicks during gameplay
    private void HandleTileClick(int row, int col)
    {
        // Check if tile has already been attacked
        if (tileScripts[row, col].IsAttacked) return;

        // Attack the tile
        AttackTile(row, col);

        // Check for ship sink
        CheckShipSink(row, col);

        // Switch to the next player's turn
        SwitchPlayerTurn();
    }

    // Attack a tile
    private void AttackTile(int row, int col)
    {
        // Instantiate a missile at the tile's position
        Instantiate(missilePrefab, boardTiles[row, col].transform.position + new Vector3(0, 1, 0), Quaternion.identity);

        // Mark the tile as attacked
        tileScripts[row, col].Attacked();

        // Check if the tile hit a ship
        if (CheckHit(row, col))
        {
            playerTurnText.text = "HIT!";
        }
        else
        {
            playerTurnText.text = "MISS!";
        }
    }

    // Check if a tile hit a ship
    private bool CheckHit(int row, int col)
    {
        if (player1Turn)
        {
            // Check if the tile is part of Player 2's ships
            foreach (ShipData ship in player2Ships)
            {
                if (ship.IsHit(row, col))
                {
                    ship.HitTile(row, col);
                    return true;
                }
            }
        }
        else
        {
            // Check if the tile is part of Player 1's ships
            foreach (ShipData ship in player1Ships)
            {
                if (ship.IsHit(row, col))
                {
                    ship.HitTile(row, col);
                    return true;
                }
            }
        }

        return false;
    }

    // Check if a ship has been sunk
    private void CheckShipSink(int row, int col)
    {
        if (player1Turn)
        {
            // Check Player 2's ships
            foreach (ShipData ship in player2Ships)
            {
                if (ship.IsSunk)
                {
                    // Sink the ship visually (e.g., change color)
                    ship.Sink(tileScripts);

                    // Add an extra turn to the player if they sunk a ship
                    playerTurnText.text = "You sunk a ship!  Take another turn.";
                    player1Turn = true;
                }
            }
        }
        else
        {
            // Check Player 1's ships
            foreach (ShipData ship in player1Ships)
            {
                if (ship.IsSunk)
                {
                    // Sink the ship visually
                    ship.Sink(tileScripts);

                    // Add an extra turn to the player if they sunk a ship
                    playerTurnText.text = "You sunk a ship!  Take another turn.";
                    player1Turn = true;
                }
            }
        }

        // Check for game over
        CheckGameOver();
    }

    // Check for game over
    private void CheckGameOver()
    {
        if (player1Ships.All(ship => ship.IsSunk) || player2Ships.All(ship => ship.IsSunk)) 
        {
            setupComplete = true;
            player1Turn = false;

            // Determine the winner
            if (player1Ships.All(ship => ship.IsSunk))
            {
                gameOverText.text = "Player 2 WINS!";
            }
            else
            {
                gameOverText.text = "Player 1 WINS!";
            }

            // Show the game over text
            gameOverText.gameObject.SetActive(true);
        }
    }

    // Place a ship
    private void PlaceShip(int row, int col)
    {
        if (currentShipIndex >= player1Ships.Count) return;

        // Check if the ship can be placed at the selected location
        if (CanPlaceShip(row, col, player1Ships[currentShipIndex].Size, shipRotation))
        {
            // Place the ship
            player1Ships[currentShipIndex].Place(row, col, shipRotation, boardTiles, tileScripts);

            // Move to the next ship
            NextShipClicked();
        }
        else
        {
            // Display an error message
            shipPlacementText.text = "Cannot place ship here. Try again.";
        }
    }

    // Check if a ship can be placed at a given location
    private bool CanPlaceShip(int row, int col, int shipSize, bool isHorizontal)
    {
        // Check if the ship would extend beyond the board bounds
        if (isHorizontal)
        {
            if (col + shipSize > boardSize || col < 0) return false;
        }
        else
        {
            if (row + shipSize > boardSize || row < 0) return false;
        }

        // Check if the ship would overlap with any existing ships
        for (int i = 0; i < shipSize; i++)
        {
            int checkRow = row;
            int checkCol = col;

            if (isHorizontal)
            {
                checkCol += i;
            }
            else
            {
                checkRow += i;
            }

            // Check if the tile is already occupied
            if (tileScripts[checkRow, checkCol].IsOccupied)
            {
                return false;
            }
        }

        return true;
    }

    // Switch to the next player's turn
    private void SwitchPlayerTurn()
    {
        player1Turn = !player1Turn;

        if (player1Turn)
        {
            playerTurnText.text = "Player 1's turn. Select a tile.";
        }
        else
        {
            playerTurnText.text = "Player 2's turn. Select a tile.";
        }
    }

    // Move to the next ship for placement
    private void NextShipClicked()
    {
        // Move to the next ship
        currentShipIndex++;

        // Check if all ships have been placed
        if (currentShipIndex < player1Ships.Count)
        {
            // Update the ship placement text
            shipPlacementText.text = "Place the " + player1Ships[currentShipIndex].Name + " (" + player1Ships[currentShipIndex].Size + " tiles)";
        }
        else
        {
            // Disable the next ship button and enable the ready button
            nextShipButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
            playerTurnText.text = "Player 1, ready up!";
        }
    }

    // Rotate the ship
    private void RotateShip()
    {
        shipRotation = !shipRotation;
    }

    // Handle the "Ready" button click
    private void ReadyButtonClicked()
    {
        if (player1Turn)
        {
            player1Ready = true;
            playerTurnText.text = "Player 2, ready up!";
        }
        else
        {
            player2Ready = true;
            playerTurnText.text = "Game starting!";

            // Start the game when both players are ready
            if (player1Ready && player2Ready)
            {
                setupComplete = true;
                readyButton.gameObject.SetActive(false);
                nextShipButton.gameObject.SetActive(false);
                rotateButton.gameObject.SetActive(false);
                shipPlacementText.gameObject.SetActive(false);
                playerTurnText.text = "Player 1's turn. Select a tile.";
            }
        }
    }

    // Data class for ships
    private class ShipData
    {
        public GameObject Prefab { get; private set; }
        public int Size { get; private set; }
        public string Name { get; private set; }
        public bool CheckIfSunk { get; private set; }
        private List<int[]> Tiles { get; set; }
        public bool IsSunk { get; private set; } = false;

        public ShipData(GameObject prefab, int size)
        {
            Prefab = prefab;
            Size = size;
            CheckIfSunk = false;
            Tiles = new List<int[]>();
        }

        // Place the ship on the board
        public void Place(int row, int col, bool isHorizontal, GameObject[,] boardTiles, TileScript[,] tileScripts)
        {
            for (int i = 0; i < Size; i++)
            {
                int checkRow = row;
                int checkCol = col;

                if (isHorizontal)
                {
                    checkCol += i;
                }
                else
                {
                    checkRow += i;
                }

                // Place the ship on the board
                GameObject shipTile = Instantiate(Prefab, boardTiles[checkRow, checkCol].transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                Tiles.Add(new int[] { checkRow, checkCol });

                // Mark the tile as occupied
                tileScripts[checkRow, checkCol].Occupied();
            }
        }

        // Check if a tile belongs to the ship
        public bool IsHit(int row, int col)
        {
            return Tiles.Any(tile => tile[0] == row && tile[1] == col);
        }

        // Mark a tile as hit
        public void HitTile(int row, int col)
        {
            Tiles.RemoveAll(tile => tile[0] == row && tile[1] == col);
        }

        // Check if the ship is sunk
       //public bool IsSunk()
       // {
            
       //     return Tiles.Count == 0;
       // }

        // Sink the ship
        public void Sink(TileScript[,] tileScripts)
        {
            IsSunk = true;

            // Change the color of the ship tiles to indicate they are sunk (example)
            foreach (int[] tile in Tiles)
            {
                tileScripts[tile[0], tile[1]].Sink();
            }
        }

        public string GetName()
        {
            return Prefab.name.Replace("(Clone)", "");
        }
    }

}

// Tile script
