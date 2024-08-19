using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    public TMP_Text tmp;

    public void Show(string text) {
        tmp.text = text;
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
}
