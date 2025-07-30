using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [MyHeader("Entity")]
    
    [Tooltip("Entity max health")]
    [SerializeField] private float _maxHealth = 3f;
    [ReadOnly]
    [Tooltip("Entity's health")]
    [SerializeField] private float _health;

    [Header("References")]
    [SerializeField] protected Rigidbody _rigid; // Entity's Rigidbody.

    public float Health
    {
        get { return _health; }
        set { _health = Mathf.Clamp(value, 0f, _maxHealth); }
    }

    public float MaxHealth => _maxHealth;

    public Rigidbody Rigid => _rigid;
    public bool isDead => Health <= 0f;

    protected virtual void Start()
    {
        if (!_rigid)
        {
            TryGetComponent(out _rigid);
        }

        Health = MaxHealth;
    }

    protected virtual void Update() { }

    // Function to reset health.
    public void ResetHealth() => Health = MaxHealth;

    // Function to execute when the entity takes damage, reduce it by the damage amount.
    public virtual void OnTakeDamage(float damageAmount)
    {
        Health -= damageAmount;

        if (isDead)
        {
            OnDeath();
        }
    }

    // Function to execute when the entity is dead.
    public virtual void OnDeath() { }
}
