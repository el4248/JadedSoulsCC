using UnityEngine;
using System.Collections;

public class Mob : Generic2DCharacterV2 {//Stats {
	
	//private const F = false;

	//presets
	public long cycle = 0;//pause between states
	public long loop = 20;//frames between commands
	public int state = 0;//current state of AI
	public long rep = 0;//how long have we done this action?
	public int lastState = -1;//last state that 
	//public int boolean = 0;
	public bool hop = true;//does this mob jump on every action?
	public int moveX = RIGHT;//which way we want to move while facing is which way we're facing
	public int id = 0;//id number, unused right now
	public SpawnMobs source = null;//If null, destroy on death


	public void march(){
		if(cycle == 0){
			controller.giveInput(moveX,0,hop);
		}
		if(rep == 20) state = -1;//after moving for this amount of time, idle
	}//march to designated direction of screen

	public void idle(){
		if(cycle == 0){
			controller.giveInput(0,0,hop);
		}
		if(rep == 3)
			state = 0;
		
	}//jump in place 3 times

	/*protected void OnTriggerEnter2D(Collider2D other){
		base.OnTriggerEnter2D(other);
		/*if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
			boolean = 1;
			this.controller.giveInput (moveX,0,false,true,true,true);
		}*/
	/*}

	protected void OnTriggerStay2D(Collider2D other){
		base.OnTriggerStay2D(other);
		/*if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
			boolean = 1;
			this.controller.giveInput (moveX,0,false,true,true,true);
		}*/
	//}

	public override bool takeDamage(float damage){
		if(base.takeDamage(damage)){
			dies();
			return true;
		}
		return false;
	}//replaces stats takedamage

    protected override void dies()
    {
        if (!dead)
        {
            //board.print(this.gameObject.name + " has died.");
            if (source == null)
                base.dies();
            else {
                this.gameObject.SetActive(false);
                source.despawn(id);
            }//if no spawner kill, else despawn
        }
    }

	public void kill(string message = ""){
		if(source == null){
            board.print(message);
            Destroy(this.gameObject);
		}
		else{
			this.gameObject.SetActive(false);
			source.despawn(id);
		}
	}

	public void setDirection(int dir){//LEFT || RIGHT
		this.moveX = dir;
	}

	protected new void Update () {
		//moveX = facing;
		if(cycle > loop){
			cycle = 0;

			if(state == lastState)
				++rep;
		}
		if(state != lastState)
			rep = 0;
		lastState = state;

		switch(state)
		{
		  	case 0:
				march();
				break;
		  	default:
				idle ();
				break;
		}
		base.Update();//calls the update from charactercontroller

		++cycle;
	}
}
