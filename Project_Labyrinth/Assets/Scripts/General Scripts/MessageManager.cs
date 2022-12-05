using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MessageManager : MonoBehaviour
{
    public static MessageManager instance;
    [SerializeField] public GameObject messagePrefab;
    [SerializeField] public GameObject objectivePrefab;
    public List<GameObject> messages;
    public GameObject messageBox;
    public GameObject objectiveBox;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        messageBox = GameObject.FindWithTag("MessageBox");
        objectiveBox = GameObject.FindWithTag("ObjectiveBox");
    }

    void Start()
    {
        
    }

    public void DisplayMessage(String message, Color textColor)
    {
        GameObject newMessage = Instantiate(messagePrefab, messageBox.transform);
        var newMessageText = newMessage.GetComponent<Text>();
        newMessageText.text = message;
        newMessageText.color = textColor;

        if (messages.Count >= 6)
        {
            messages.RemoveAt(messages.Count-1);
            Destroy(messages[messages.Count-1]);
        }
        
        messages.Add(newMessage);
        
    }
    
    public void AddObjective(String message, Color textColor)
    {
        GameObject newMessage = Instantiate(objectivePrefab, objectiveBox.transform);
        var newMessageText = newMessage.GetComponent<Text>();
        newMessageText.text = message;
        newMessageText.color = textColor;
        
        messages.Add(newMessage);
        
    }
    
    public void UpdateObjective(String newMessage, String objectiveTitle)
    {
        foreach (GameObject message in objectiveBox.transform)
        {
            var messageTextComponent = message.GetComponent<Text>();
            
            if (messageTextComponent == null)
                continue;
            
            if (messageTextComponent.text.Contains("objectiveTitle"))
            {
                messageTextComponent.text = objectiveTitle + ": " + newMessage;
            }
        }
        

        
    }

}
