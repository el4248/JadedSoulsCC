using UnityEngine;
using System.Collections;
/*A collection of information about a mobile object
 * Determines an objects "state"
 * */

public class Stats : MonoBehaviour {

	public const bool LETHAL = true;//lethal damage done to this character
	public const int RIGHT = 1;//if facing right
	public const int LEFT = -1;//if facing left
	public float health;//current hp
	public float max_health = 100;//max hp by default
	public int str = 50;//unused atm
	public float stunClk = 0;//stun, whatever that is
	protected int facing = 1;//which direction we are currently facing
	public Vector3 resLoc = Vector2.zero;//where char respawns if at all
	public bool destroyOnDeath = true;//do we remove this object on death
	[SerializeField][Range(1,5)]private int team;
	public TextBoard board;//in game message board
	public bool dead = false;

	// Use this for initialization
	protected virtual void Awake () {
		if(transform.localScale.x < 0)facing = -1; 
		if(resLoc == Vector3.zero)
			resLoc = transform.position;
		gameObject.tag = "Team" + team;
		//Debug.Log(this.gameObject.name + " Health: " + health + "/" + max_health);//Debug Log
	}

	protected virtual void Start () {
        board = FindObjectOfType<TextBoard>();
        health = max_health;//allows non-standard health
	}
	
	// Update is called once per frame
	protected virtual void Update (){
		if(health <= 0 && !dead){
			dead = true;
			dies();
		}
	}

	public void changeTeam(int team){
		this.team = team;
		gameObject.tag = "Team" + team;
	}
	
	//deal damage to this asset
	protected virtual bool takeDamage(float damage){
		//if(stunClk == 0){
			health -= damage;
			Debug.Log(this.gameObject.name + " Health: " + health + "/" + max_health);
			if(health <= 0)
				return true;
			else
				return false;
		//}
	}

	public virtual bool receive_damage(float damage){
		return takeDamage(damage);
	}

	public virtual void setFacing(int dir){//set direction of character
		this.facing = dir;
	}

	public int getFacing(){
		return this.facing;
	}

	protected void apply_stun(int frames){//don't allow movement for frames frames
		stunClk = frames;
	}

	protected virtual void dies(){
		//Debug.Log(this.gameObject.name + " has died");
		board.print (this.gameObject.name + " has died.");//print to in game menu
		StartCoroutine( onDeath());//prepare this object for "deletion"
	}

	public virtual IEnumerator onDeath(){//what this asset does on death
		if(destroyOnDeath)
			Destroy(this.gameObject);
		else 
			reset();
		StopCoroutine(onDeath());
		return null;
	}//what happens when this thing dies


	public virtual void reset(){
		//Debug.Log(this.gameObject.name + " has respawned");
		health = max_health;//allows non-standard health
		transform.position = resLoc;
		dead = false;
	}
}
