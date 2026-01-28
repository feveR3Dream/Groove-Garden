using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Browse : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float browsingSpeed = 2.5f;      // Camera pan speed for browsing at SELECTION


    [Header("Turntable Reference")]
    [SerializeField] private Transform turntable;


    // References
    private Camera cam;
    private Vector2 differencePos;


    // Values
    private float heightDifference;

    private bool canBrowseVertically = true;   
    private bool canBrowseHorizontally = true;


    private void Awake()
    {
        cam = Camera.main;

        if (turntable != null && cam != null)
        {
            heightDifference = Mathf.Abs(turntable.position.y - cam.transform.position.y);
        }
    }

    void Update()
    {
        if (UIManager.Instance.CurrentScreen == Screen.Selection &&
            !UIManager.Instance.IsTransitioning)
        {
            Corrector();

            // Browsing Horizontally
            if (UIManager.Instance.CurrentHoverDirection != Direction.None &&
                canBrowseHorizontally)
            {
                HorizontalBrowse(UIManager.Instance.CurrentHoverDirection);
            }

            // Browsing Vertically
            if (canBrowseVertically)
            {
                VerticalBrowse();
            }
        }
        else
        {
            canBrowseHorizontally = false;
            canBrowseVertically = false;    
        }
    }

    private void Corrector()
    {
        float camHeight = UIManager.Instance.TopCameraLimit.position.y - UIManager.Instance.BottomCameraLimit.position.y;
        float shelfHeight = UIManager.Instance.TopShelfLimit.position.y - UIManager.Instance.BottomShelfLimit.position.y;

        float camWidth = UIManager.Instance.RightCameraLimit.position.x - UIManager.Instance.LeftCameraLimit.position.x;
        float shelfWidth = UIManager.Instance.RightShelfLimit.position.x - UIManager.Instance.LeftShelfLimit.position.x;

        Vector3 correction = Vector3.zero;

        // --- HORIZONTAL CORRECTION ---
        if (camWidth > shelfWidth)
        {
            // Camera is BIGGER than the shelf -> Center it.
            if (canBrowseHorizontally) canBrowseHorizontally = false;

            float shelfCenter = (UIManager.Instance.LeftShelfLimit.position.x + UIManager.Instance.RightShelfLimit.position.x) / 2f;
            float camCenter = (UIManager.Instance.LeftCameraLimit.position.x + UIManager.Instance.RightCameraLimit.position.x) / 2f;
            correction.x = shelfCenter - camCenter;
        }
        else
        {
            // Camera fits inside -> Clamp edges.
            if (!canBrowseHorizontally) canBrowseHorizontally = true;

            if (UIManager.Instance.LeftCameraLimit.position.x < UIManager.Instance.LeftShelfLimit.position.x)
            {
                // Push right.
                correction.x = UIManager.Instance.LeftShelfLimit.position.x - UIManager.Instance.LeftCameraLimit.position.x;
            }
            else if (UIManager.Instance.RightCameraLimit.position.x > UIManager.Instance.RightShelfLimit.position.x)
            {
                // Push left.
                correction.x = UIManager.Instance.RightShelfLimit.position.x - UIManager.Instance.RightCameraLimit.position.x;
            }
        }

        // --- VERTICAL CORRECTION ---
        if (camHeight > shelfHeight)
        {
            // Camera is TALLER than shelf -> Center vertically.
            if (canBrowseVertically) canBrowseVertically = false;

            float shelfCenterY = (UIManager.Instance.TopShelfLimit.position.y + UIManager.Instance.BottomShelfLimit.position.y) / 2f;
            float camCenterY = (UIManager.Instance.TopCameraLimit.position.y + UIManager.Instance.BottomCameraLimit.position.y) / 2f;
            correction.y = shelfCenterY - camCenterY;
        }
        else
        {
            // Camera fits vertically -> Clamp edges.
            if (!canBrowseVertically) canBrowseVertically = true;

            if (UIManager.Instance.BottomCameraLimit.position.y < UIManager.Instance.BottomShelfLimit.position.y)
            {
                correction.y = UIManager.Instance.BottomShelfLimit.position.y - UIManager.Instance.BottomCameraLimit.position.y;
            }
            else if (UIManager.Instance.TopCameraLimit.position.y > UIManager.Instance.TopShelfLimit.position.y)
            {
                correction.y = UIManager.Instance.TopShelfLimit.position.y - UIManager.Instance.TopCameraLimit.position.y;
            }
        }

        cam.transform.position += correction;
    }

    private void HorizontalBrowse(Direction direction)
    {
        float speed = browsingSpeed * Time.deltaTime;

        if (direction == Direction.Left)
        {
            // Check Limits via Singleton
            if (UIManager.Instance.LeftCameraLimit.position.x > UIManager.Instance.LeftShelfLimit.position.x + 0.1f)
            {
                cam.transform.position += Vector3.left * speed;
            }
        }
        else if (direction == Direction.Right)
        {
            if (UIManager.Instance.RightCameraLimit.position.x < UIManager.Instance.RightShelfLimit.position.x - 0.1f)
            {
                cam.transform.position += Vector3.right * speed;
            }
        }
    }

    private void VerticalBrowse()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scrollInput, 0f)) return;

        float moveAmount = scrollInput * browsingSpeed * Time.deltaTime * 200f;

        if (moveAmount > 0)
        {
            if (UIManager.Instance.TopCameraLimit.position.y < UIManager.Instance.TopShelfLimit.position.y)
            {
                cam.transform.Translate(Vector3.up * moveAmount);
            }
        }
        else if (moveAmount < 0)
        {
            if (UIManager.Instance.BottomCameraLimit.position.y > UIManager.Instance.BottomShelfLimit.position.y)
            {
                cam.transform.Translate(Vector3.up * moveAmount);
            }
        }

        TurntableFollowCameraY();
    }

    private void TurntableFollowCameraY()
    {
        differencePos = new Vector2(turntable.position.x, cam.transform.position.y + heightDifference);
        turntable.position = differencePos;
    }
}