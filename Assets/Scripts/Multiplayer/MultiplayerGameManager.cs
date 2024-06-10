using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MultiplayerGameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    // Removed: public EnemyScript enemyScript;
    private ShipScript shipScript;
    // Removed: private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<TileScript> allTileScripts;    

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI playerShipText;
    public TextMeshProUGUI enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    // Removed: public GameObject enemyMissilePrefab;
    public GameObject firePrefab;
    public GameObject woodDock;

    private bool setupComplete = false;
    private bool playerTurn = true; // Player 1 starts
    private bool player1SetupComplete = false;
    private bool player2SetupComplete = false;

    private List<GameObject> playerFires = new List<GameObject>();
    // Removed: private List<GameObject> enemyFires = new List<GameObject>();

    // Removed: private int enemyShipCount = 5;
    private int playerShipCount = 5;

    // Start is called before the first frame update
    void Start()
    {
        if (ships.Length == 0)
        {
            UnityEngine.Debug.LogError("No ships assigned in the inspector!");
            return;
        }

        shipScript = ships[shipIndex]?.GetComponent<ShipScript>();
        if (shipScript == null)
        {
            UnityEngine.Debug.LogError($"Ship at index {shipIndex} does not have a ShipScript component!");
            return;
        }

        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateClicked());
        replayBtn.onClick.AddListener(() => ReplayClicked());
        // Removed: enemyShips = enemyScript.PlaceEnemyShips();
    }

    private void NextShipClicked()
    {
        Debug.Log("OnGameBoard() returned: " + shipScript.OnGameBoard()); 
        if (shipScript == null)
        {
            UnityEngine.Debug.LogError("shipScript is null!");
            return;
        }

        if (!shipScript.OnGameBoard())
        {
            shipScript.FlashColor(Color.red);
        }
        else
        {
            if (shipIndex < ships.Length - 1)
            {
                shipIndex++;
                if (ships[shipIndex] == null)
                {
                    UnityEngine.Debug.LogError($"Ship at index {shipIndex} is null!");
                    return;
                }

                shipScript = ships[shipIndex].GetComponent<ShipScript>();
                if (shipScript == null)
                {
                    UnityEngine.Debug.LogError($"Ship at index {shipIndex} does not have a ShipScript component!");
                    return;
                }

                shipScript.FlashColor(Color.yellow);
            }
            else
            {
                if (playerTurn) 
                {
                    nextBtn.gameObject.SetActive(true);
                    rotateBtn.gameObject.SetActive(true);
                    woodDock.SetActive(true);
                    player1SetupComplete = true;
                    topText.text = "Player 2, place your ships.";
                    shipIndex = 0; // Reset ship index for Player 2
                    shipScript = ships[shipIndex].GetComponent<ShipScript>();
                    
                }
                else 
                {
                    nextBtn.gameObject.SetActive(false);
                    rotateBtn.gameObject.SetActive(false);
                    woodDock.gameObject.SetActive(false);
                    player2SetupComplete = true;
                    topText.text = "Player 1, start guessing.";
                    setupComplete = true;
                    for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
                    
                }
            }
        }
    }

    public void TileClicked(GameObject tile)
    {
        if (setupComplete && playerTurn)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 15;
            playerTurn = false;
            Instantiate(missilePrefab, tilePos, missilePrefab.transform.rotation);
        }
        else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex]?.GetComponent<ShipScript>();
        if (shipScript == null)
        {
            UnityEngine.Debug.LogError($"Ship at index {shipIndex} does not have a ShipScript component!");
            return;
        }

        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    void RotateClicked()
    {
        shipScript.RotateShip();
    }

    public void CheckHit(GameObject tile)
    {
        int tileNum = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;
        TileScript tileScript = tile.GetComponent<TileScript>();
        foreach (TileScript targettileScript in allTileScripts)
        {
            if (tileScript.tileNumArrays.Contains(tileNum))
            {
                for (int i = 0; i < tileScript.tileNumArrays.Length; i++)
                {
                    if (tileScript.tileNumArrays[i] == tileNum)
                    {
                        tileScript.tileNumArrays[i] = -5;
                        hitCount++;
                    }
                    else if (tileScript.tileNumArrays[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileScript.tileNumArrays.Length)
                {
                    playerShipCount--; // Assuming the player being attacked is always losing a ship
                    if (playerTurn) 
                    {
                        enemyShipText.text = playerShipCount.ToString();
                        topText.text = "Player 2 sunk a ship!  Player 1, take another turn.";
                        playerTurn = true;
                    }
                    else 
                    {
                        playerShipText.text = playerShipCount.ToString();
                        topText.text = "Player 1 sunk a ship!  Player 2, take another turn.";
                        playerTurn = false;
                    }
                    playerFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tileScript.SetTileColor(1, new Color32(68, 0, 0, 255));
                    tileScript.SwitchColors(1);
                }
                else
                {
                    if (playerTurn) 
                    {
                        enemyShipText.text = playerShipCount.ToString();
                        topText.text = "Player 2 hit! Player 1, your turn.";
                    }
                    else
                    {
                        playerShipText.text = playerShipCount.ToString();
                        topText.text = "Player 1 hit! Player 2, your turn.";
                    }
                    tileScript.SetTileColor(1, new Color32(255, 0, 0, 255));
                    tileScript.SwitchColors(1);
                }
                break;
            }

        }
        if(hitCount == 0)
        {
            if (playerTurn) 
            {
                enemyShipText.text = playerShipCount.ToString();
                topText.text = "Player 2 missed! Player 1, your turn.";
            }
            else 
            {
                playerShipText.text = playerShipCount.ToString();
                topText.text = "Player 1 missed! Player 2, your turn.";
            }
            tileScript.SetTileColor(1, new Color32(38, 57, 76, 255));
            tileScript.SwitchColors(1);
        }
        // Invoke("EndPlayerTurn", 1.0f); // Removed - No need for separate turn ending functions
    }

    // Removed: public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    // Removed: private void EndPlayerTurn()
    // Removed: public void EndEnemyTurn()
    // Removed: private void ColorAllTiles(int colorIndex)

    void GameOver(string winner)
    {
        topText.text = "Game Over: " + winner;
        replayBtn.gameObject.SetActive(true);
        playerTurn = false;
    }

    void ReplayClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}

// Tile script
