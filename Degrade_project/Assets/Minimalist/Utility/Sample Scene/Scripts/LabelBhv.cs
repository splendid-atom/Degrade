using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using Minimalist.Quantity;

namespace Minimalist.Utility.SampleScene
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LabelBhv : QuantitySubscriber
    {
        // Private serialized fields
        [SerializeField] private DiscretizedGradient _fontColorGradient;
        [SerializeField] private string _amountFormat = "0";
        [SerializeField] private bool _displayQuantityName = true;
        [SerializeField] private bool _displayQuantityAmount = true;
        [SerializeField] private bool _displayQuantityMaximum = true;
        [SerializeField] private bool _displayQuantityUnits = true;

        // Private properties
        private TextMeshProUGUI TextMeshPro => _textMeshPro == null ? GetComponent<TextMeshProUGUI>() : _textMeshPro;


        // Private fields
        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = this.TextMeshPro;
        }

        private void Start()
        {
            SpecifyQuantityEventActions();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            this.UpdateText();
        }

        private void UpdateText()
        {
            if (Quantity == null)
            {
                return;
            }

            string text = "";

            if (_displayQuantityName)
            {
                text += Quantity.name + " ";
            }
            if (_displayQuantityAmount)
            {
                text += Quantity.Amount.ToString(_amountFormat);
            }
            if (_displayQuantityMaximum)
            {
                text += "/" + Quantity.MaximumAmount.ToString(_amountFormat);
            }
            if (_displayQuantityUnits)
            {
                text += " " + Quantity.Units;
            }

            this.TextMeshPro.text = text;

            this.TextMeshPro.color = _fontColorGradient.Evaluate(Quantity.FillAmount);
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }

        protected override void SpecifyQuantityEventActions()
        {
            OnQuantityAmountChangedAction = UpdateText;

            OnQuantityInvalidRequestAction = null;
        }
    }
}