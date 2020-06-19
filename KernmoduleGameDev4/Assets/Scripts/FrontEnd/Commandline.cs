using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System.Reflection;
using TMPro;

public class Commandline : MonoBehaviour
{
    private List<string> outputLines = new List<string>();

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private int maxLineAmount;

    [SerializeField]
    private TextMeshProUGUI outputText;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject)
            {
                EnterCommand(inputField);
            }
        }
    }

    public void EnterCommand(TMP_InputField inputField)
    {
        string _line = inputField.text;

        inputField.text = "";

        string[] _args = _line.Split(' ');
        string _cmd = _args[0];
        bool _unknownCommand = false;

        switch(_cmd)
        {
            default:
                _unknownCommand = true;
                AddNewLine($"Unrecognised command: {_cmd}");
                break;
            case "host":
                (new GameObject()).AddComponent<ServerBehaviour>();
                break;
            case "connect":
                (new GameObject()).AddComponent<ClientBehaviour>();
                break;
        }

        if (!_unknownCommand) { AddNewLine(_line); }
            
        EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
        inputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void AddNewLine(string _newLine)
    {
        outputLines.Add(_newLine);
        if(outputLines.Count > maxLineAmount) {  outputLines.RemoveAt(0); }

        outputText.text  = "";
        foreach (var _line in outputLines)
        {
            outputText.text += $"{_line}\n";
        }
    }
}
