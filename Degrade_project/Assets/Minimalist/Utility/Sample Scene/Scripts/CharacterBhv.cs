using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimalist.Quantity;

namespace Minimalist.Utility.SampleScene
{
    [ExecuteInEditMode]
    public class CharacterBhv : MonoBehaviour,
        IDamageable,
        IHealable
    {
        // Public fields
        public enum Team
        {
            Blue,
            Red
        }
        [Header("Team:")]
        public Team team;

        [Header("Quantities:")]
        public QuantityBhv health;

        public virtual void TakeDamage(float damage)
        {
            health.Amount -= damage;
        }

        public virtual void ReceiveHeal(float heal)
        {
            health.Amount += heal;
        }
    }
}