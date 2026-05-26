using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnJumpStarted;
    public event EventHandler OnPauseStarted;
    public event EventHandler OnBindingRebound;

    public enum Binding
    {
        MoveLeft,
        MoveRight,
        Jump,
        FastFall,
        Pause
    }

    private InputActions inputActions;
    
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    public Vector2 MoveInput { get; private set; }
    public InputActions InputActions => inputActions;
    public bool IsRebinding => rebindingOperation != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inputActions = new InputActions();

        inputActions.Player.Jump.started += _ => OnJumpStarted?.Invoke(this, EventArgs.Empty);
        inputActions.Player.Pause.started += _ => OnPauseStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Start()
    {
        if (SaveSystem.Instance != null)
        {
            string savedOverrides = SaveSystem.Instance.GetBindingOverrides();
            if (!string.IsNullOrEmpty(savedOverrides))
            {
                inputActions.LoadBindingOverridesFromJson(savedOverrides);
            }
        }
    }

    private void Update()
    {
        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void OnDestroy()
    {
        inputActions.Player.Disable();
        inputActions.Dispose();
    }

    public void EnablePlayerInput()
    {
        inputActions?.Player.Enable();
    }

    public void DisablePlayerInput()
    {
        inputActions?.Player.Disable();
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            case Binding.FastFall:
                return inputActions.Player.Move.GetBindingDisplayString(1);
            case Binding.MoveLeft:
                return inputActions.Player.Move.GetBindingDisplayString(2);
            case Binding.MoveRight:
                return inputActions.Player.Move.GetBindingDisplayString(3);
            case Binding.Jump:
                return inputActions.Player.Jump.GetBindingDisplayString();
            case Binding.Pause:
                return inputActions.Player.Pause.GetBindingDisplayString();
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Cancels the current interactive rebinding operation if one is running.
    /// </summary>
    public void CancelRebind()
    {
        rebindingOperation?.Cancel();
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        // Prevent starting overlapping operations
        if (rebindingOperation != null) return;

        inputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex = 0;

        switch (binding)
        {
            default:
            case Binding.FastFall:
                inputAction = inputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.MoveLeft:
                inputAction = inputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.MoveRight:
                inputAction = inputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Jump:
                inputAction = inputActions.Player.Jump;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 0;
                break;
        }

        rebindingOperation = inputAction.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>")
            .OnComplete(callback =>
            {
                callback.Dispose();
                rebindingOperation = null;
                inputActions.Player.Enable();
                onActionRebound();

                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.SetBindingOverrides(inputActions.SaveBindingOverridesAsJson());
                }

                OnBindingRebound?.Invoke(this, EventArgs.Empty);
            })
            .OnCancel(callback =>
            {
                callback.Dispose();
                rebindingOperation = null;
                inputActions.Player.Enable();
                onActionRebound(); // Calls visual reset back in UI
            });

        rebindingOperation.Start();
    }
}