using System;
using System.Collections.Generic;
using Codebase.Gameplay.Props;
using Codebase.Levels;
using UniRx;
using UnityEngine;

namespace Codebase.Gameplay.LevelGenerator
{
    public class PropsContainer
    {
        private readonly ISubject<(Prop, Vector2Int)> _onSwiped = new Subject<(Prop, Vector2Int)>();
        
        public IObservable<(Prop, Vector2Int)> OnSwiped => _onSwiped;
        public readonly Dictionary<Prop, PropObject> Props = new();

        public void RegisterProp(Prop prop, PropObject propObject)
        {
            Props.Add(prop, propObject);
            propObject.OnSwiped.Subscribe(direction => _onSwiped.OnNext((prop, direction)));
        }

        public void RemoveProps(List<Prop> propsToDestroy)
        {
            foreach (Prop prop in propsToDestroy)
            {
                Props[prop].gameObject.SetActive(false);
                Props.Remove(prop);
            }
        }
    }
}