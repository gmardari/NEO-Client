using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TabInputField : MonoBehaviour
{
    public TMP_InputField[] fields;
    public UnityEvent onAccept;

    private int inputSelected;



    void Awake()
    {
        for (int i = 0; i < fields.Length; i++)
        {
            int b = i;
            fields[i].onSelect.AddListener((s) => OnInputSelected(b));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                inputSelected--;

                if (inputSelected < 0)
                    inputSelected = fields.Length - 1;

                SelectInputField();
            }
            else
            {
                inputSelected++;

                if (inputSelected > fields.Length - 1)
                    inputSelected = 0;

                SelectInputField();
            }
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(onAccept != null)
                onAccept.Invoke();
        }

      
    }

    private void SelectInputField()
    {
        fields[inputSelected].Select();
    }

    public void OnInputSelected(int i)
    {
        inputSelected = i;
    }

}

