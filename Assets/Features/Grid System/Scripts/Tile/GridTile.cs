using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GridTile : MonoBehaviour, IGridTile, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
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
        Debug.Log($"Clicked {name}!");

    }

    public void Hovered()
    {
        hovered = !hovered;
        string check = hovered ? "Hovered" : "Moved Off";

        if (hovered)
        {
            GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }
        else
        {
            GetComponentInChildren<MeshRenderer>().material.color = Color.white;

        }

    }

    #region Pointer Events
    public void OnPointerClick(PointerEventData eventData) => Clicked();
    public void OnPointerEnter(PointerEventData eventData) => Hovered();
    public void OnPointerExit(PointerEventData eventData) => Hovered();

    #endregion
}