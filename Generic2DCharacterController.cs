using UnityEngine;
using System.Collections;

/*Generic character controller for 2D games in Unity
 * 
 */

public class Generic2DCharacterController: Stats {
	//Constants
	const float MAXSPEED = 15f;//maximum landspeed by default
	const int LAND_DELAY = 5;//lag from landing, wait this many frames, considering time based
	const int DASH_FRAMES = 40;//frames between allowing dashes
	const int ATT_DELAY = 10;
	const double THRESHOLD = 0.5;
	
	//Components
	public Rigidbody2D body;//physics object in unity
	public ControllerInput controller;//controller inputs
	private Animator anim;//the animator


	//Presets
	bool layerCollide = false;//collide with objects on same layer
	public float accel = 100;//acceleration of character
	//[SerializeField][Range(0f,100f)]private float speed;
	public float jumpSpeed = 30;//speed when jumping
	public int airMod = 10;//change in speed when not grounded
	public float dashSpeed = 30;//speed when dashing
	public bool animate = false;//Does this thing have animation (more on this later TODO)
	public bool takeKnockback = true;//Does this charcter take knockback on hits
	public bool leftfacing_sprite = false;
	
	//Current State
	//inherits StunClk from stats
	public bool isGrounded;//is the character on the ground, set on collisions
	public Vector2 speed;
	public int[] cooldown = {0,0,0};//jump,dash,attack
	
	//Intermediate Values
	private Vector2 moveVect;//vector applied to movement, velocity is set to this every frame
	private Vector3 tempVect;//random vector
	//private float tempF;//intermediary float
	//private Quaternion rotation;
	
	
	//first thing that is run when game starts
	protected override void Awake(){
		base.Awake();
		anim = GetComponent<Animator>();
		controller = GetComponent<ControllerInput>();
		body = GetComponent<Rigidbody2D>();//main rigidbody
    }
	
	// Use this for initialization
	protected override void Start () {
		base.Start();//call start from 
	}
	
