using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	
	public GameObject TilePrefab;
	public GameObject UserPlayerPrefab;
	public GameObject EnemyPrefab;
	public GameObject TowerPrefab;
	public GameObject CrystalPrefab;

	public Crystal CrystalObj = null;
	
	public int mapSize = 11;
	
	public List<List<Tile>> map = new List<List<Tile>> ();
	public List<Player> players = new List<Player>();
	public List<Tower> towers = new List<Tower>();
	public List<Enemy> enemies = new List<Enemy>();
	public int currentPlayerIndex = 0;
	public int currentTowerIndex = 0;
	public int currentEnemyIndex = 0;

	public int unitTurn = 0;
	int spawnCounter = 4;
	int spawnWave = 1;

	void Awake()
	{
		instance = this;
	}
	
	// Use this for initialization
	void Start ()
	{
		generateMap();
		generatePlayers();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(unitTurn == 0) // Players go first
		{
			if(players[currentPlayerIndex].HP > 0) players[currentPlayerIndex].TurnUpdate();
			else nextTurn();
		}
		else if (unitTurn == 1) // Towers second
		{
			if(towers.Count == 0)
				unitTurn++;
			else
				towers[currentTowerIndex].TurnUpdate();
		}
		else if (unitTurn == 2) // Enemies last
		{
			if(enemies.Count == 0)
				unitTurn++;
			else
			{
				if(enemies[currentEnemyIndex].HP > 0)
					enemies[currentEnemyIndex].TurnUpdate();
				else
					nextTurn();
			}
		}

	}
	
	void OnGUI()
	{
		players[currentPlayerIndex].TurnOnGUI();
	}
	
	public void nextTurn()
	{
		if(unitTurn == 0) // increment Player Index if it's the players' turn
		{
			if (currentPlayerIndex + 1 < players.Count)
			{
				currentPlayerIndex++;
			}
			else
			{
				//Debug.Log("Hit");
				currentPlayerIndex = 0;
				unitTurn++;
			}
		}
		else if(unitTurn == 1) // increment Tower Index if it's the towers' turn
		{
			if(currentTowerIndex + 1 < towers.Count)
			{
				currentTowerIndex++;
			}
			else
			{
				currentTowerIndex = 0;
				unitTurn++;
			}
		}
		else if(unitTurn == 2) // increment Enemy Index if it's the enemies' turn
		{
			if(currentEnemyIndex + 1 < enemies.Count)
			{
				currentEnemyIndex++;
			}
			else
			{
				currentEnemyIndex = 0;
				unitTurn = 0;
				spawnEnemies();
			}
		}

		//Debug.Log("Index: " + currentPlayerIndex);
		Debug.Log("UT: " + unitTurn + ", PI: " + currentPlayerIndex + ", TI: " + currentTowerIndex + ", EI: " + currentEnemyIndex);

	}
	
	public void highlightTilesAt(Vector2 originLocation, Color highlightColor, int distance)
	{
		List<Tile> highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance);
		
		foreach(Tile t in highlightedTiles)
		{
			t.transform.renderer.material.color = highlightColor;
		}
	}
	
	public void removeTileHighlights()
	{
		for (int i = 0; i < mapSize; i++)
		{
			for (int j = 0; j < mapSize; j++)
			{
				if (!map[i][j].impassible)
					map[i][j].transform.renderer.material.color = Color.white;
			}
		}
	}

	public void removeEnemies()
	{
		for(int i = 0; i < enemies.Count; i++)
		{
			if(enemies[i].HP <= 0)
			{
				Destroy(enemies[i]);
				enemies.RemoveAt(i);
			}
		}
	}

	public void spawnEnemies()
	{
		if(spawnCounter == 0)
		{
			Enemy newEnemy;

			for(int i = 0; i < spawnWave; i++)
			{
				int side = Random.Range(0, 3);
				int xPos = 0, yPos = 0;

				switch(side)
				{
					case 0:
						xPos = 0;
						yPos = Random.Range(0, mapSize-1);
						break;
					case 1:
						xPos = Random.Range(0, mapSize-1);
						yPos = 0;
						break;
					case 2:
						xPos = mapSize-1;
						yPos = Random.Range(0, mapSize-1);
						break;
					case 3:
						xPos = Random.Range(0, mapSize-1);
						yPos = mapSize-1;
						break;
				}

				newEnemy = ((GameObject)Instantiate(EnemyPrefab, new Vector3(map[xPos][yPos].transform.position.x, 1.5f, map[xPos][yPos].transform.position.z), Quaternion.Euler(new Vector3()))).GetComponent<Enemy>();
				newEnemy.gridPosition = new Vector2(xPos, yPos);
				enemies.Add(newEnemy);
			}

			spawnWave++;
			spawnCounter = 3;
		}
		else
		{
			spawnCounter--;
		}
	}
	
	public void moveCurrentPlayer(Tile destTile)
	{
		if(destTile.transform.renderer.material.color != Color.white)
		{
			removeTileHighlights();
			players[currentPlayerIndex].moving = false;
			foreach(Tile t in TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y],destTile)) {
				players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position + 1.5f * Vector3.up);
				//Debug.Log("(" + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].y + ")");
			}
			map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y].impassible = false;
			players[currentPlayerIndex].gridPosition = destTile.gridPosition;
			map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y].impassible = true;
		}
		else
		{
			Debug.Log("Destination invalid");
		}
	}
	
	public void attackWithCurrentPlayer(Tile destTile)
	{
		if(destTile.transform.renderer.material.color != Color.white && !destTile.impassible)
		{
			Player target = null;
			
			foreach (Player p in players)
			{
				if (p.gridPosition == destTile.gridPosition)
				{
					target = p;
				}
			}
			
			if (target != null)
			{
				//Debug.Log("p.x: " + players[currentPlayerIndex].gridPosition.x + ", p.y: " + players[currentPlayerIndex].gridPosition.y);
				//Debug.Log("t.x: " + target.gridPosition.x + ", t.y: " + target.gridPosition.y);
				
				// if adjacent
				if (players[currentPlayerIndex].gridPosition.x >= target.gridPosition.x - 1 && players[currentPlayerIndex].gridPosition.x <= target.gridPosition.x + 1 &&
				    players[currentPlayerIndex].gridPosition.y >= target.gridPosition.y - 1 && players[currentPlayerIndex].gridPosition.y <= target.gridPosition.y + 1)
				{
					players[currentPlayerIndex].actionPoints--;
					removeTileHighlights();
					players[currentPlayerIndex].moving = false;
					// attack logic
					// roll to hit
					bool hit = Random.Range(0.0f, 1.0f) <= players[currentPlayerIndex].attackChance;
					
					if(hit)
					{
						// damage logic
						int amountOfDamage = (int)Mathf.Floor(players[currentPlayerIndex].damageBase + Random.Range(0, players[currentPlayerIndex].damageRollSides));
						
						target.HP -= amountOfDamage;
						
						Debug.Log(players[currentPlayerIndex].playerName + " successfully hit " + target.playerName + " for " + amountOfDamage + " damage!");
					}
					else
					{
						Debug.Log(players[currentPlayerIndex].playerName + " missed " + target.playerName);
					}
				}
				//if not adjacent
				else
				{
					Debug.Log("Target is not adjacent!");
				}
			}
		}
	}
	
	public void buildWithCurrentPlayer(Tile destTile)
	{
		if(destTile.transform.renderer.material.color != Color.white && !destTile.impassible)
		{
			Tower newTower;

			newTower = ((GameObject)Instantiate(TowerPrefab, new Vector3(destTile.transform.position.x, 1, destTile.transform.position.z), Quaternion.Euler(new Vector3()))).GetComponent<Tower>();
			newTower.gridPosition = destTile.gridPosition;
			towers.Add(newTower);
			destTile.impassible = true;
			destTile.transform.renderer.material.color = Color.magenta;

			players[currentPlayerIndex].actionPoints--;
			
			removeTileHighlights();
		}
	}

	void generateMap ()
	{
		map = new List<List<Tile>>();
		for (int i = 0; i < mapSize; i ++)
		{
			List<Tile> row = new List<Tile>();
			
			for (int j = 0; j < mapSize; j++)
			{
				Tile tile = ((GameObject)Instantiate(TilePrefab, new Vector3(i - Mathf.Floor(mapSize/2), 0, -j + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
				tile.gridPosition = new Vector2(i, j);
				row.Add(tile);
			}
			
			map.Add (row);
		}
	}
	
	void generatePlayers()
	{
		UserPlayer player;
		
		player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSize/2), 1.5f, -0 + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
		player.gridPosition = new Vector2(0, 0);
		player.playerName = "Bob";
		map[(int)player.gridPosition.x][(int)player.gridPosition.y].impassible = true;
		
		players.Add(player);
		
		player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3((mapSize-1) - Mathf.Floor(mapSize/2), 1.5f, -(mapSize-1) + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
		player.gridPosition = new Vector2(mapSize - 1, mapSize - 1);
		player.playerName = "Kyle";
		map[(int)player.gridPosition.x][(int)player.gridPosition.y].impassible = true;
		
		players.Add(player);
		
		player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3(4 - Mathf.Floor(mapSize/2), 1.5f, -4 + Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
		player.gridPosition = new Vector2(4, 4);
		player.playerName = "Lars";
		map[(int)player.gridPosition.x][(int)player.gridPosition.y].impassible = true;
		
		players.Add(player);

		Enemy testDummy;

		testDummy = ((GameObject)Instantiate(EnemyPrefab, new Vector3(-2.0f, 1.5f, 3.0f), Quaternion.Euler(new Vector3()))).GetComponent<Enemy>();
		testDummy.gridPosition = new Vector2(3, 2);

		enemies.Add(testDummy);

		CrystalObj = ((GameObject)Instantiate(CrystalPrefab, new Vector3(3.0f, 1.5f, -3.0f), Quaternion.Euler(new Vector3()))).GetComponent<Crystal>();
		CrystalObj.gridPosition = new Vector2(8, 8);
		map[8][8].impassible = true;
		map[8][8].transform.renderer.material.color = Color.cyan;
	}
}
