using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Player {

	int stepCount = 2;

	Tower targetT = null;
	UserPlayer targetP = null;

	Tile moveTile = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (HP <= 0)
		{
			transform.renderer.material.color = Color.gray;
			transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
			GameManager.instance.map[(int)this.gridPosition.x][(int)this.gridPosition.y].impassible = false;
			//GameManager.instance.enemies.Remove(this);
			//Destroy(this);
		}
	}
	
	public override void TurnUpdate()
	{
		Debug.Log("Enemy Turn");

		// Find a location to move to
		if (!moving && !attacking)
		{
			moveTile = null;
			List<Tile> moveTileList = new List<Tile>();
			float dist = float.MaxValue;
			float tempDist;

			// Check if you can move to the Crystal first
			if(moveTile == null)
			{
				moveTileList.Add(GameManager.instance.map[(int)GameManager.instance.CrystalObj.gridPosition.x+1][(int)GameManager.instance.CrystalObj.gridPosition.y]);
				moveTileList.Add(GameManager.instance.map[(int)GameManager.instance.CrystalObj.gridPosition.x][(int)GameManager.instance.CrystalObj.gridPosition.y+1]);
				moveTileList.Add(GameManager.instance.map[(int)GameManager.instance.CrystalObj.gridPosition.x-1][(int)GameManager.instance.CrystalObj.gridPosition.y]);
				moveTileList.Add(GameManager.instance.map[(int)GameManager.instance.CrystalObj.gridPosition.x][(int)GameManager.instance.CrystalObj.gridPosition.y-1]);

				foreach(Tile mt in moveTileList)
				{
					if(!mt.impassible)
					{
						tempDist = Mathf.Sqrt(Mathf.Pow(mt.gridPosition.x - this.gridPosition.x, 2) + Mathf.Pow(mt.gridPosition.y - this.gridPosition.y, 2));
						if(tempDist <= dist)
						{
							moveTile = mt;
						}
					}
				}

				moveTileList.Clear();
			}

			// If there's no moveTile around the crystal, check for move spaces around Towers
			if(moveTile == null)
			{
				foreach(Tower to in GameManager.instance.towers)
				{
					moveTileList.Add(GameManager.instance.map[(int)to.gridPosition.x+1][(int)to.gridPosition.y]);
					moveTileList.Add(GameManager.instance.map[(int)to.gridPosition.x][(int)to.gridPosition.y+1]);
					moveTileList.Add(GameManager.instance.map[(int)to.gridPosition.x-1][(int)to.gridPosition.y]);
					moveTileList.Add(GameManager.instance.map[(int)to.gridPosition.x][(int)to.gridPosition.y-1]);
				}

				foreach(Tile mt in moveTileList)
				{
					if(!mt.impassible)
					{
						tempDist = Mathf.Sqrt(Mathf.Pow(mt.gridPosition.x - this.gridPosition.x, 2) + Mathf.Pow(mt.gridPosition.y - this.gridPosition.y, 2));
						if(tempDist <= dist)
						{
							moveTile = mt;
						}
					}
				}
				
				moveTileList.Clear();
			}

			// if there's no moveTile around a tower, check for spaces around players
			if(moveTile == null)
			{
				foreach(UserPlayer pl in GameManager.instance.players)
				{
					moveTileList.Add(GameManager.instance.map[(int)pl.gridPosition.x+1][(int)pl.gridPosition.y]);
					moveTileList.Add(GameManager.instance.map[(int)pl.gridPosition.x][(int)pl.gridPosition.y+1]);
					moveTileList.Add(GameManager.instance.map[(int)pl.gridPosition.x-1][(int)pl.gridPosition.y]);
					moveTileList.Add(GameManager.instance.map[(int)pl.gridPosition.x][(int)pl.gridPosition.y-1]);
				}

				foreach(Tile mt in moveTileList)
				{
					if(!mt.impassible)
					{
						tempDist = Mathf.Sqrt(Mathf.Pow(mt.gridPosition.x - this.gridPosition.x, 2) + Mathf.Pow(mt.gridPosition.y - this.gridPosition.y, 2));
						if(tempDist <= dist)
						{
							moveTile = mt;
						}
					}
				}
				
				moveTileList.Clear();
			}

			// if a valid moveTile was found, find a path to it if possible
			if(moveTile != null)
			{
				if(moveTile.gridPosition != this.gridPosition)
				{
					List<Tile> movePath = TilePathFinder.FindPath(GameManager.instance.map[(int)this.gridPosition.x][(int)this.gridPosition.y], 
					                                              GameManager.instance.map[(int)moveTile.gridPosition.x][(int)moveTile.gridPosition.y]);
					if(movePath != null)
					{
						foreach(Tile t in movePath)
						{
							this.positionQueue.Add(GameManager.instance.map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position + 1.5f * Vector3.up);
							//Debug.Log("(" + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].y + ")");
						}
						GameManager.instance.map[(int)this.gridPosition.x][(int)this.gridPosition.y].impassible = false;
						if(movePath.Count >= 2)
							this.gridPosition = movePath[1].gridPosition;
						else
							this.gridPosition = movePath[0].gridPosition;
						moving = true;
						GameManager.instance.map[(int)this.gridPosition.x][(int)this.gridPosition.y].impassible = true;
					}
				}
			}
		}

		// Move to the calculated position if possible
		if (positionQueue.Count > 0)
		{
			if (Vector3.Distance(positionQueue[0], transform.position) > 0.1f)
			{
				transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
				
				if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
				{
					transform.position = positionQueue[0];
					positionQueue.RemoveAt(0);

					stepCount--;

					if(stepCount == 0)
					{
						while(positionQueue.Count != 0)
							positionQueue.RemoveAt(0);

						stepCount = 2;

						moving = false;
						attacking = true;
					}
				}
			}
		}
		else
		{
			moving = false;
			attacking = true;
		}

		// attack anything nearby (in order: crystal, towers, players)
		if(attacking)
		{
			if((GameManager.instance.CrystalObj.gridPosition.x == this.gridPosition.x+1 && GameManager.instance.CrystalObj.gridPosition.y == this.gridPosition.y)
			   || (GameManager.instance.CrystalObj.gridPosition.x == this.gridPosition.x && GameManager.instance.CrystalObj.gridPosition.y == this.gridPosition.y+1)
			   || (GameManager.instance.CrystalObj.gridPosition.x == this.gridPosition.x-1 && GameManager.instance.CrystalObj.gridPosition.y == this.gridPosition.y)
			   || (GameManager.instance.CrystalObj.gridPosition.x == this.gridPosition.x && GameManager.instance.CrystalObj.gridPosition.y == this.gridPosition.y-1))
			{
				bool hit = Random.Range(0.0f, 1.0f) <= this.attackChance;
				
				if(hit)
				{
					// damage logic
					int amountOfDamage = (int)Mathf.Floor(this.damageBase + Random.Range(0, this.damageRollSides));
					
					GameManager.instance.CrystalObj.HP -= amountOfDamage;
					
					Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " successfully hit Crystal for " + amountOfDamage + " damage!");
				}
				else
				{
					Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " missed Crystal.");
				}
			}
			else
			{
				foreach(Tower t in GameManager.instance.towers)
				{
					if ((t.gridPosition.x == this.gridPosition.x+1 && t.gridPosition.y == this.gridPosition.y)
					    || (t.gridPosition.x == this.gridPosition.x && t.gridPosition.y == this.gridPosition.y+1)
					    || (t.gridPosition.x == this.gridPosition.x-1 && t.gridPosition.y == this.gridPosition.y)
					    || (t.gridPosition.x == this.gridPosition.x && t.gridPosition.y == this.gridPosition.y-1))
					{
						targetT = t;
					}
				}

				foreach(UserPlayer p in GameManager.instance.players)
				{
					if ((p.gridPosition.x == this.gridPosition.x+1 && p.gridPosition.y == this.gridPosition.y)
					    || (p.gridPosition.x == this.gridPosition.x && p.gridPosition.y == this.gridPosition.y+1)
					    || (p.gridPosition.x == this.gridPosition.x-1 && p.gridPosition.y == this.gridPosition.y)
					    || (p.gridPosition.x == this.gridPosition.x && p.gridPosition.y == this.gridPosition.y-1))
					{
						targetP = p;
					}
				}

				if(targetT != null)
				{
					bool hit = Random.Range(0.0f, 1.0f) <= this.attackChance;
					
					if(hit)
					{
						// damage logic
						int amountOfDamage = (int)Mathf.Floor(this.damageBase + Random.Range(0, this.damageRollSides));
						
						targetT.HP -= amountOfDamage;
						
						Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " successfully hit Tower for " + amountOfDamage + " damage!");
					}
					else
					{
						Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " missed Tower.");
					}
				}
				else if(targetP != null)
				{
					bool hit = Random.Range(0.0f, 1.0f) <= this.attackChance;
					
					if(hit)
					{
						// damage logic
						int amountOfDamage = (int)Mathf.Floor(this.damageBase + Random.Range(0, this.damageRollSides));
						
						targetP.HP -= amountOfDamage;
						
						Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " successfully hit Player " + targetP.name + " for " + amountOfDamage + " damage!");
					}
					else
					{
						Debug.Log("Enemy " + GameManager.instance.currentEnemyIndex + " missed Tower.");
					}
				}

				targetT = null;
				targetP = null;
			}

			attacking = false;
		}

		//base.TurnUpdate();
		if(!moving && !attacking)
			GameManager.instance.nextTurn();
	}
}
