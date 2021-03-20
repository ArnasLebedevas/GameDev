using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;

    public Transform AttackPoint;

    public LayerMask enemyLayers;

    public int AttackDamage;

    public float AttackRate = 2f;
    public float AttackRange = 0.5f;

    private float nextAttackTime = 0f;

	private void Start()
	{
        animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Time.time >= nextAttackTime)
		{
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				Attack();
				nextAttackTime = Time.time + 1f / AttackRate;
			}
		}
	}

    private void Attack()
	{
		animator.SetTrigger("Attack1");

		Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AttackPoint.position, AttackRange, enemyLayers);

		foreach (Collider2D Enemies in hitEnemies)
		{
			//Enemies.GetComponent<Enemy>().TakeDamage(attackDamage);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (AttackPoint == null) return;

		Gizmos.DrawWireSphere(AttackPoint.position, AttackRange);
	}

}
