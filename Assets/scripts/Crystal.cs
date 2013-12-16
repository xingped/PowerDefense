using UnityEngine;
using System.Collections;

public class Crystal : MonoBehaviour {

	public Vector2 gridPosition;
	public int HP = 100;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (HP <= 0)
		{
			Application.LoadLevel("GameOver");
		}
	}
}
