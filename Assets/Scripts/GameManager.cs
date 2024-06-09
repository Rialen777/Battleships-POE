using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScript enemyScript;
    private ShipScript shipScript;
    private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<Tile> allTileScripts;

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI playerShipText;
    public TextMeshProUGUI enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject firePrefab;
    public GameObject woodDock;

    private bool setupComplete = false;
    private bool playerTurn = true;

    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();

    private int enemyShipCount = 5;
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
        enemyShips = enemyScript.PlaceEnemyShips();
    }

    private void NextShipClicked()
    {
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
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.SetActive(false);
                topText.text = "Guess an enemy tile.";
                setupComplete = true;
                for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
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
        foreach (int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
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
        topText.text = "Enemy's turn";
        enemyScript.NPCTurn();
        ColorAllTiles(0);
        if (playerShipCount < 1) GameOver("ENEMY WINS!!!");
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++) ships[i].SetActive(false);
        foreach (GameObject fire in playerFires) fire.SetActive(false);
        foreach (GameObject fire in enemyFires) fire.SetActive(true);
        playerShipText.text = playerShipCount.ToString();
        topText.text = "Select a tile";
        playerTurn = true;
        ColorAllTiles(1);
        if (enemyShipCount < 1) GameOver("YOU WIN!!");
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
