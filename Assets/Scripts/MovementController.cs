using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
	[SerializeField]
	Vector2 velocity; //players x, y velocity
	bool isGrounded; //is grounded
	bool isJumping; // isJumping
	public float speed = 2f;
	[SerializeField] public Rigidbody2D rb;
	public Transform tf;
	public float cur_velocity;
	
	
	int time; //time of jump (in frames 1 jump = 60 frames)
	int timePerJump; //this is the time per a jump. out of 60
	int jumpState = 1;
	
	

	public Animator animator;
	public bool facingRight = true;
	public float horizMovement;

	void Update()
	{
		Vector2 playerLocation = GetComponent<Transform>().position;
		

		horizMovement = Input.GetAxisRaw("Horizontal");
		animator.ResetTrigger("Dash");


		//playerLocation.x += horizMovement * speed * Time.deltaTime;
		float targetVelocity = horizMovement * speed;
		rb.velocity = new Vector2 (targetVelocity, rb.velocity.y);

		cur_velocity = rb.velocity.x;

		// Set animator condition
		animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
		//animator.SetBool(press_shift);

		

		// Flip sprite when sprite direction and movement direction are different
		if(horizMovement < 0 && facingRight) {
			FlipSprite();
		}
		else if (horizMovement > 0 && !facingRight) {
			FlipSprite();
		}
		
				
		
		GetComponent<Transform>().position = playerLocation;
		if (Input.GetKey(KeyCode.Space)) //jump
		{
			if (timePerJump < 60)
			{
				timePerJump += 4;
			}
			if (!isJumping)
			{
				jumpState = 1;
				isJumping = true;
				isGrounded = false;
			}
		}
		if (isJumping)
		{
			HandleJump(timePerJump, playerLocation);
		}

	}
	
	bool dash;
	private void FixedUpdate() {

		if (Input.GetKey(KeyCode.LeftShift))
		{
			float dashVelocity = horizMovement * speed * 10;
			rb.velocity = new Vector2 (dashVelocity, rb.velocity.y);
			animator.SetTrigger("Dash");
		}
	}


	
	private void FlipSprite() {
		facingRight = !facingRight;

		// Change local scale
		Vector3 scale = tf.localScale;
		scale.x *= -1;
		tf.localScale = scale; 
	}
	
	private void HandleJump(int estimateFrames, Vector2 playerLocation)
	{
		Vector2 v = GetComponent<Rigidbody2D>().velocity; //RB velocity

		if (jumpState == 1) //jumping
		{
			if (time >= estimateFrames / 2) //change to the falling state
			{
				jumpState = 2; //commit from neo-vim
				return;
			}
			
			// position_y = y + vt + 0.5gt^2 
			// the += removes the need for a position variable
			playerLocation.y +=  velocity.y * Time.deltaTime + ((9.8f*(Time.deltaTime*Time.deltaTime))/2);	
			// velocity_final = v_init + at
			v.y += velocity.y * Time.deltaTime + ((9.8f*(Time.deltaTime*Time.deltaTime))/2);
			
			// Update components
			GetComponent<Transform>().position = playerLocation;
			GetComponent<Rigidbody2D>().velocity = v;
		}
		else if (jumpState == 2) //falling
		{
			// same as state1, but -= this time
			playerLocation.y -= velocity.y * Time.deltaTime + ((9.8f*(Time.deltaTime*Time.deltaTime))/2);

			v.y = (velocity.y - v.y) * Time.deltaTime + ((9.8f*(Time.deltaTime*Time.deltaTime))/2); //velocity*time + ((G * time^2)/2) good formula for jumps
			GetComponent<Rigidbody2D>().velocity = v;
			GetComponent<Transform>().position = playerLocation;
		}
		//this makes sure its valid
		if (time > estimateFrames && isGrounded)
		{
			ResetJumps();
			return;
		}
		time++;

	}
	/// <summary>
	/// resets jump states.
	/// </summary>
	
	private void ResetJumps()
	{
		isJumping = false;
		time = 0;
		jumpState = 1;
		timePerJump = 0;
	}
	
	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "floor"){

			//calculates angle so you only get it iff its on the floor
			Vector3 hit = other.contacts[0].normal;
			float angle = Vector3.Angle(hit, Vector3.up);
			if (Mathf.Approximately(angle, 0))  //if floor collsion
			{
				ResetJumps();
				isGrounded = true;
			}
			else if (Mathf.Approximately(angle, 180))  //if roof collison
			{
				//Up
				if (isJumping)
				{
					jumpState = 2; //modify to falling state if jumping active

				}
			}
		}
	}
	



	
}
