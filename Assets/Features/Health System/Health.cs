using UnityEngine;

namespace HealthSystem
{
    public class Health : MonoBehaviour, IDamageable
    {
        [field: Header("Health Settings")]
        [field: SerializeField] public float maxHealth { get; protected set; } = 100f;
        public float currentHealth { get; protected set; }

        [field: Header("Damage Settings")]
        [field: SerializeField] public float damage { get; protected set; } = 1f;

        private void Awake()
        {
            currentHealth = maxHealth;

        }


        public void Damage(float damage)
        {
            if (currentHealth <= 0) Die();
            currentHealth = Mathf.Clamp(currentHealth -= damage, 0, maxHealth);

        }

        public void Die()
        {
            Destroy(gameObject);


        }

    }
}