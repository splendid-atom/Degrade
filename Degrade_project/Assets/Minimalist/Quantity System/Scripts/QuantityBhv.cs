using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using Minimalist.Utility;

namespace Minimalist.Quantity
{
    [DisallowMultipleComponent]
    public class QuantityBhv : MonoBehaviour
    {
        // Public properties
        public string Units
        {
            get
            {
                return _units;
            }
        }
        public float MaximumAmount
        {
            get
            {
                return _maximumAmount;
            }

            set
            {
                _maximumAmount = value;
                _minimumAmount = ValidateMinimumAmount(_minimumAmount);
                _capacity = Capacity;
                Amount = ValidateCurrentAmount(_currentAmount);
            }
        }
        public float MinimumAmount
        {
            get
            {
                return _minimumAmount;
            }

            set
            {
                _minimumAmount = value;
                _maximumAmount = ValidateMaximumAmount(_maximumAmount);
                _capacity = Capacity;
                Amount = ValidateCurrentAmount(_currentAmount);
            }
        }
        public float Capacity
        {
            get
            {
                return _maximumAmount - _minimumAmount;
            }
        }
        public float Amount
        {
            get
            {
                return _currentAmount;
            }

            set
            {
                float previousAmount = _currentAmount;
                _currentAmount = ValidateCurrentAmount(value);
                _deltaAmount = _currentAmount - previousAmount;
                _fillAmount = (_currentAmount - _minimumAmount) / Capacity;
                _onAmountChanged.Invoke();
                if (_currentAmount != previousAmount && (value == _maximumAmount || value == _minimumAmount) || value > _maximumAmount || value < _minimumAmount)
                {
                    _onInvalidRequest.Invoke();
                }
            }
        }
        public float DeltaAmount
        {
            get
            {
                return _deltaAmount;
            }
        }
        public float FillAmount
        {
            get
            {
                return _fillAmount;
            }

            set
            {
                float previousAmount = _currentAmount;
                _fillAmount = value;
                _currentAmount = _fillAmount * Capacity + _minimumAmount;
                _deltaAmount = _currentAmount - previousAmount;
                _onAmountChanged.Invoke();
            }
        }
        public QuantityDynamics PassiveDynamics
        {
            get
            {
                return _passiveDynamics;
            }
        }
        public UnityEvent OnNameChanged
        {
            get
            {
                return _onNameChanged;
            }

            set
            {
                _onNameChanged = value;
            }
        }
        public UnityEvent OnAmountChanged
        {
            get
            {
                return _onAmountChanged;
            }

            set
            {
                _onAmountChanged = value;
            }
        }
        public UnityEvent OnInvalidRequest
        {
            get
            {
                return _onInvalidRequest;
            }

            set
            {
                _onInvalidRequest = value;
            }
        }

        // Private serialized fields
        [SerializeField] private string _units;
        [SerializeField] private float _maximumAmount = 100;
        [SerializeField] private float _minimumAmount = 0;
        [SerializeField, ReadOnly] private float _capacity;
        [SerializeField] private float _currentAmount;
        [SerializeField, ReadOnly] private float _deltaAmount;
        [SerializeField, Range(0, 1)] private float _fillAmount;
        [SerializeField] private QuantityDynamics _passiveDynamics;
        [SerializeField] private Coroutine _passiveDynamicsCoroutine;
        [SerializeField] private UnityEvent _onNameChanged = new UnityEvent();
        [SerializeField] private UnityEvent _onAmountChanged = new UnityEvent();
        [SerializeField] private UnityEvent _onInvalidRequest = new UnityEvent();

        private float ValidateMaximumAmount(float value)
        {
            return Mathf.Max(_minimumAmount + .01f, value);
        }

        private float ValidateMinimumAmount(float value)
        {
            return Mathf.Min(_maximumAmount - .01f, value);
        }

        private float ValidateCurrentAmount(float value)
        {
            return Mathf.Clamp(value, _minimumAmount, _maximumAmount);
        }

        public void OnUndoRedoCallback()
        {
            MaximumAmount = MaximumAmount;
            MinimumAmount = MinimumAmount;
            FillAmount = FillAmount;
        }

        private void Start()
        {
            if (Application.isPlaying && PassiveDynamics.Type != QuantityDynamicsType.None)
            {
                StartPassiveDynamics();
            }
        }

        public void StartPassiveDynamics()
        {
            StopPassiveDynamics();

            _passiveDynamicsCoroutine = StartCoroutine(PassiveDynamicsCoroutine());
        }

        public void StopPassiveDynamics()
        {
            if (_passiveDynamicsCoroutine != null)
            {
                StopCoroutine(_passiveDynamicsCoroutine);
            }
        }

        private IEnumerator PassiveDynamicsCoroutine()
        {
            while (Application.isPlaying)
            {
                if (PassiveDynamics.Enabled)
                {
                    Amount = ValidateCurrentAmount(Amount + PassiveDynamics.SignedDeltaAmount);

                    yield return new WaitForSeconds(PassiveDynamics.DeltaTime);
                }

                else
                {
                    yield return null;
                }
            }
        }

        private void OnValidate()
        {
            this.PseudoResetOnCreation();
        }

        private void PseudoResetOnCreation()
        {
#if UNITY_EDITOR
            if (Event.current != null)
            {
                if (Event.current.commandName == "Duplicate" || Event.current.commandName == "Paste")
                {
                    _onNameChanged = new UnityEvent();
                    _onAmountChanged = new UnityEvent();
                    _onInvalidRequest = new UnityEvent();
                }
            }
#endif
        }

        #region UnityEvent Subscription Handling
        public void AddListener(UnityEvent unityEvent, UnityAction unityAction)
        {
#if UNITY_EDITOR
            if (unityAction == null)
            {
                return;
            }

            UnityEventTools.AddPersistentListener(unityEvent, unityAction);

            unityEvent.SetPersistentListenerState(unityEvent.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime);

            CleanUp(unityEvent);
#endif
        }

        public void RemoveListener(UnityEvent unityEvent, int index)
        {
#if UNITY_EDITOR
            UnityEventTools.RemovePersistentListener(unityEvent, index);

            CleanUp(unityEvent);
#endif
        }

        private void CleanUp(UnityEvent unityEvent)
        {
#if UNITY_EDITOR
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
            {
                if (unityEvent.GetPersistentTarget(i) == null)
                {
                    UnityEventTools.RemovePersistentListener(unityEvent, i);
                }
            }
#endif
        }
        #endregion
    }
}