using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minimalist.Utility
{
    [ExecuteInEditMode]
    public class CachedMonoBehaviour : MonoBehaviour
    {
        // Protected properties
        protected GameObject GameObject => _gameObject == null || !Application.isPlaying ? base.gameObject : _gameObject;
        protected Transform Transform => _transform == null || !Application.isPlaying ? base.transform : _transform;

        // Private fields
        private GameObject _gameObject;
        private Transform _transform;

        private void Awake()
        {
            _gameObject = this.GameObject;

            _transform = this.Transform;
        }
    }
}
