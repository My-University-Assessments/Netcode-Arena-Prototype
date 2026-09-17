using System;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GridTile : MonoBehaviour, IGridTile, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TileType tileType { get; set; } = TileType.Ground;

    public static Action<GameObject> OnTileClicked { get; set; }

    private Rigidbody rb;
    public bool interactable { get; protected set; }
    private bool hovered;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

    }

    public void Clicked()
    {
        if (tileType != TileType.Ground) return;
        OnTileClicked?.Invoke(gameObject);

    }

    private Color originalColour;
    public void Hovered()
    {
        if (tileType != TileType.Ground) return;
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

        hovered = !hovered;
        string check = hovered ? "Hovered" : "Moved Off";

        if (hovered)
        {
            originalColour = meshRenderer.material.color; // INFO: Store the original colour
            meshRenderer.material.color = Color.green;

        }
        else
        {
            meshRenderer.material.color = originalColour;

        }

    }

    #region Pointer Events
    public void OnPointerClick(PointerEventData eventData) => Clicked();
    public void OnPointerEnter(PointerEventData eventData) => Hovered();
    public void OnPointerExit(PointerEventData eventData) => Hovered();

    #endregion
}