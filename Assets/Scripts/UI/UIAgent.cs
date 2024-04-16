using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAgent : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject victoryMenu;
    
    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
        private set { animator = value; }
    }

    private static UIAgent instance;
    public static UIAgent Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<UIAgent>();
            return instance;
        }
        private set { instance = value; }
    }

    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (victoryMenu.activeSelf) return;

            bool isSettingsOpen = settingsMenu.activeSelf;
            if (isSettingsOpen)
            {
                settingsMenu.SetActive(false);
            }
            else if (GameController.Instance.AllowedToPause)
            {
                settingsMenu.SetActive(true);
            }
        }

        bool freeCursor = settingsMenu.activeSelf || victoryMenu.activeSelf
            || Input.GetAxis("Alt") > 0.1f;
        Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeCursor;
    }

    public void ActivateVictoryScreen()
    {
        settingsMenu.SetActive(false);
        victoryMenu.SetActive(true);
    }

    internal void HideAllMenus()
    {
        settingsMenu.SetActive(false);
        victoryMenu.SetActive(false);
    }
}
