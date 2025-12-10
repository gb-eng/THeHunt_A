using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    [Header("Card Settings")]
    public int cardID;
    private Sprite cardFace;
    private Sprite cardBack;

    private Image cardImage; // ✅ Changed to Image for Canvas
    private Button button;   // ✅ Changed to Button for Clicking
    private bool isFlipped = false;
    private bool isMatched = false;
    
    void Awake()
    {
        cardImage = GetComponent<Image>();
        button = GetComponent<Button>();
        
        if(button != null) 
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetupCard(int id, Sprite face, Sprite back)
    {
        cardID = id;
        cardFace = face;
        cardBack = back;
        
        // Show Back initially
        if (cardImage != null) cardImage.sprite = cardBack;
    }

    public void OnClick()
    {
        if (isMatched || isFlipped) return;
        if (HuntMaster.Instance != null && HuntMaster.Instance.CanFlipCard())
        {
            FlipCard();
        }
    }

    public void FlipCard()
    {
        isFlipped = true;
        if (cardImage != null) cardImage.sprite = cardFace;
        if (HuntMaster.Instance != null) HuntMaster.Instance.CardFlipped(this);
    }

    public void FlipBack()
    {
        if (!isFlipped || isMatched) return;
        isFlipped = false;
        if (cardImage != null) cardImage.sprite = cardBack;
    }

    public void SetMatched()
    {
        isMatched = true;
        // Fade out to show it's done
        if (cardImage != null)
        {
            Color c = cardImage.color;
            c.a = 0.5f;
            cardImage.color = c;
        }
        if (button != null) button.interactable = false;
    }

    public int GetCardID() => cardID;
}