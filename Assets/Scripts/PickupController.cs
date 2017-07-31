using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    public Sprite NormalPickup;
    public Sprite SuperPickup;

    public bool Visible;
    public bool PreviewInSuperPuzzle;

    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();

        Visible = true;
    }

    void Update()
    {
        if (Visible)
        {
            if (PreviewInSuperPuzzle)
            {
                _renderer.sprite = SuperPickup;
            }
            else
            {
                _renderer.sprite = NormalPickup;
            }

            _renderer.enabled = true;
        }
        else
        {
            _renderer.enabled = false;
        }
    }
}
