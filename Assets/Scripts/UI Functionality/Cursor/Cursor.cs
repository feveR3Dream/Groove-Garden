using System.Collections;
using UnityEngine;

public class Cursor : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private LayerMask coverLayer;
    [SerializeField] private LayerMask turntableLayer;

    // References
    private GameObject currentSelectedGO = null;
    private Camera cam;
    private LayerMask targetLayerValue;

    // Values
    private Vector3 rayDir = new Vector3(0f, 0f, -10);

    // Coroutines
    private Coroutine draggingCoroutine = null;

    // Scripts
    private Record record; // This is your "Memory" of what you are holding

    // Interfaces
    private IButtonInteractable buttonInteractable = null;
    private IKnobInteractable knobInteractable = null;


    private void Awake()
    {
        cam = Camera.main;
        targetLayerValue = coverLayer | turntableLayer;
    }

    private void Update()
    {
        MouseInput();
    }

    private void MouseInput()
    {
        // MOUSE DOWN-------------------
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, targetLayerValue);

            if (hit.collider == null) return;
            currentSelectedGO = hit.collider.gameObject;

            #region BUTTON INTERACTION
            if (currentSelectedGO.TryGetComponent<IButtonInteractable>(out IButtonInteractable buttonInteractable))
            {
                this.buttonInteractable = buttonInteractable;
                this.buttonInteractable.ButtonInteracted(registered: true, MouseButton.Down);
            }
            #endregion


            #region KNOB INTERACTION
            else if (currentSelectedGO.TryGetComponent<IKnobInteractable>(out IKnobInteractable knobInteractable))
            {
                this.knobInteractable = knobInteractable;
                this.knobInteractable.KnobInteracted(MouseButton.Down);
            }
            #endregion


            #region NO VALID INTERACTION
            else
            {
                currentSelectedGO = null;
                this.buttonInteractable = null;
                this.knobInteractable = null;
            }
            #endregion

        }

        // MOUSE HOLD--------------------
        else if (Input.GetMouseButton(0))
        {
            if (this.buttonInteractable == null &&
                this.knobInteractable == null) return;

            Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, targetLayerValue);


            #region BUTTON INTERACTION
            if (this.buttonInteractable != null)
            {
                bool mouseDriftedOff = (hit.collider == null) || (hit.collider.gameObject != currentSelectedGO);

                if (mouseDriftedOff)
                {
                    this.buttonInteractable.ButtonInteracted(registered: false, MouseButton.Up);

                    this.buttonInteractable = null;
                    currentSelectedGO = null;
                    return;
                }

                this.buttonInteractable.ButtonInteracted(registered: true, MouseButton.Hold);
            }
            #endregion


            #region KNOB INTERACTION
            else if (this.knobInteractable != null)
            {
                this.knobInteractable.KnobInteracted(MouseButton.Hold);
            }            
            #endregion
        }

        // MOUSE UP------------------------
        else if (Input.GetMouseButtonUp(0))
        {
            #region BUTTON INTERACTION
            if (this.buttonInteractable != null)
            {
                Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 10f, targetLayerValue);

                bool validRelease = (hit.collider != null) && (hit.collider.gameObject == currentSelectedGO);
                this.buttonInteractable.ButtonInteracted(registered: validRelease, MouseButton.Up);
            }

            #endregion


            #region KNOB INTERACTION
            else if (this.knobInteractable != null)
            {
                this.knobInteractable.KnobInteracted(MouseButton.Up);
            }
            #endregion
            
            this.buttonInteractable = null;
            this.knobInteractable = null;
            currentSelectedGO = null;
        }
    }



    
}