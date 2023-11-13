using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerEnter : MonoBehaviour
{
    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     var temp = transform.GetComponent<UnityEngine.UI.Image>();
    //     transform.GetComponent<UnityEngine.UI.Image>().color = new Color(temp.color.r, temp.color.g, temp.color.b, 0.25f);
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     var temp = transform.GetComponent<UnityEngine.UI.Image>();
    //     transform.GetComponent<UnityEngine.UI.Image>().color = new Color(temp.color.r, temp.color.g, temp.color.b, 0f);
    // }

    void OnMouseEnter()
    {
        // Debug.Log("Enter");
        var temp = transform.GetComponent<SpriteRenderer>();
        if (temp != null)
        {
            if (GameManager.instance.isPlayable)
            {
                if (GameManager.instance.movableFields.Contains(this.transform.GetComponent<PieceField>()))
                {
                    temp.color = new Color(255f, 255f, 0f, 0.5f);
                }
                else
                {
                    temp.color = new Color(255f, 255f, 255f, 0.5f);
                }
            }


        }
        else
        {
            Debug.LogWarning("SpriteRenderer component not found on object: " + name);
        }

    }

    void OnMouseExit()
    {
        // Debug.Log("Exit");
        var temp = transform.GetComponent<SpriteRenderer>();
        if (temp != null)
        {
            if (GameManager.instance.isPlayable)
            {
                if (GameManager.instance.movableFields.Contains(this.transform.GetComponent<PieceField>()))
                {
                    temp.color = new Color(255f, 255f, 0f, 0.25f);
                }
                else
                {
                    temp.color = new Color(255f, 255f, 255f, 0f);
                }
            }
        }
        else
        {
            Debug.LogWarning("SpriteRenderer component not found on object: " + name);
        }

    }

    void OnMouseDown()
    {
        if (GameManager.instance.isPlayable)
        {
            if (this.transform.GetComponent<PieceField>() != null && this.transform.GetComponent<PieceField>().GetPiece() != null
                    && this.transform.GetComponent<PieceField>().GetPiece().GetPlayerOwner() == GameManager.instance.GetCurrentTurnPlayer())
            {
                GameManager.instance.UpdateMovableField(this.transform.GetComponent<PieceField>());
            }

            if (GameManager.instance.GetSelectedPiece() == null
                    && this.gameObject.GetComponent<PieceField>().GetPiece() != null
                    && GameManager.instance.GetCurrentTurnPlayer() == this.gameObject.GetComponent<PieceField>().GetPiece().GetPlayerOwner())
            {
                GameManager.instance.SetSelectedPiece(this.gameObject.GetComponent<PieceField>());

                Debug.Log("Case1");
            }
            else if (GameManager.instance.GetSelectedPiece() != null
            && this.gameObject.GetComponent<PieceField>().GetPiece() != null
            && this.gameObject.GetComponent<PieceField>().GetPiece().GetPlayerOwner() == GameManager.instance.GetSelectedPiece().GetPiece().GetPlayerOwner()
             && GameManager.instance.GetCurrentTurnPlayer() == this.gameObject.GetComponent<PieceField>().GetPiece().GetPlayerOwner())
            {
                GameManager.instance.SetSelectedPiece(this.gameObject.GetComponent<PieceField>());
                Debug.Log("Case2");
            }
            else if (this.gameObject.TryGetComponent(out PieceField pieceField) &&
                     ((pieceField.GetPiece() == null) || (pieceField.GetPiece() != null &&
                     pieceField.GetPiece().GetPlayerOwner() != Players.Empty &&
                     pieceField.GetPiece().GetPlayerOwner() != GameManager.instance.GetSelectedPiece()?.GetPiece()?.GetPlayerOwner())) &&
                     GameManager.instance.GetSelectedPiece() != null &&
                     GameManager.instance.GetSelectedPiece().GetPiece() != null &&
                     GameManager.instance.GetCurrentTurnPlayer() == GameManager.instance.GetSelectedPiece().GetPiece().GetPlayerOwner())
            {
                var movableFields = GameManager.instance.MovablePieceField(GameManager.instance.GetSelectedPiece());
                if (movableFields != null)
                {
                    for (int i = 0; i < movableFields.Count; i++)
                    {
                        if (pieceField == movableFields[i])
                        {
                            GameManager.instance.MoveAction(GameManager.instance.GetSelectedPiece(), movableFields[i]);
                            Debug.Log("Case3");
                            return;
                        }
                    }
                    GameManager.instance.movableFields = new List<PieceField>();
                    GameManager.instance.ResetMovableAuraField();
                    Debug.Log("Case4");
                    GameManager.instance.SetSelectedPiece(null);
                }
                else
                {
                    Debug.Log("Case5");

                }

            }
            else
            {
                Debug.Log("Case6");
            }

        }
    }
}
