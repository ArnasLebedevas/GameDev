using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private float movementInputDirection;
	private float jumpTimer;
	private float turnTimer;
	private float wallJumpTimer;
	private float dashTimeLeft;
	private float lastImageXpos;
	private float lastDash = -100f;

	private int amountOfJumpsLeft;
	private int facingDirection = 1;
	private int lastWallJumpDirection;

	private bool isFacingRight = true;
	private bool isWalking;
	private bool isGrounded;
	private bool isTouchingWall;
	private bool isWallSliding;
	private bool canNormalJump;
	private bool canWallJump;
	private bool isAttemptingToJump;
	private bool canMove;
	private bool canFlip;
	private bool hasWallJumped;
	private bool isTouchingLedge;
	//private bool canClimbLedge;
	//private bool ledgeDetected;
	private bool isDashing;

	//private Vector2 ledgePosBot;
	//private Vector2 ledgePos1;
	//private Vector2 ledgePos2;

	private Rigidbody2D rb2D;
	private Animator animator;

	public int AmountOfJumps = 1;

	public float MovementSpeed = 10.0f;
	public float JumpForce = 16.0f;
	public float GroundCheckRadius;
	public float WallCheckDistance;
	public float WallSlideSpeed;
	public float MovementForceInAir;
	public float AirDragMultiplier = 0.95f;
	public float WallHopForce;
	public float WallJumpForce;
	public float JumpTimerSet = 0.15f;
	public float VariableJumpHeightMultiplier = 0.5f;
	public float TurnTimerSet = 0.1f;
	public float WallJumpTimerSet = 0.5f;

	//public float ledgeClimbXOffSet1 = 0f;
	//public float ledgeClimbYOffSet1 = 0f;
	//public float ledgeClimbXOffSet2 = 0f;
	//public float ledgeClimbYOffSet2 = 0f;
	public float DashTime;
	public float DashSpeed;
	public float DistanceBetweenImages;
	public float DashCoolDown;

	public Vector2 WallHopDirection;
	public Vector2 WallJumpDirection;

	public Transform GroundCheck;
	public Transform WallCheck;
	//public Transform LedgeCheck;

	public LayerMask WhatIsGround;

	private void Start()
	{
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		amountOfJumpsLeft = AmountOfJumps;

		WallHopDirection.Normalize();
		WallJumpDirection.Normalize();
	}

	private void Update()
	{
		CheckInput();
		CheckMovementDirection();
		UpdateAnimations();
		CheckIfCanJump();
		CheckIfWallSliding();
		CheckJump();
		//CheckLedgeClimb();
		CheckDash();
	}

	private void FixedUpdate()
	{
		ApplyMovement();
		CheckSurroundings();
	}

	private void CheckIfWallSliding()
	{
		if (isTouchingWall && movementInputDirection == facingDirection /*&& rb2D.velocity.y < 0 && !canClimbLedge*/)
		{
			isWallSliding = true;
		}
		else
		{
			isWallSliding = false;
		}
	}

	/*private void CheckLedgeClimb()
	{
		if(ledgeDetected && !canClimbLedge)
		{
			canClimbLedge = true;

			if(isFacingRight)
			{
				ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + WallCheckDistance) - ledgeClimbXOffSet1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffSet1);
				ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + WallCheckDistance) + ledgeClimbXOffSet2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffSet2);
			}
			else
			{
				ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - WallCheckDistance) + ledgeClimbXOffSet1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffSet1);
				ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - WallCheckDistance) - ledgeClimbXOffSet2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffSet2);
			}

			canMove = false;
			canFlip = false;

			animator.SetBool("CanClimbLedge", canClimbLedge);
		}

		if(canClimbLedge)
		{
			transform.position = ledgePos1;
		}
	}*/

	/*public void FinishLedgeClimb()
	{
		canClimbLedge = false;
		transform.position = ledgePos2;
		canMove = true;
		canFlip = true;
		ledgeDetected = false;
		animator.SetBool("CanClimbLedge", canClimbLedge);
	}*/

	private void CheckSurroundings()
	{
		isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, WhatIsGround);

		isTouchingWall = Physics2D.Raycast(WallCheck.position, transform.right, WallCheckDistance, WhatIsGround);

		/*isTouchingLedge = Physics2D.Raycast(LedgeCheck.position, transform.right, WallCheckDistance, WhatIsGround);

		if(isTouchingWall && !isTouchingLedge && !ledgeDetected)
		{
			ledgeDetected = true;
			ledgePosBot = WallCheck.position;
		}*/
	}

	private void CheckIfCanJump()
	{
		if (isGrounded && rb2D.velocity.y <= 0.01f)
			amountOfJumpsLeft = AmountOfJumps;

		if (isTouchingWall)
			canWallJump = true;

		if (amountOfJumpsLeft <= 0)
			canNormalJump = false;
		else
			canNormalJump = true;
	}

	private void CheckDash()
	{
		if(isDashing)
		{
			if(dashTimeLeft > 0)
			{
				canMove = false;
				canFlip = false;
				rb2D.velocity = new Vector2(DashSpeed * facingDirection, 0);
				dashTimeLeft -= Time.deltaTime;

				if (Mathf.Abs(transform.position.x - lastImageXpos) > DistanceBetweenImages)
				{
					PlayerAfterImagePool.Instance.GetFromPool();
					lastImageXpos = transform.position.x;
				}
			}

			if(dashTimeLeft <= 0 || isTouchingWall)
			{
				isDashing = false;
				canMove = true;
				canFlip = true;
			}
		}
	}

	private void CheckMovementDirection()
	{
		if(isFacingRight && movementInputDirection < 0)
			Flip();
		else if(!isFacingRight && movementInputDirection > 0)
			Flip();

		if (Mathf.Abs(rb2D.velocity.x) >= 0.01f)
			isWalking = true;
		else
			isWalking = false;
	}

	private void UpdateAnimations()
	{
		animator.SetBool("IsRunning", isWalking);
		animator.SetBool("IsGrounded", isGrounded);
		animator.SetFloat("yVelocity", rb2D.velocity.y);
		animator.SetBool("IsWallSliding", isWallSliding);
	}

	private void CheckInput()
	{
		movementInputDirection = Input.GetAxisRaw("Horizontal");

		if(Input.GetButtonDown("Jump"))
		{
			if(isGrounded ||(amountOfJumpsLeft > 0 && isTouchingWall))
			{
				NormalJump();
			}
			else
			{
				jumpTimer = JumpTimerSet;

				isAttemptingToJump = true;
			}
		}

		if(Input.GetButtonDown("Horizontal") && isTouchingWall)
		{
			if(!isGrounded && movementInputDirection != facingDirection)
			{
				canMove = false;
				canFlip = false;

				turnTimer = TurnTimerSet;
			}
		}

		if(turnTimer >= 0)
		{
			turnTimer -= Time.deltaTime;

			if(turnTimer <= 0)
			{
				canMove = true;
				canFlip = true;
			}
		}

		if(Input.GetButtonDown("Dash"))
		{
			if(Time.time >= (lastDash + DashCoolDown))
				AttemptToDash();
		}
	}

	private void AttemptToDash()
	{
		isDashing = true;
		dashTimeLeft = DashTime;
		lastDash = Time.time;

		PlayerAfterImagePool.Instance.GetFromPool();
		lastImageXpos = transform.position.x;
	}

	private void ApplyMovement()
	{
		if (!isGrounded && !isWallSliding && movementInputDirection == 0)
		{
			rb2D.velocity = new Vector2(rb2D.velocity.x * AirDragMultiplier, rb2D.velocity.y);
		}
		else if(canMove)
		{
			rb2D.velocity = new Vector2(MovementSpeed * movementInputDirection, rb2D.velocity.y);
		}

		if(isWallSliding)
		{
			if (rb2D.velocity.y < -WallSlideSpeed)
			{
				rb2D.velocity = new Vector2(rb2D.velocity.x, -WallSlideSpeed);
			}
		}	
	}

	private void Flip()
	{
		if(!isWallSliding && canFlip)
		{
			facingDirection *= -1;
			isFacingRight = !isFacingRight;
			transform.Rotate(0.0f, 180.0f, 0.0f);
		}
	}

	public void DisableFlip()
	{
		canFlip = false;
	}

	public void EnableFlip()
	{
		canFlip = true;
	}

	private void CheckJump()
	{
		if(jumpTimer > 0)
		{
			//WallJump
			if(!isGrounded && isTouchingWall && movementInputDirection !=0 && movementInputDirection != facingDirection)
			{
				WallJump();
			}
			else if(isGrounded)
			{
				NormalJump();
			}
		}

		if(isAttemptingToJump)
			jumpTimer -= Time.deltaTime;

		if(wallJumpTimer > 0)
		{
			if(hasWallJumped && movementInputDirection == -lastWallJumpDirection)
			{
				rb2D.velocity = new Vector2(rb2D.velocity.x, -3.5f);
				hasWallJumped = false;
			}
			else if(wallJumpTimer <= 0)
			{
				hasWallJumped = false;
			}
			else
			{
				wallJumpTimer -= Time.deltaTime;
			}
		}
	}

	private void NormalJump()
	{
		if (canNormalJump)
		{
			rb2D.velocity = new Vector2(rb2D.velocity.x, JumpForce);
			amountOfJumpsLeft--;
			jumpTimer = 0;
			isAttemptingToJump = false;
		}
	}

	private void WallJump()
	{
	    if (canWallJump)
		{
			rb2D.velocity = new Vector2(rb2D.velocity.x, 0.0f);

			isWallSliding = false;

			amountOfJumpsLeft = AmountOfJumps;
			amountOfJumpsLeft--;

			Vector2 forceToAdd = new Vector2(WallJumpForce * WallJumpDirection.x * movementInputDirection, WallJumpForce * WallJumpDirection.y);
			rb2D.AddForce(forceToAdd, ForceMode2D.Impulse);

			jumpTimer = 0;

			isAttemptingToJump = false;

			turnTimer = 0;

			canMove = true;
			canFlip = true;

			hasWallJumped = true;
			wallJumpTimer = WallJumpTimerSet;
			lastWallJumpDirection = -facingDirection;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);

		Gizmos.DrawLine(WallCheck.position, new Vector3(WallCheck.position.x + WallCheckDistance, WallCheck.position.y, WallCheck.position.z));
	}
}
