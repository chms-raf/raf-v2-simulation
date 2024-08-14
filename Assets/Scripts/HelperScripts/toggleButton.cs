/// |-------------------------------------------Toggle Button-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class controls the button the GUI to change the off and on text and the sprite if its 
///              interacted with.
/// |-------------------------------------------------------------------------------------------------------------|

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class toggleButton : MonoBehaviour {
    // Accessible Properties
    [SerializeField] public string offText = "";
    [SerializeField] public string onText = "";
    [SerializeField] public Sprite offSprite;
    [SerializeField] public Sprite onSprite;
    [SerializeField] public TextMeshProUGUI textObj;
    [SerializeField] public GameObject gameObj;
    [HideInInspector] public bool selected = false;
    
    // Private variables
    private int counter = 0; // Determines if it is on or off
    private Image img;

    // Start is called before the first frame update
    void Start() {
        img = this.gameObject.GetComponent<Image>();
    }

    // Toggle the button
    public void buttonToggle() {
        counter++;
        if (counter % 2 == 0) {
            selected = false;
            textObj.text = offText;
            if (gameObj != null) { gameObj.SetActive(false); }
            img.sprite = offSprite;
        } else {
            selected = true;
            textObj.text = onText;
            if (gameObj != null) { gameObj.SetActive(true); }
            img.sprite = onSprite;
        }
    }
}
