using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewCharacter : MonoBehaviour
{
    public TMP_InputField charNameInput;
    public Image genderImg;
    public Image hairStyleImg;
    public Image hairColourImg;
    public Image skinColourImg;
    public Button genderBtn;
    public Button hairStyleBtn;
    public Button hairColourBtn;
    public Button skinColourBtn;

    public CharacterPreview charPreviewScript;

    private byte _gender;
    private byte _hairStyle;
    private byte _hairColour;
    private byte _skinColour;

    CharacterDef def;

    void Awake()
    {
        /*
        genderBtn.onClick.AddListener(OnGenderBtnClick);
        hairStyleBtn.onClick.AddListener(OnHairStyleBtnClick);
        hairColourBtn.onClick.AddListener(OnHairColourBtnClick);
        skinColourBtn.onClick.AddListener(OnSkinColourBtnClick);
        */

        def = new CharacterDef();
        charPreviewScript.SetCharacterDef(def);
    }

    public void OnGenderBtnClick()
    {
        _gender++;

        if (_gender >= ResourceLibrary.Singleton.genderUIImages.Length)
            _gender = 0;

        genderImg.sprite = ResourceLibrary.Singleton.genderUIImages[_gender];

        def.gender = _gender;
        charPreviewScript.OnCharacterDefChange();
    }

    public void OnHairStyleBtnClick()
    {
        _hairStyle++;

        if (_hairStyle >= ResourceLibrary.Singleton.hairStyleUIImages.Length)
            _hairStyle = 0;

        hairStyleImg.sprite = ResourceLibrary.Singleton.hairStyleUIImages[_hairStyle];

        def.hairStyle = _hairStyle;
        charPreviewScript.OnCharacterDefChange();
    }

    public void OnHairColourBtnClick()
    {
        _hairColour++;

        if (_hairColour >= ResourceLibrary.Singleton.hairColourUIImages.Length)
            _hairColour = 0;

        hairColourImg.sprite = ResourceLibrary.Singleton.hairColourUIImages[_hairColour];

        def.hairColour = _hairColour;
        charPreviewScript.OnCharacterDefChange();
    }

    public void OnSkinColourBtnClick()
    {
        _skinColour++;

        if (_skinColour >= ResourceLibrary.Singleton.skinColourUIImages.Length)
            _skinColour = 0;

        skinColourImg.sprite = ResourceLibrary.Singleton.skinColourUIImages[_skinColour];

        def.skinColour = _skinColour;
        charPreviewScript.OnCharacterDefChange();

    }

    public void OnOKClick()
    {
        EOManager.Singleton.packetManager.Register(typeof(CharacterCreateResponse), OnCharacterCreateResp, OnCharacterCreateTimeout, 3.0f);
        EOManager.Singleton.CreateCharacter(charNameInput.text, _gender, _hairStyle, _hairColour, _skinColour);
    }

    public void OnCancelClick()
    {
        Destroy(this.gameObject);
    }

    public void OnCharacterCreateResp(Packet packet)
    {
        CharacterCreateResponse resp = packet as CharacterCreateResponse;

        CHARACTER_CREATE_RESP response = (CHARACTER_CREATE_RESP)resp.response;

        switch (response)
        {
            case CHARACTER_CREATE_RESP.SUCCESS:
                UIManager.Singleton.ShowGameDialog("Character Created", "Character successfully created");
                Destroy(this.gameObject);
                break;

            case CHARACTER_CREATE_RESP.NAME_TAKEN:
                UIManager.Singleton.ShowGameDialog("Name Taken", "Character name taken");
                break;

            case CHARACTER_CREATE_RESP.SERVER_ERROR:
                UIManager.Singleton.ShowGameDialog("Server Error", UIManager.serverFailRequestMsg);
                break;

            default:
                UIManager.Singleton.ShowGameDialog("Error", "There was an error. Please try again.");
                break;
        }
    }

    public void OnCharacterCreateTimeout()
    {
        UIManager.Singleton.ShowGameDialog("Server Error", UIManager.serverFailRequestMsg);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
