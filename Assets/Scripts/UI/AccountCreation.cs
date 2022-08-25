using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccountCreation : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPassInput;
    public TMP_InputField realNameInput;
    public TMP_InputField locInput;
    public TMP_InputField emailInput;

    

    public uint[] minChars;

    void Awake()
    {

    }

    public void OnCreateClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string confirmPass = confirmPassInput.text;
        string realName = realNameInput.text;
        string loc = locInput.text;
        string email = emailInput.text;
        
        //Error check inputs
        if(username.Length < minChars[0])
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Username needs minumum {minChars[0]} characters.");
            return;
        }
        else if(password.Length < minChars[1])
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Password needs minumum {minChars[1]} characters.");

            return;
        }
        else if(!confirmPass.Equals(passwordInput.text))
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Passwords must match.");

            return;
        }
        else if(realName.Length < minChars[3])
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Real name needs minumum {minChars[3]} characters.");
            return;
        }
        else if (loc.Length < minChars[4])
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Location needs minumum {minChars[4]} characters.");

            return;
        }
        else if (email.Length < minChars[5])
        {
            UIManager.Singleton.ShowGameDialog("Invalid field", $"Email needs minumum {minChars[5]} characters.");

            return;
        }
        
        Debug.Log($"Creating account: {username}, {password}");
        

        //Inputs are valid
        EOManager.Singleton.CreateAccount(username, password, realName, loc, email);

    }

    public void OnCancelClick()
    {
        UIManager.Singleton.SetState(UIState.MAIN_MENU);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
