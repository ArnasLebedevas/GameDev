using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
	[SerializeField]
	private bool combatEnabled;
	[SerializeField]
	private float inputTimer, attack1Radius, attack1Damage;
	[SerializeField]
	private Transform attack1HitBoxPos;
	[SerializeField]
	private LayerMask whatIsDamageable;

	private bool gotInput, isAttaccking, isFirstAttack;

	private float lastInputTime = Mathf.NegativeInfinity;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("CanAttack", combatEnabled);
	}

	private void Update()
	{
		CheckCombatInput();
		CheckAttacks();
	}

	private void CheckCombatInput()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if(combatEnabled)
			{
				gotInput = true;
				lastInputTime = Time.time;
			}
		}
	}

	private void CheckAttacks()
	{
		if(gotInput)
		{
			if(!isAttaccking)
			{
				gotInput = false;
				isAttaccking = true;
				isFirstAttack = !isFirstAttack;

				animator.SetBool("Attack1", true);
				animator.SetBool("FirstAttack", isFirstAttack);
				animator.SetBool("IsAttacking", isAttaccking);
			}
		}
		if(Time.time >= lastInputTime + inputTimer)
		{
			gotInput = false;
		}
	}

	private void CheckAttackHitBox()
	{
		Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

		foreach (Collider2D collider in detectedObjects)
		{
			collider.transform.parent.SendMessage("Damage", attack1Damage);
		}
	}

	private void FinishAttack1()
	{
		isAttaccking = false;
		animator.SetBool("IsAttacking", isAttaccking);
		animator.SetBool("Attack1", false);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
	}
}
