using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnMobs : MonoBehaviour {
	private const int RIGHT = 1;
	private const int LEFT = -1;	

	public int cooldown = 0;//frame count
	public int time = 400;//frames until stuff is spawned
	public int count = 0;//number of mobs available
	public int limit = 0;//max mobs
	public bool on = true;
	public Mob[] spawnlist;//list of things that can be spawned
	public List<Mob> active;
	public List<Mob> pool;
	public Rigidbody2D body;
	public Vector3 spawn = Vector3.zero;
	public int direction = RIGHT;
	private Mob lastSpawn,buffer;
	[Range(1,5)]public int team = 1;
	private Vector3 temp;
	public Vector4 spriteColor = new Vector4(0,0,0,1);
    public bool printDeaths = false;//output message on death
	[SerializeField]public GameObject dispatch;

	// Use this for initialization
	void Awake(){
			pool = new List<Mob>();
			active = new List<Mob>();
	}

	public void setColor(float v, float g, float b, float a){
		spriteColor = new Vector4(v,g,b,a);
	}

	void Start () {
		temp = spawnlist[0].gameObject.transform.lossyScale;
		spawn = transform.position;
	for(count = 0; count < limit; count++){
		lastSpawn = (Mob)Instantiate(spawnlist[0], spawn, Quaternion.identity);
		lastSpawn.name = spawnlist[0].name;
		lastSpawn.gameObject.SetActive(false);
		lastSpawn.transform.SetParent(dispatch.transform,false);
		pool.Add(lastSpawn);
		lastSpawn.changeTeam(team);
		//lastSpawn.setDirection(direction);//from mob
		//lastSpawn.setFacing(direction);//from stats
		//lastSpawn.transform.localScale = temp;
		//lastSpawn.transform.localScale = new Vector3(direction * lastSpawn.transform.localScale.x,lastSpawn.transform.localScale.y,0);
	}//Spawning Pools
	}
	
	// Update is called once per frame
	void Update () {
		if(cooldown > 0)
			--cooldown;
		else {
			if(count > 0 && on){
				lastSpawn = pool[0];
				lastSpawn.reset();
				active.Add(lastSpawn);//start activating mob
				lastSpawn.setDirection(direction);//from mob
				//lastSpawn.setFacing(direction);//from stats
				lastSpawn.gameObject.transform.lossyScale.Set(temp.x, temp.y, temp.z);
				lastSpawn.transform.position = spawn;
				lastSpawn.gameObject.SetActive(true);//turn mob on
				lastSpawn.source = this;
				lastSpawn.changeTeam(team);
				lastSpawn.GetComponent<SpriteRenderer>().color = spriteColor;
				pool.RemoveAt(0);//remove mob from other list
				--count;//number of spawns left
				//lastSpawn.id = count;
				//++count;
			}
		
			cooldown = time;
		}
	}

	public void despawn(int id){
		bool found = false;
		for(int i = 0; !found && i < limit - count;){
			if(!active[i].isActiveAndEnabled){
                buffer = active[i];
				pool.Add(buffer);
				buffer.gameObject.SetActive(false);
                if(printDeaths) active[i].board.print(active[i].gameObject.name + " has died.");
                active.RemoveAt(i);
				found = true;
				++count;
				//i = limit - count;//breakout
			}
			else
				++i;
		}
	}//move object back to pool
}
