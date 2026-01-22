using UnityEngine;
using UnityEngine.InputSystem;

public class InGameMenuController : MonoBehaviour
{
    GameObject pauseMenu;
    bool pauseMenuActive = false;
    PlayerInput playerInput;
    InputAction pauseAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        pauseAction = playerInput.actions["Cancel"];
    }

    void Start()
    {
        pauseMenu = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (pauseAction.triggered)
            ChangeMenuState(!pauseMenuActive);
    }

    public void ChangeMenuState(bool isPaused)
    {
        if (pauseMenuActive == isPaused)
            return;

        pauseMenu.SetActive(isPaused);

        pauseMenuActive = isPaused;
        GameManager.IsPaused = isPaused;
    }
}
