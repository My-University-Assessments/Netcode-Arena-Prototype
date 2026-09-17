using System;
using UnityEngine;

namespace ArenaPrototype.Feature.GridSystem.Interface
{
    public interface IGridTile
    {
        bool interactable { get; }
        TileType tileType { get; set; }

        static Action<GameObject> OnTileClicked { get; }

        void Hovered();
        void Clicked();

    }

    public enum TileType
    {
        Ground = 0,
        Water = 1,
        Hill = 2,

    }

}