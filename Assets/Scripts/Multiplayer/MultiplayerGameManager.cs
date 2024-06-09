using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MultiplayerGameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScript enemyScript;
    private ShipScript shipScript;

    //public Tile tileScript;
    private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<Tile> allTileScripts;    

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public TextMeshPro topText;
    public TextMeshPro playerShipText;
    public TextMeshPro enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject firePrefab;
    public GameObject woodDock;

    private bool setupComplete = false;
    private bool playerTurn = true;
    private bool player1Turn = true; // Track whose turn it is (Player 1 or Player 2)

    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();

    private int enemyShipCount = 5;
    private int playerShipCount = 5;

    // Start is called before the first frame update
    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateClicked());
        replayBtn.onClick.AddListener(() => ReplayClicked());
        // Initialize enemy ships for Player 2
        enemyShips = enemyScript.PlaceEnemyShips();
    }

    private void NextShipClicked()
    {
        if (!shipScript.OnGameBoard())
        {
            shipScript.FlashColor(Color.red);
        } else
        {
            if(shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScript>();
                shipScript.FlashColor(Color.yellow);
            }
            else
            {
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.SetActive(false);
                topText.text = "Player 1's turn. Guess an enemy tile.";
                setupComplete = true;
                for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
            }
        }

    }

    public void TileClicked(GameObject tile)
    {
        if(setupComplete && playerTurn)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 15;
            playerTurn = false;
            // Use the correct missile prefab based on whose turn it is
            Instantiate((player1Turn) ? missilePrefab : enemyMissilePrefab, tilePos, missilePrefab.transform.rotation);
        } else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
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
        foreach (int[] tileNumArray in enemyShips)
        {
            if (Array.Exists(tileNumArray, element => element == tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    topText.text = "SUNK!!!!!!";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<Tile>().SetTileColor(1, new Color32(68, 0, 0, 255));
                    tile.GetComponent<Tile>().SwitchColors(1);
                }
                else
                {
                    topText.text = "HIT!!";
                    tile.GetComponent<Tile>().SetTileColor(1, new Color32(255, 0, 0, 255));
                    tile.GetComponent<Tile>().SwitchColors(1);
                }
                break;
            }
        }
        if (hitCount == 0)
        {
            tile.GetComponent<Tile>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<Tile>().SwitchColors(1);
            topText.text = "Missed, there is no ship there.";
        }
        Invoke("EndPlayerTurn", 1.0f);
    }


    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        enemyScript.MissileHit(tileNum);
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));
        if (hitObj.GetComponent<ShipScript>().HitCheckSank())
        {
            playerShipCount--;
            playerShipText.text = playerShipCount.ToString();
            enemyScript.SunkPlayer();
        }
       Invoke("EndEnemyTurn", 2.0f);
    }

    private void EndPlayerTurn()
    {
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(true);
        foreach (GameObject fire in playerFires) fire.SetActive(true);
        foreach (GameObject fire in enemyFires) fire.SetActive(false);
        enemyShipText.text = enemyShipCount.ToString();

        // Switch to the other player's turn
        player1Turn = !player1Turn;

        if (player1Turn)
        {
            topText.text = "Player 1's turn. Select a tile.";
        }
        else
        {
            topText.text = "Player 2's turn. Select a tile.";
        }
        
        playerTurn = true;
        ColorAllTiles(1);
        if (enemyShipCount < 1) GameOver("Player 1 WINS!!");
        if (playerShipCount < 1) GameOver("Player 2 WINS!!!");
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
        foreach (GameObject fire in playerFires) fire.SetActive(false);
        foreach (GameObject fire in enemyFires) fire.SetActive(true);
        playerShipText.text = playerShipCount.ToString();
        topText.text = "Player 2's turn. Select a tile.";
        playerTurn = true;
        ColorAllTiles(1);
        if (enemyShipCount < 1) GameOver("Player 1 WINS!!");
        if (playerShipCount < 1) GameOver("Player 2 WINS!!!");
    }

    private void ColorAllTiles(int colorIndex)
    {
        foreach (Tile tileScript in allTileScripts)
        {
            tileScript.SwitchColors(colorIndex);
        }
    }

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
