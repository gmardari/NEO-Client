using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public GameObject loginPanel;

    void Awake()
    {
        
    }

    public void OnCreateAccClick()
    {

        if (!EOManager.Connected)
        {
            EOManager.Singleton.Connect();
            UIManager.Singleton.waitingForConnection = true;
            UIManager.Singleton.willOpenLogin = false;
        }
        else
        {
            UIManager.Singleton.SetState(UIState.ACCOUNT_CREATION);
        }

    }

    public void OnPlayGameClick()
    {

        if (!EOManager.Connected)
        {
            EOManager.Singleton.Connect();
            UIManager.Singleton.waitingForConnection = true;
            UIManager.Singleton.willOpenLogin = true;
        }
        else
        {
            //OpenLoginPanel();
            UIManager.Singleton.SetState(UIState.LOGIN);
        }
        
    }



    public void OnExitGameClick()
    {
        Application.Quit();
    }

    //TODO: Input verification
    public void OnLoginConnectClick()
    {
        //Transform loginPanel = guiRoot.transform.Find("LoginPanel");
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (username.Length == 0 || password.Length == 0)
        {
            UIManager.Singleton.ShowGameDialog("Invalid credentials", "Username and password fields must not be empty");
            return;
        }

        Debug.Log($"Username:{username}, Password:{password}");

        EOManager.Singleton.Login(username, password);
    }

    public void OnLoginCancelClick()
    {

        //Clear input fields
        usernameInput.text = "";
        passwordInput.text = "";

        loginPanel.SetActive(false);
        UIManager.Singleton.SetState(UIState.MAIN_MENU);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
