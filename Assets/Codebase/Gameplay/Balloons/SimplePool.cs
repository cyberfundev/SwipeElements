using System.Collections.Generic;
using UnityEngine;

namespace Codebase.Gameplay.Balloons
{
    public class SimplePool<T> where T: MonoBehaviour
    {
        private List<T> activeElements = new();
        private List<T> freeElements = new();
        private Transform _parent;
        private T _prefab;

        public SimplePool(Transform parent, T prefab, int initialAmount = 0)
        {
            _prefab = prefab;
            _parent = parent;
		
            for (int i = 0; i < initialAmount; i++)
            {
                AddElement();
            }
        }

        public void Despawn(T element)
        {
            activeElements.Remove(element);
            element.gameObject.SetActive(false);
            freeElements.Add(element);
        }
	
        public T GetElement()
        {
            if (freeElements.Count == 0)
            {
                AddElement();
            }

            T element = freeElements[0];
            freeElements.Remove(element);
            activeElements.Add(element);
            element.gameObject.SetActive(true);
            return element;
        }

        public void Clear()
        {
            freeElements.AddRange(activeElements);
            activeElements.Clear();

            foreach (T element in freeElements)
            {
                element.gameObject.SetActive(false);
            }
        }

        private void AddElement()
        {
            T newElement = Object.Instantiate(_prefab, _parent);
            newElement.gameObject.SetActive(false);
            freeElements.Add(newElement);
        }
    }
}