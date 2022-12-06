using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class Elevator : Interactable
{

    private bool unlocked = false;
    
    public override void Unlock()
    {
        unlocked = true;
    }

    public override void DoAction()
    {
        TheAction();
    }

    void TheAction()
    {
        if (!unlocked)
        {
            MessageManager.instance.DisplayMessage("You need a key for this!", Color.yellow);
            return;
        }

        GameDirector.instance.NextLevel();
    }
}
