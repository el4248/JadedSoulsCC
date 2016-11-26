using UnityEngine;
using System.Collections.Generic;

public class TextBoard : MonoBehaviour {

	public GameText listing;
	private GameText temp;
	public int count = 0;
	private Vector3 test;
	// Use this for initialization
	void Start () {
		test = listing.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		//if(count < 10){
			//Print ("I print stuff");
		//}
	}

	public void print(string message){
		temp = (GameText)Instantiate(listing);
		temp.transform.SetParent(gameObject.transform);
		temp.transform.localScale = test;
		temp.source = this;
		temp.setMessage(message);
		temp.gameObject.SetActive(true);
		count++;
	}

	public void decrement(){
		--count;
	}
}
