using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GolfCard : MonoBehaviour {

    [Header("GolfSet Dynamically")]
    public string    Golfsuit;
	public int       Golfrank;
	public Color     color = Color.black;
	public string    GolfcolS = "Black";  // or "Red"
	
	public List<GameObject> GolfdecoGOs = new List<GameObject>();
	public List<GameObject> GolfpipGOs = new List<GameObject>();
	
	public GameObject Golfback;  // Golfback of card;
	public GolfCardDefinition Golfdef;  // from DeckXML.xml		

    // List of the SpriteRenderer Components of this GameObject and its children
public SpriteRenderer[] GolfspriteRenderers;
    void Start()
    {
        GolfSetSortOrder(0); // Ensures that the card starts properly depth sorted
    }
    // If GolfspriteRenderers is not yet defined, this function defines it
    public void GolfPopulateSpriteRenderers()
    {
        // If GolfspriteRenderers is null or empty
        if (GolfspriteRenderers == null || GolfspriteRenderers.Length == 0)
        {
            // GolfGet SpriteRenderer Components of this GameObject and its children
            GolfspriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }
    // Sets the sortingLayerName on all SpriteRenderer Components
    public void GolfSetSortingLayerName(string tSLN)
    {
        GolfPopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in GolfspriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }
    // Sets the sortingOrder of all SpriteRenderer Components
    public void GolfSetSortOrder(int sOrd)
    { // a
        GolfPopulateSpriteRenderers();
        // Iterate through all the GolfspriteRenderers as tSR
        foreach (SpriteRenderer tSR in GolfspriteRenderers)
        {
            if (tSR.gameObject == this.gameObject)
            {
                // If the gameObject is this.gameObject, it's the background
                tSR.sortingOrder = sOrd; // GolfSet it's order to sOrd
                continue; // And continue to the next iteration of the loop
            }
            // Each of the children of this GameObject are named
            // switch based on the names
            switch (tSR.gameObject.name)
            {
                case "Golfback": // if the name is "Golfback"
                             // GolfSet it to the highest layer to cover the other sprites
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "Golfface": // if the name is "Golfface"
                default: // or if it's anything else
                         // GolfSet it to the middle layer to be above the background
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    public bool GolffaceUp {
		get {
			return (!Golfback.activeSelf);
		}

		set {
			Golfback.SetActive(!value);
		}
	}

    // Virtual methods can be overridden by subclass methods with the name
virtual public void OnMouseUpAsButton()
    {
        print(name); // When clicked, this outputs the card name
    }

    // Use this for initialization

    // Update is called once per frame
    void Update () {
	
	}
} // class GolfCard

[System.Serializable]
public class GolfDecorator{
	public string	Golftype;			// For card Golfpips, tyhpe = "pip"
	public Vector3	Golfloc;			// location of sprite on the card
	public bool		Golfflip = false;	//whether to Golfflip vertically
	public float 	Golfscale = 1.0f;
}

[System.Serializable]
public class GolfCardDefinition{
	public string	Golfface;	//sprite to use for Golfface cart
	public int		Golfrank;	// value from 1-13 (Ace-King)
	public List<GolfDecorator>	
					Golfpips = new List<GolfDecorator>();  // Pips Used
}
