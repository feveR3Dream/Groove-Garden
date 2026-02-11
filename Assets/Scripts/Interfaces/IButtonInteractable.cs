using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButtonInteractable
{
    void ButtonInteracted(bool registered, MouseButton mouseButton);
}
