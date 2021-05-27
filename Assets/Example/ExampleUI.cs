using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleUI : MonoBehaviour, IComponentConnector {
    [ComponentConnect] public Text someText;
    [ComponentConnect] public Button someButton;

    [OnClick("SomeButton")]
    public void OnClickSomeButton() {
        someText.text = "Button pressed!";
    }
}
