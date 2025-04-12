using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minimalist.Quantity
{
    [System.Serializable]
    public class QuantityDynamics
    {
        // Public properties
        public QuantityDynamicsType Type
        {
            get
            {
                return _type;
            }
        }
        public float DeltaPercentage
        {
            get
            {
                return _deltaPercentage / 100f;
            }
        }
        public float SignedDeltaAmount
        {
            get
            {
                return _deltaAmount * (float)_type;
            }
        }
        public float DeltaTime
        {
            get
            {
                return _deltaTime;
            }
        }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = value;
            }
        }

        // Private serialized fields
        [SerializeField] private QuantityDynamicsType _type = QuantityDynamicsType.Accumulation;
        [SerializeField] private QuantityDynamicsDeltaAmountType _deltaAmountType = QuantityDynamicsDeltaAmountType.Absolute;
        [SerializeField, Range(0f, 100f)] private float _deltaPercentage = 1f;
        [SerializeField, Min(0f)] private float _deltaAmount;
        [SerializeField, Min(.02f)] private float _deltaTime = .5f;
        [SerializeField] private bool _enabled = true;
    }
}