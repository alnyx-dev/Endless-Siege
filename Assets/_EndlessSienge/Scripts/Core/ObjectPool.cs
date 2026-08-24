using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new Stack<T>();

        public ObjectPool(T prefab, Transform parent, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                T instance = CreateNew();
                Deactivate(instance);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();

            Transform t = instance.transform;
            t.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            instance.OnSpawn();

            return instance;
        }

        public void Release(T instance)
        {
            Deactivate(instance);
        }

        private T CreateNew()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        private void Deactivate(T instance)
        {
            instance.OnDespawn();
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }
    }
}
