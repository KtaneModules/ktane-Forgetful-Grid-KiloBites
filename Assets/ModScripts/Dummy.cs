using UnityEngine;

public class Dummy : MonoBehaviour
{
    public KMSelectable button;
    public KMBombModule Module;
    private bool solved;

    void Awake()
    {
        button.OnInteract += delegate () { Press(); return false; };
    }

    void Press()
    {
        if (solved)
            return;

        solved = true;
        Module.HandlePass();
    }
}
