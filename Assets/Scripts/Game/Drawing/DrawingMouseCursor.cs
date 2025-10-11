using UnityEngine;
using Spine.Unity;

public class DrawingMouseCursor : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation spineObject;

    private string currentAnimation;
    private float lastMouseX;

    private const float idleThreshold = 0.05f;

    private bool isActivateDrawMouse = false;


    public void SetMouse()
    {
        lastMouseX = Input.mousePosition.x;
        Cursor.visible = false;
        isActivateDrawMouse = true;

        SetAnimation("wolf_idle1");
    }

    public void Init()
    {
        spineObject.gameObject.SetActive(false);
        isActivateDrawMouse = false;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isActivateDrawMouse) return;

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        worldMousePosition.z = spineObject.transform.position.z;
        transform.position = worldMousePosition;

        float mouseDeltaX = mousePosition.x - lastMouseX;

        if (Mathf.Abs(mouseDeltaX) > idleThreshold)
        {
            if (mouseDeltaX > 0)
            {
                SetAnimation("wolf_Right");
            }
            else
            {
                SetAnimation("wolf_Left");
            }
        }
        else
        {
            SetAnimation("wolf_idle1");
        }

        lastMouseX = mousePosition.x;
    }

    private void SetAnimation(string animationName, bool loop = true)
    {
        if (currentAnimation != animationName)
        {
            spineObject.AnimationState.SetAnimation(0, animationName, loop);
            currentAnimation = animationName;
        }
    }
}