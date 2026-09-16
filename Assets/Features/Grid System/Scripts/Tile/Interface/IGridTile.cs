using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem.Interface
{
    public interface IGridTile
    {
        bool interactable { get; }

        void Hovered();
        void Clicked();

    }
}