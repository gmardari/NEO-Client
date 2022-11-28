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
    public GameObject mainPanel;

    void Awake()
    {
        foreach(Transform t in mainPanel.transform)
        {
            Button button = t.GetComponent<Button>();

            if(button != null)
            {
                button.onClick.AddListener(() => AudioManager.Singleton.Play(1));
            }
        }

        foreach (Transform t in loginPanel.transform)
        {
            Button button = t.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() => AudioManager.Singleton.Play(1));
            }
        }
    }

    public void OnCreateAccClick()
    {

        if (!EOManager.Connected)
        {
            if (!UIManager.Singleton.waitingForConnection)
            {
                EOManager.packetManager.Register(PacketType.HELLO_PACKET, 2.0f, (packet) => OnHelloPacketReceive(packet, UIState.ACCOUNT_CREATION),
                () =>
                {
                    UIManager.Singleton.waitingForConnection = false;
                    UIManager.Singleton.ShowGameDialog("No connection", "Failed to connect to server");
                });
                EOManager.Singleton.Connect();
                UIManager.Singleton.waitingForConnection = true;
                UIManager.Singleton.willOpenLogin = false;
            }   
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
            if(!UIManager.Singleton.waitingForConnection)
            {
                EOManager.packetManager.Register(PacketType.HELLO_PACKET, 2.0f, (packet) => OnHelloPacketReceive(packet, UIState.LOGIN), 
                () =>
                {
                    UIManager.Singleton.waitingForConnection = false;
                    UIManager.Singleton.ShowGameDialog("No connection", "Failed to connect to server");
                });
                EOManager.Singleton.Connect();
                UIManager.Singleton.waitingForConnection = true;
                UIManager.Singleton.willOpenLogin = true;
            }
        }
        else if(EOManager.Singleton.GetSessionState() == NetworkSessionState.ACCEPTED)
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

    public void OnHelloPacketReceive(PacketReader packet, UIState uiState)
    {
        if (EOManager.Singleton.GetSessionState() == NetworkSessionState.PINGING)
        {
            UIManager.Singleton.waitingForConnection = false;
            EOManager.Singleton.SetSessionState(NetworkSessionState.ACCEPTED);
            UIManager.Singleton.SetState(uiState);

            Debug.Log("Connection opened!!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
