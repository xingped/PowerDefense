using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
	public Vector2 gridPosition = Vector2.zero;
	public int attackRange = 1;
	
	public int HP = 25;
	
	public float attackChance = 0.75f;
	public float defenseReduction = 0.15f;
	public int damageBase = 5;
	public float damageRollSides = 6; //d6

	float closestDist;
	Enemy target = null;

	public bool firing = false;

	GameObject laser;
	public GameObject LaserPrefab;

	float moveSpeed = 5.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(HP < 0)
		{
			GameManager.instance.towers.Remove(this);
			Destroy(this);
		}
	}

	public void TurnUpdate()
	{
		//Debug.Log("Tower turn");
		if(!firing)
		{
			closestDist = attackRange;
			foreach(Enemy e in GameManager.instance.enemies)
			{
				float dist = Mathf.Sqrt(Mathf.Pow(e.gridPosition.x - this.gridPosition.x, 2) + Mathf.Pow(e.gridPosition.y - this.gridPosition.y, 2));
				
				if (dist <= (float)attackRange && dist <= closestDist)
				{
					target = e;
				}
			}
			
			if(target != null)
			{
				laser = (GameObject)Instantiate(LaserPrefab, this.transform.position, Quaternion.identity);

				firing = true;
			}
			else
			{
				GameManager.instance.nextTurn();
			}
		}
		else
		{
			if (Vector3.Distance(target.transform.position, transform.position) > 0.1f)
			{
				laser.transform.position += (target.transform.position - laser.transform.position).normalized * moveSpeed * Time.deltaTime;

				if (Vector3.Distance(target.transform.position, laser.transform.position) <= 0.1f)
				{
					laser.transform.position = target.transform.position;
					Destroy(laser);

					// Apply Damage
					bool hit = Random.Range(0.0f, 1.0f) <= this.attackChance;
					
					if(hit)
					{
						// damage logic
						int amountOfDamage = (int)Mathf.Floor(this.damageBase + Random.Range(0, this.damageRollSides));
						
						target.HP -= amountOfDamage;
						
						Debug.Log("Tower " + GameManager.instance.currentTowerIndex + " successfully hit " + target.playerName + " for " + amountOfDamage + " damage!");
					}
					else
					{
						Debug.Log("Tower " + GameManager.instance.currentTowerIndex + " missed " + target.playerName);
					}
					
					target = null;
					firing = false;
					GameManager.instance.nextTurn();
				}
			}
		}
	}
}