using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minimalist.Utility
{
    [ExecuteInEditMode]
    public class MinimalistSystemsManager : Singleton<MinimalistSystemsManager>
    {
        // Public fields
        [Tooltip("Controls whether or not quantity subscribers are automatically renamed to match their subsciption")]
        public bool automaticObjectNaming = true;

        private void OnEnable()
        {
            if (automaticObjectNaming)
            {
                this.name = "Minimalist Systems Manager";
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}