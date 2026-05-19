using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : BasePanel
{
    public static SettingsUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button musicVolumeButton;
    [SerializeField] private Button sfxVolumeButton;
    [SerializeField] private Button moveLeftRebindButton;
    [SerializeField] private Button moveRightRebindButton;
    [SerializeField] private Button fastFallRebindButton;
    [SerializeField] private Button jumpRebindButton;
    [SerializeField] private Button pauseRebindButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI moveLeftRebindText;
    [SerializeField] private TextMeshProUGUI moveRightRebindText;
    [SerializeField] private TextMeshProUGUI fastFallRebindText;
    [SerializeField] private TextMeshProUGUI jumpRebindText;
    [SerializeField] private TextMeshProUGUI pauseRebindText;

    [Header("Rebinding Colors")]
    [SerializeField] private Color normalTexColor = Color.black;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color activeTextColor = Color.white;
    [SerializeField] private Color activeButtonColor = Color.gray;

    private GameInput.Binding? activeRebindingBinding = null;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        closeButton.onClick.AddListener(Hide);

        // Assign listeners for all rebind operations
        moveLeftRebindButton.onClick.AddListener(() => StartRebindingProcess(GameInput.Binding.MoveLeft, moveLeftRebindText, moveLeftRebindButton));
        moveRightRebindButton.onClick.AddListener(() => StartRebindingProcess(GameInput.Binding.MoveRight, moveRightRebindText, moveRightRebindButton));
        fastFallRebindButton.onClick.AddListener(() => StartRebindingProcess(GameInput.Binding.FastFall, fastFallRebindText, fastFallRebindButton));
        jumpRebindButton.onClick.AddListener(() => StartRebindingProcess(GameInput.Binding.Jump, jumpRebindText, jumpRebindButton));
        pauseRebindButton.onClick.AddListener(() => StartRebindingProcess(GameInput.Binding.Pause, pauseRebindText, pauseRebindButton));

        SnapHidden();
    }

    public override void Show(System.Action onClose = null)
    {
        base.Show(onClose);
        UpdateVisual();

        if (GameInput.Instance != null && PlayerController.Instance != null)
        {
            GameInput.Instance.InputActions.Player.Disable();
        }
    }

    public override void Hide()
    {
        // Safe check: If menu is closed mid-rebind, force cancel it
        if (GameInput.Instance != null && GameInput.Instance.IsRebinding)
        {
            GameInput.Instance.CancelRebind();
        }

        if (GameInput.Instance != null && PlayerController.Instance != null)
        {
            GameInput.Instance.InputActions.Player.Enable();
        }

        base.Hide();
    }

    // =========================================================================
    // Visual Updates
    // =========================================================================

    private void UpdateVisual()
    {
        if (GameInput.Instance == null) return;

        // Fetch current key text strings
        moveLeftRebindText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveLeft);
        moveRightRebindText.text = GameInput.Instance.GetBindingText(GameInput.Binding.MoveRight);
        fastFallRebindText.text = GameInput.Instance.GetBindingText(GameInput.Binding.FastFall);
        jumpRebindText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Jump);
        pauseRebindText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);

        // Reset all styles/colors back to standard defaults
        ResetButtonColors(moveLeftRebindText, moveLeftRebindButton);
        ResetButtonColors(moveRightRebindText, moveRightRebindButton);
        ResetButtonColors(fastFallRebindText, fastFallRebindButton);
        ResetButtonColors(jumpRebindText, jumpRebindButton);
        ResetButtonColors(pauseRebindText, pauseRebindButton);
    }

    private void ResetButtonColors(TextMeshProUGUI textMesh, Button button)
    {
        textMesh.color = normalTexColor;
        if (button.TryGetComponent(out Image btnImage))
        {
            btnImage.color = normalButtonColor;
        }
    }

    // =========================================================================
    // Rebinding Logic Handlers
    // =========================================================================

    private void StartRebindingProcess(GameInput.Binding binding, TextMeshProUGUI rebindText, Button rebindButton)
    {
        // If clicking the EXACT same button that is actively listening -> CANCEL IT
        if (activeRebindingBinding == binding)
        {
            GameInput.Instance.CancelRebind();
            return;
        }

        // If some other rebind button is already running, block input overlap
        if (GameInput.Instance.IsRebinding) return;

        activeRebindingBinding = binding;

        // Switch to Listening State visual cues
        rebindText.text = "Press any key...";
        rebindText.color = activeTextColor;
        
        if (rebindButton.TryGetComponent(out Image btnImage))
        {
            btnImage.color = activeButtonColor;
        }

        // Pass operation to the Input Manager
        GameInput.Instance.RebindBinding(binding, () =>
        {
            // Callback context executes on completion or cancellation
            activeRebindingBinding = null;
            UpdateVisual();
        });
    }
}