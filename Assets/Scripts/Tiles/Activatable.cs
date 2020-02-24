using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour
{
    public char type;

    public void Activate() => this.gameObject.SetActive(true);
}
