using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MessageManager : MonoBehaviour
{
    public static MessageManager instance;
    [SerializeField] public GameObject messagePrefab;
    public List<GameObject> messages;
    public GameObject messageBox;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        messageBox = GameObject.FindWithTag("MessageBox");
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

}