	// Update is called once per frame
	protected override void Update(){
		if(dead){
			return;
		}
		speed = body.velocity;
		moveVect = body.velocity;
		if(cooldown[0] < 1 && stunClk < 1){
			attack();
			if(isGrounded && ((animate) ? !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") : true)){//if attacking don't allow movement
					move();//move in x direction
					jump();//put before dash
					dash();//quick burst of speed in one direction
			}
			else{
				if(!isGrounded){
					move(airMod);
				}
			}
		}
		if(!isGrounded && (facing*moveVect.x) > MAXSPEED)
			moveVect.Set(facing*MAXSPEED,moveVect.y);
		body.velocity = moveVect;//modify physics based on moveVect
		base.Update();
	}
	
	protected virtual void FixedUpdate(){//run in a fixed amount of frames
		for(int i = 0; i < cooldown.Length; i++){
			if(cooldown[i] > 0)
				--cooldown[i];
		}//decrement all counters

		if(stunClk > 0)
			--stunClk;//decrement stunClk;
		
		if(controller.h == 0){
			if(animate){anim.SetBool("Walking", false);}
		}
		
		if(controller.h == 0 && isGrounded){
			tempVect = body.velocity;//apply current vel to vector
			tempVect.x = tempVect.x - tempVect.x/10;//apply extra friction
			body.velocity = tempVect;//change velocity
			return;
		}//Friction
	}
	
	void move(int modifier = 1){//movement in the x direction
		
		if(controller.h > 0){//check movement right
			if(animate){
				anim.SetBool("Walking", true);
			}
			tempVect = body.transform.localScale;
			if(facing == LEFT){
				flip_sprite();
			}

			//moveVect.x = MAXSPEED/modifier;
			moveVect.x += accel/modifier;
			if(moveVect.x > MAXSPEED) moveVect.x = MAXSPEED;
		}else if(controller.h < 0){//check movement left
			if(animate){
				anim.SetBool("Walking", true);
			}
			tempVect = body.transform.localScale;
			if(facing == RIGHT){
				flip_sprite();
			}
			
			//moveVect.x = -MAXSPEED/modifier;
			moveVect.x -= accel/modifier;
			if(moveVect.x < -MAXSPEED) moveVect.x = -MAXSPEED;
		}
	}//move
	
	void jump(){//jump upwards
		if(controller.f1 && isGrounded){//check if is on ground
			if(((animate) ? anim.GetCurrentAnimatorStateInfo(0).IsName("Dash") : false) || moveVect.x > MAXSPEED || moveVect.x < -MAXSPEED){
				moveVect.y += jumpSpeed;//move up
				moveVect.x = facing*MAXSPEED;//
			}//special dash jump,gives additional boost
			else{
				moveVect.y += jumpSpeed;//normal jump
			}
			//isGrounded = false;
		}//single jump only
	}

	void drop(){//not actually used, might not use built in gravity
		if(controller.v <= 0 && controller.f1){
			//Debug.Log ("drop");
		}
	}
	
	void dash(int modifier = 1){//accelerate in one direction
		if(controller.f2 && isGrounded){
			if(cooldown[1] == 0){
				moveVect += (Vector2.right * dashSpeed/modifier * facing);
				cooldown[1] = DASH_FRAMES;
				if(animate){anim.Play("Dash");}//play Dashing animation
			}
		}
	}

	public void flip_sprite(){
		setFacing ((facing == LEFT) ? RIGHT : LEFT);
	}//flips the sprite

	void attack(){//attack animation
		if(controller.f3){
			if(animate){anim.Play("Attack");}
			moveVect.x = 0;
			cooldown[0] = ATT_DELAY;
		}
	}

	public override void setFacing(int dir){//set direction in which character is facing
		if(dir != facing){
			tempVect.x = -tempVect.x;
			body.transform.localScale = tempVect;
			base.setFacing (dir);
		}
	}

	public override bool takeDamage(float damage){
		return receive_damage(damage);
	}

	public bool receive_damage(float damage, float x_force = 0, float y_force = 0, int stun_f = 0, bool reset_x = false, bool reset_y = false, Rigidbody2D other = null){
		/*
			damage: amount of damage to inflict
			x_force: knockback in the x direction
			y_force: knockback in the y direction
			stun_f: amount of stun to this thing
			reset_x: kill momentum in the x
			reset_y: kill momentum in the y
			other: thing that is attacking, physics is applied in relation to this.
		*/
		apply_stun(stun_f);//apply stun if any
		if(takeKnockback){//if this character can be knockedback
			if(reset_x)body.velocity = Vector3.up * body.velocity.y;
			if(reset_y)body.velocity = Vector3.right * body.velocity.x;
			body.AddForce(Vector2.up * y_force, ForceMode2D.Impulse);//hit object upwards
			body.AddForce(Vector3.right * (((other.position.x - body.position.x) > 0) ? LEFT : RIGHT ) * x_force, ForceMode2D.Impulse);//move object in relation to other
		}
		if(base.takeDamage(damage) == LETHAL){//from stats
			body.velocity = Vector3.zero;
			dies();//optional, state based action in stats
			return true;
		}
		return false;
	}//take damage, if new hp <= 0 delete object
	
	
	
	public override IEnumerator onDeath (){//coroutine: play death animation if applicable and destroy this
		if(animate && anim.HasState(0, Animator.StringToHash("Death"))){//if this asset has an animator
			anim.Play("Death");//play the death animation
			yield return null;
			while(anim.GetCurrentAnimatorStateInfo(0).IsName("Death")){
				//Debug.Log ("Dying");
				yield return null;
			}
			if(destroyOnDeath)
				Destroy(body.gameObject);
			else 
				reset();
		}

		base.onDeath();
	}
	
	
	
	//physics: handling colliders and triggers
	protected void OnCollisionEnter2D(Collision2D coll) {
		if(coll.gameObject.CompareTag("Ground") || coll.gameObject.CompareTag("Hitbox")){
			isGrounded = true;
			cooldown[0] = LAND_DELAY;
		}
	}

	protected void OnCollisionStay2D(Collision2D coll){
		if(coll.gameObject.tag == "Ground" || coll.gameObject.tag == "Hitbox"){
			isGrounded = true;
		}
	}
	
	protected void OnCollisionExit2D(Collision2D coll) {
		if(coll.gameObject.tag == "Ground" || coll.gameObject.tag == "Hitbox"){
			isGrounded = false;
		
		}
		
	}

	protected void OnTriggerEnter2D(Collider2D other){
		if( speed.y > 0 && other.gameObject.layer == LayerMask.NameToLayer("Platform")){
			Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(),other, true);//deactivate platform collisions
		}
	}

	protected void OnTriggerStay2D(Collider2D other){
		if(controller.v <= -THRESHOLD && other.gameObject.layer == LayerMask.NameToLayer("Platform")){
			Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(),other, true);//deactivate platform collisions
		}
	}
	
	//end of colliders
}
