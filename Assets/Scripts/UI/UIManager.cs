using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton;
    public static string serverFailRequestMsg = "We could not process your request at this time. Please try again.";

    public GameObject startMenuPrefab;
    public GameObject csMenuPrefab;
    public GameObject accCreatePrefab;
    public GameObject gameOverlayPrefab;
    public GameObject gameDialogPrefab;
    

    [HideInInspector]
    public UIState state;
    [HideInInspector]
    public GameObject guiBranch;
    [HideInInspector]
    public GameObject guiRoot;
    [HideInInspector]
    public GameObject gameDialog;

    private GameObject startMenu;
    private GameObject csMenu;
    private GameObject accCreateMenu;
    private GameObject healthBar;
    private GameObject manaBar;
    private GameObject energyBar;
    private GameObject expBar;


    public bool canTransition;
    public bool waitingForConnection;
    public bool willOpenLogin;             //After connection is open, opens the login panel if true, otherwise, open account creation page
    public bool waitingForCharacters;

    // Start is called before the first frame update
    void Awake()
    {
        guiRoot = GameObject.FindGameObjectWithTag("GUI");
        Singleton = this;
        StartMainMenu();
    }

    public void StartMainMenu()
    {
        state = UIState.MAIN_MENU;
        canTransition = true;
        waitingForConnection = false;
        waitingForCharacters = false;

        startMenu = Instantiate(startMenuPrefab, this.transform);
        csMenu = Instantiate(csMenuPrefab, this.transform);
        accCreateMenu = Instantiate(accCreatePrefab, this.transform);

        startMenu.name = startMenuPrefab.name;
        csMenu.name = csMenuPrefab.name;
        accCreateMenu.name = accCreatePrefab.name;
        csMenu.SetActive(false);
        accCreateMenu.SetActive(false);
        guiBranch = startMenu;

        Transform mainButtonPanel = startMenu.transform.Find("MainButtonPanel");
        Transform loginPanel = startMenu.transform.Find("LoginPanel");

        /*
        mainButtonPanel.Find("CreateAcc").GetComponent<Button>().onClick.AddListener(() => OnCreateAccClick());
        mainButtonPanel.Find("Play").GetComponent<Button>().onClick.AddListener(() => OnPlayGameClick(startMenu));
        mainButtonPanel.Find("Exit").GetComponent<Button>().onClick.AddListener(() => OnExitGameClick(startMenu));

        loginPanel.Find("Cancel").GetComponent<Button>().onClick.AddListener(() => OnLoginCancelClick());
        loginPanel.Find("Connect").GetComponent<Button>().onClick.AddListener(OnLoginConnect);
        */

        EOManager.OnLoginSucceed += OnLoginSucceed;
        //EOManager.OnCSLoaded += OnCSLoaded;
        //EOManager.OnAccCreate += OnAccCreate;
    }

    public void StartGameOverlay()
    {
        GameObject gameGui = Instantiate(gameOverlayPrefab, this.transform);
        gameGui.name = gameOverlayPrefab.name;

        var overlay = gameGui.transform.Find("Overlay");
        var paperdoll = gameGui.transform.Find("Paperdoll");
        var chestDisplay = gameGui.transform.Find("ChestDisplay");
        var display = gameGui.transform.Find("Display");

        overlay.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        //Attach button listeners
        display.Find("Inventory/PaperdollButton").GetComponent<Button>().onClick.AddListener(PaperdollManager.Singleton.OnPaperdollBtnClick);
        paperdoll.Find("OK").GetComponent<Button>().onClick.AddListener(PaperdollManager.Singleton.OnOkBtnClick);

        guiBranch = gameGui;

        PaperdollManager.Singleton.uiPaperdoll = paperdoll.gameObject;
        PaperdollManager.Singleton.slotsContainer = paperdoll.Find("Slots").gameObject;
        PaperdollManager.Singleton.ui_properties = paperdoll.Find("Properties").gameObject;
        InvManager.Singleton.invContainer = display.Find("Inventory/Items").gameObject;
        ChestInvManager.Singleton.invDisplay = chestDisplay.gameObject;
        //chestDisplay.Find("Footer/Close").GetComponent<Button>().onClick.AddListener(ChestInvManager.Singleton.OnCloseBtnClick);
        
        ChestInvManager.Singleton.RegisterEvents();

        healthBar = gameGui.transform.Find("Health/Bar").gameObject;
        manaBar = gameGui.transform.Find("Mana/Bar").gameObject;
        energyBar = gameGui.transform.Find("Energy/Bar").gameObject;
        expBar = gameGui.transform.Find("Exp/Bar").gameObject;
    }

    public void ShowGameDialog(string title, string message)
    {
        if(gameDialog != null)
        {
            Destroy(gameDialog);
        }

        gameDialog = Instantiate(gameDialogPrefab, transform);
        gameDialog.transform.Find("Title").GetComponent<TMP_Text>().text = title;
        gameDialog.transform.Find("Message").GetComponent<TMP_Text>().text = message;
        gameDialog.transform.Find("OK").GetComponent<Button>().onClick.AddListener(() => 
        { Destroy(gameDialog); gameDialog = null; });
    }

    public void ShowGameDialog(string title, string message, Action onConfirm)
    {
        if (gameDialog != null)
        {
            Destroy(gameDialog);
        }

        gameDialog = Instantiate(gameDialogPrefab, transform);
        gameDialog.transform.Find("Title").GetComponent<TMP_Text>().text = title;
        gameDialog.transform.Find("Message").GetComponent<TMP_Text>().text = message;
        gameDialog.transform.Find("OK").GetComponent<Button>().onClick.AddListener(() =>
        { Destroy(gameDialog); gameDialog = null; onConfirm(); });
    }

    //TODO: Handle from In Game -> Main Menu
    public void SetState(UIState _state)
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }

        state = _state;

        switch(_state)
        {
            case UIState.MAIN_MENU:

                guiBranch = startMenu;
                guiBranch.SetActive(true);
                
                break;

            case UIState.LOGIN:

                guiBranch = startMenu;
                guiBranch.SetActive(true);
                guiBranch.transform.Find("LoginPanel").gameObject.SetActive(true);
                guiBranch.transform.Find("LoginPanel/UsernameInput").gameObject.GetComponent<TMP_InputField>().Select();
                break;

            case UIState.CHARACTER_SELECT:

                guiBranch = csMenu;
                guiBranch.SetActive(true);
                break;

            case UIState.ACCOUNT_CREATION:

                guiBranch = accCreateMenu;
                guiBranch.SetActive(true);
                break;

            case UIState.IN_GAME:

                foreach (Transform t in transform)
                {
                    Destroy(t.gameObject);
                }

                StartGameOverlay();
                //Instantiate(invManagerPrefab); //Use InvManager.Singleton to access

                break;
        }

    }

    //TODO: Input verification
    public void OnLoginConnect()
    {
        //Transform loginPanel = guiRoot.transform.Find("LoginPanel");
        string username = guiBranch.transform.Find("LoginPanel/UsernameInput").GetComponent<InputField>().text;
        string password = guiBranch.transform.Find("LoginPanel/PasswordInput").GetComponent<InputField>().text;

        Debug.Log($"Username:{username}, Password:{password}");

        EOManager.Singleton.Login(username, password);
    }

    public void OnPlayGameClick(GameObject startMenu)
    {
        if(state == UIState.MAIN_MENU)
        {
            //startMenu.transform.Find("LoginPanel").gameObject.SetActive(true);
            //state = UIState.LOGIN;

            if(!EOManager.Connected)
            {
                EOManager.Singleton.Connect();
                waitingForConnection = true;
                willOpenLogin = true;
            }
            else
            {
                //OpenLoginPanel();
                SetState(UIState.LOGIN);
            }
        }
    }

    public void OnCreateAccClick()
    {
        if(state == UIState.MAIN_MENU)
        {
            if (!EOManager.Connected)
            {
                EOManager.Singleton.Connect();
                waitingForConnection = true;
                willOpenLogin = false;
            }
            else
            {
                SetState(UIState.ACCOUNT_CREATION);
            }
        }
    }

    public void OnPaperdollBtnClick()
    {
        var paperdoll = guiBranch.transform.Find("Paperdoll");
        paperdoll.gameObject.SetActive(!paperdoll.gameObject.activeSelf);
    }

    /*
    private void OpenLoginPanel()
    {
        guiRoot.transform.Find("LoginPanel").gameObject.SetActive(true);
        SetState(UIState.LOGIN);
    }
    */


    public void OnLoginSucceed()
    {
        Debug.Log("UIManager login succeed");
        
        waitingForCharacters = true;
    }

    public void OnCSLoaded()
    {
        transform.Find("CharacterSelect").GetComponent<CharacterSelect>().Init();
        SetState(UIState.CHARACTER_SELECT);
        
    }

    public void OnAccCreate(ACCOUNT_CREATE_RESP resp)
    {
        if(resp == ACCOUNT_CREATE_RESP.SUCCESS)
            SetState(UIState.MAIN_MENU);

        switch (resp)
        {
            case ACCOUNT_CREATE_RESP.SUCCESS:
                UIManager.Singleton.ShowGameDialog("Account Created", "Account created. You may login with your credentials.");
                break;

            case ACCOUNT_CREATE_RESP.SERVER_ERROR:
                UIManager.Singleton.ShowGameDialog("Account Failed", serverFailRequestMsg);
                break;

            case ACCOUNT_CREATE_RESP.USER_TAKEN:
                UIManager.Singleton.ShowGameDialog("Account Failed", "This username is taken. Try another one");
                break; 

            case ACCOUNT_CREATE_RESP.USER_INVALID:
                UIManager.Singleton.ShowGameDialog("Account Failed", "Username is invalid.");
                break; 

            case ACCOUNT_CREATE_RESP.PASS_INVALID:
                UIManager.Singleton.ShowGameDialog("Account Failed", "Password is invalid.");
                break; 

            case ACCOUNT_CREATE_RESP.REALNAME_INVALID:
                UIManager.Singleton.ShowGameDialog("Account Failed", "Real name is invalid.");
                break; 

            case ACCOUNT_CREATE_RESP.LOCATION_INVALID:
                UIManager.Singleton.ShowGameDialog("Account Failed", "Location is invalid.");
                break; 

            case ACCOUNT_CREATE_RESP.EMAIL_INVALID:
                UIManager.Singleton.ShowGameDialog("Account Failed", "Email is invalid.");
                break; 
        }

        
       
    }

    public void OnCharacterLoginClick(int char_index)
    {
        CS_CharacterDef def = EOManager.cs_defs[char_index];

        Debug.Log($"Entering world with char {def.name}");
        EOManager.OnWorldEnter += OnWorldEnter;
        EOManager.Singleton.EnterWorld(char_index);
    }

    public void OnLoginCancelClick()
    {
        if (state == UIState.LOGIN)
        {
            //Clear input fields
            guiBranch.transform.Find("LoginPanel/UsernameInput").GetComponent<InputField>().text = "";
            guiBranch.transform.Find("LoginPanel/PasswordInput").GetComponent<InputField>().text = "";

            guiBranch.transform.Find("LoginPanel").gameObject.SetActive(false);
            SetState(UIState.MAIN_MENU);
        }
    }

    //Inventory UI

    private void OnWorldEnter()
    {
        //Debug.Log("Entering world UI event");
        SetState(UIState.IN_GAME);
    }

    public void OnExitGameClick(GameObject startMenu)
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            /*case UIState.MAIN_MENU:
                
                if(waitingForConnection)
                {
                    if(EOManager.GetSessionState() == NetworkSessionState.ACCEPTED)
                    {
                        waitingForConnection = false;
                        Debug.Log("Connection opened!!!");

                        if (willOpenLogin)
                            //OpenLoginPanel();
                            SetState(UIState.LOGIN);
                        else
                            SetState(UIState.ACCOUNT_CREATION);
                    }   
                }
                break;*/

            case UIState.LOGIN:

                break;

                //TODO: Put in UIGame.cs
            case UIState.IN_GAME:
                {
                    if(EOManager.eo_map.IsLoaded && EOManager.player != null)
                    {
                        var eo_char = EOManager.EO_Character;
                        float healthAlpha = (eo_char.props.maxHealth > 0) ? ( (float)eo_char.props.health) / ( (float)eo_char.props.maxHealth) : 0;
                        float manaAlpha = (eo_char.props.maxMana > 0) ? ( (float)eo_char.props.mana) / ( (float)eo_char.props.maxMana) : 0;
                        float energyAlpha = (eo_char.props.maxEnergy > 0) ? ((float)eo_char.props.energy) / ( (float)eo_char.props.maxEnergy) : 0;
                        ulong barExp = (eo_char.props.exp - eo_char.props.expLevel);
                        float expAlpha = (barExp > 0) ? ((float)barExp) / ((float)eo_char.props.expTNL) : 0;

                        healthBar.GetComponent<Image>().fillAmount = healthAlpha;
                        manaBar.GetComponent<Image>().fillAmount = manaAlpha;
                        energyBar.GetComponent<Image>().fillAmount = energyAlpha;
                        expBar.GetComponent<Image>().fillAmount = expAlpha;
                        //healthBar.GetComponent<Image>().fillAmount = healthAlpha;
                    }
                    break;
                }
        }
    }
}
