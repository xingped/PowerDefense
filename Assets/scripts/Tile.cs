using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	
	public Vector2 gridPosition = Vector2.zero;
	
	public int movementCost = 1;
	public bool impassible = false;
	
	public List<Tile> neighbors = new List<Tile>();
	
	// Use this for initialization
	void Start () {
		generateNeighbors();
	}
	
	void generateNeighbors()
	{
		neighbors = new List<Tile>();
		
		// up
		if (gridPosition.y > 0)
		{
			Vector2 neighbor = new Vector2(gridPosition.x, gridPosition.y - 1);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(neighbor.x)][(int)Mathf.Round(neighbor.y)]);
		}
		// down
		if (gridPosition.y < GameManager.instance.map.Count - 1)
		{
			Vector2 neighbor = new Vector2(gridPosition.x, gridPosition.y + 1);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(neighbor.x)][(int)Mathf.Round(neighbor.y)]);
		}
		// left
		if (gridPosition.x > 0)
		{
			Vector2 neighbor = new Vector2(gridPosition.x - 1, gridPosition.y);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(neighbor.x)][(int)Mathf.Round(neighbor.y)]);
		}
		// right
		if (gridPosition.x < GameManager.instance.map.Count - 1)
		{
			Vector2 neighbor = new Vector2(gridPosition.x + 1, gridPosition.y);
			neighbors.Add(GameManager.instance.map[(int)Mathf.Round(neighbor.x)][(int)Mathf.Round(neighbor.y)]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnMouseEnter()
	{
		/*
		if(GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
		{
			transform.renderer.material.color = Color.blue;
		}
		if(GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
		{
			transform.renderer.material.color = Color.red;
		}
		*/
		//Debug.Log("my position is (" + gridPosition.x + "," + gridPosition.y + ")");
	}
	
	void OnMouseExit()
	{
		//transform.renderer.material.color = Color.white;
	}
	
	void OnMouseDown()
	{
		if(GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
		{
			GameManager.instance.moveCurrentPlayer(this);
		}
		else if(GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
		{
			GameManager.instance.attackWithCurrentPlayer(this);
		}
		else if(GameManager.instance.players[GameManager.instance.currentPlayerIndex].building)
		{
			GameManager.instance.buildWithCurrentPlayer(this);
		}
		else
		{
			transform.renderer.material.color = Color.white;
		}
		/*else
		{
			impassible = impassible ? false : true;

			if(impassible)
			{
				transform.renderer.material.color = new Color(0.5f, 0.5f, 0.0f);
			}
			else
			{
				transform.renderer.material.color = Color.white;
			}
		}*/
	}
}
