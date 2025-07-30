using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [SerializeField] private GameObject PausePanel;

    private GameObject currentPanel; // Current pause panel.
    private bool IsPaused = false; // Determine if game is pause or not.
    private bool canClose = true; // Determine if player can close panel with key.

    private bool keyLock = false;

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        // [ESC] - Pause.
        if (Input.GetKeyDown(KeyCode.Escape) && canClose && !keyLock)
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Function to resume the game.
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (currentPanel)
        {
            Animator currentPanelAnimator;

            if (currentPanel.TryGetComponent(out currentPanelAnimator))
            {
                currentPanelAnimator.Play("Hide");
                Invoke("ClosePanel", currentPanelAnimator.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                currentPanel.SetActive(false);
                currentPanel = null;
            }

            return;
        }

        Animator panelAnimator;

        if (PausePanel.TryGetComponent(out panelAnimator))
        {
            panelAnimator.Play("Hide");
            Invoke("ClosePanel", panelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
        else
        {
            PausePanel.SetActive(false);
            currentPanel = null;
        }
    }

    // Function to pause the game.
    public void PauseGame()
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        PausePanel.SetActive(true);
        currentPanel = PausePanel;

        Animator panelAnimator;

        if (PausePanel.TryGetComponent(out panelAnimator))
        {
            panelAnimator.Play("Show");
        }
    }

    // Functio to pause the game with particular panel.
    public void PauseGame(GameObject panel)
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        currentPanel = panel;
        panel.SetActive(true);

        Animator panelAnimator;

        if (panel.TryGetComponent(out panelAnimator))
        {
            panelAnimator.Play("Show");
        }
    }

    // Function to pause the game with particular panel.
    public void PauseGame(GameObject panel, bool canClose)
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        currentPanel = panel;
        panel.SetActive(true);

        Animator panelAnimator;

        if (panel.TryGetComponent(out panelAnimator))
        {
            panelAnimator.Play("Show");
        }

        this.canClose = canClose;
    }

    // Function to set key lock.
    public void SetKeyLock(bool value) => keyLock = value;

    // Function to close current panel (For panel with animation).
    private void ClosePanel()
    {
        if (currentPanel)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }
    }
}
