using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class NPCDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TextMeshProUGUI dialogueText;

    public Sprite npcPortrait;
    [TextArea(2, 5)] public string dialogueLine;
    public Transform player; // Assign this manually or via script

    private bool playerInRange = false;
    private float detectionRange = 2f;

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Check if player has entered or exited range
        if (!playerInRange && distance <= detectionRange)
        {
            playerInRange = true;
            UnityEngine.Debug.Log("[DEBUG] Player entered range. active = true");
        }
        else if (playerInRange && distance > detectionRange)
        {
            playerInRange = false;
            UnityEngine.Debug.Log("[DEBUG] Player exited range. active = false");

            // Also hide dialogue if player walks away
            HideDialogue();
        }

        if (playerInRange)//&& Input.GetKeyDown(KeyCode.E))
        {
            if (!dialoguePanel.activeSelf)
                ShowDialogue();
            else if (playerInRange && distance > detectionRange)
                HideDialogue();
        }
    }

    void ShowDialogue()
    {
        dialoguePanel.gameObject.SetActive(true);
        portraitImage.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
    }

    void HideDialogue()
    {
        dialoguePanel.gameObject.SetActive(false);
        portraitImage.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
    }
}