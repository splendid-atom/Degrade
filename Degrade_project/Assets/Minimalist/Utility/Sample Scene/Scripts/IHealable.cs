using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minimalist.Utility.SampleScene
{
    public interface IHealable
    {
        public void ReceiveHeal(float heal);
    }
}