using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minimalist.Quantity
{
    public class QuantitySubscriberTemplateBhv : QuantitySubscriber
    {
        private void DummyMethod1()
        {
            Debug.Log("Current amount: " + Quantity.Amount + " " + Quantity.Units);
        }

        private void DummyMethod2()
        {
            Debug.Log("The requested change would result in an invalid amount.");
        }

        protected override void SpecifyQuantityEventActions()
        {
            OnQuantityAmountChangedAction = DummyMethod1;

            OnQuantityInvalidRequestAction = DummyMethod2;
        }
    }
}