using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player Input")]
public class PlayerInput : ScriptableObject, InputActions.IGamePlayerActions
{
    // 通过事件调用下面的移动函数
    public event UnityAction<Vector3> onMove = delegate{};
    public event UnityAction onStopMove = delegate{};


    InputActions inputActions;

    void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.GamePlayer.SetCallbacks(this);
        Debug.Log("PlayerInput 已启用");
    }

    public void DisableAllInputs()
    {
        inputActions.GamePlayer.Disable();
    }


    public void EnableGamePlayerInput()
    {
        inputActions.GamePlayer.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            onMove.Invoke(context.ReadValue<Vector2>());    
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            onStopMove.Invoke();
        }
    }


}
