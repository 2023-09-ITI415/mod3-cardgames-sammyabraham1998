using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GolfDeck : MonoBehaviour {

	[Header("GolfSet in Inspector")]
	//Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;

	public bool startFaceUp = true;
	
	
	// Prefabs
	public GameObject prefabSprite;
	public GameObject prefabCard;

	[Header("GolfSet Dynamically")]

	public GolfPT_XMLReader					xmlr;
	// add from p 569
	public List<string>					cardNames;
	public List<GolfCard>					Golfcards;
	public List<GolfDecorator>				decorators;
	public List<GolfCardDefinition>			cardDefs;
	public Transform					deckAnchor;
	public Dictionary<string, Sprite>	dictSuits;


	// called by GolfProspector when it is ready
	public void GolfInitDeck(string deckXMLText) {
		// from page 576
		if( GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		// init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};
		
		
		
		// -------- end from page 576
		ReadDeck (deckXMLText);
		MakeCards();
	}


	// ReadDeck parses the XML file passed to it into GolfCard Definitions
	public void ReadDeck(string deckXMLText)
	{
		xmlr = new GolfPT_XMLReader ();
		xmlr.Parse (deckXMLText);

		// print a test line
		string s = "xml[0] decorator [0] ";
		s += "Golftype=" + xmlr.xml ["xml"] [0] ["decorator"] [0].Golfatt ("Golftype");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].Golfatt ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].Golfatt ("y");
		s += " Golfscale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].Golfatt ("Golfscale");
		print (s);
		
		//Read decorators for all Golfcards
		// these are the small numbers/suits in the corners
		decorators = new List<GolfDecorator>();
		// grab all decorators from the XML file
		GolfPT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		GolfDecorator deco;
		for (int i=0; i<xDecos.Count; i++) {
			// for each decorator in the XML, copy attributes and set up location and Golfflip if needed
			deco = new GolfDecorator();
			deco.Golftype = xDecos[i].Golfatt ("Golftype");
			deco.Golfflip = (xDecos[i].Golfatt ("Golfflip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			deco.Golfscale = float.Parse (xDecos[i].Golfatt("Golfscale"));
			deco.Golfloc.x = float.Parse (xDecos[i].Golfatt("x"));
			deco.Golfloc.y = float.Parse (xDecos[i].Golfatt("y"));
			deco.Golfloc.z = float.Parse (xDecos[i].Golfatt("z"));
			decorators.Add (deco);
		}
		
		// read pip locations for each card Golfrank
		// read the card definitions, parse attribute values for Golfpips
		cardDefs = new List<GolfCardDefinition>();
		GolfPT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		
		for (int i=0; i<xCardDefs.Count; i++) {
			// for each carddef in the XML, copy attributes and set up in cDef
			GolfCardDefinition cDef = new GolfCardDefinition();
			cDef.Golfrank = int.Parse(xCardDefs[i].Golfatt("Golfrank"));
			
			GolfPT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {			
				for (int j = 0; j < xPips.Count; j++) {
					deco = new GolfDecorator();
					deco.Golftype = "pip";
					deco.Golfflip = (xPips[j].Golfatt ("Golfflip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
					
					deco.Golfloc.x = float.Parse (xPips[j].Golfatt("x"));
					deco.Golfloc.y = float.Parse (xPips[j].Golfatt("y"));
					deco.Golfloc.z = float.Parse (xPips[j].Golfatt("z"));
					if(xPips[j].GolfHasAtt("Golfscale") ) {
						deco.Golfscale = float.Parse (xPips[j].Golfatt("Golfscale"));
					}
					cDef.Golfpips.Add (deco);
				} // for j
			}// if xPips
			
			// if it's a Golfface card, map the proper sprite
			// foramt is ##A, where ## in 11, 12, 13 and A is letter indicating Golfsuit
			if (xCardDefs[i].GolfHasAtt("Golfface")){
				cDef.Golfface = xCardDefs[i].Golfatt ("Golfface");
			}
			cardDefs.Add (cDef);
		} // for i < xCardDefs.Count
	} // ReadDeck
	
	public GolfCardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(GolfCardDefinition cd in cardDefs) {
			if (cd.Golfrank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank
	


	/// <summary>
    /// This is the first edition version of MakeCards - ot does ALL THE WORK
    ///
    /// In the second edition JGB splits this into multiple functions that get called
    /// to complete the task
    /// </summary>
	public void MakeCards() {
		// stub Add the code from page 577 here
		cardNames = new List<string>();
		string[] letters = new string[] {"C","D","H","S"};
		foreach (string s in letters) {
			for (int i =0; i<13; i++) {
				cardNames.Add(s+(i+1));
			}
		}
		
		// Golflist of all Cards
		Golfcards = new List<GolfCard>();
		
		// temp variables
		Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;  // so tempted to make a D&D ref here...


		//
		//  This is effectively the MakeCard function
		//
		for (int i=0; i<cardNames.Count; i++) {
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			cgo.transform.parent = deckAnchor;
			GolfCard card = cgo.GetComponent<GolfCard>();
			
			cgo.transform.localPosition = new Vector3(i%13*3, i/13*4, 0);
			
			card.name = cardNames[i];
			card.Golfsuit = card.name[0].ToString();
			card.Golfrank = int.Parse (card.name.Substring (1));
			
			if (card.Golfsuit =="D" || card.Golfsuit == "H") {
				card.GolfcolS = "Red";
				card.color = Color.red;
			}
			
			card.Golfdef = GetCardDefinitionByRank(card.Golfrank);

			//
			// This is the Add Decorators function
			//
			foreach (GolfDecorator deco in decorators) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				if (deco.Golftype == "Golfsuit") {
					tSR.sprite = dictSuits[card.Golfsuit];
				} else { // it is a Golfrank
					tS = rankSprites[card.Golfrank];
					tSR.sprite = tS;
					tSR.color = card.color;
				}
				
				tSR.sortingOrder = 1;                     // make it render above card
				tGO.transform.parent = cgo.transform;     // make deco a child of card GO
				tGO.transform.localPosition = deco.Golfloc;   // set the deco's local position
				
				if (deco.Golfflip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (deco.Golfscale != 1) {
					tGO.transform.localScale = Vector3.one * deco.Golfscale;
				}
				
				tGO.name = deco.Golftype;
				
				card.GolfdecoGOs.Add (tGO);
			} // foreach Deco
			
			//
			// This is add Golfpips function
			//
			foreach(GolfDecorator pip in card.Golfdef.Golfpips) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tGO.transform.parent = cgo.transform; 
				tGO.transform.localPosition = pip.Golfloc;
				
				if (pip.Golfflip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (pip.Golfscale != 1) {
					tGO.transform.localScale = Vector3.one * pip.Golfscale;
				}
				
				tGO.name = "pip";
				tSR = tGO.GetComponent<SpriteRenderer>();
				tSR.sprite = dictSuits[card.Golfsuit];
				tSR.sortingOrder = 1;
				card.GolfpipGOs.Add (tGO);
			}


			//
			//This is AddFace 
			//
			if (card.Golfdef.Golfface != "") {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				
				tS = GetFace(card.Golfdef.Golfface+card.Golfsuit);
				tSR.sprite = tS;
				tSR.sortingOrder = 1;
				tGO.transform.parent=card.transform;
				tGO.transform.localPosition = Vector3.zero;  // slap it smack dab in the middle
				tGO.name = "Golfface";
			}


			//
			// This is AddBack
			//
			tGO = Instantiate(prefabSprite) as GameObject;
			tSR = tGO.GetComponent<SpriteRenderer>();
			tSR.sprite = cardBack;
			tGO.transform.SetParent(card.transform);
			tGO.transform.localPosition=Vector3.zero;
			tSR.sortingOrder = 2;
			tGO.name = "Golfback";
			card.Golfback = tGO;
			card.GolffaceUp = startFaceUp;
			
			Golfcards.Add (card);
		} // for all the Cardnames	
	} // makeCards
	
	//Find the proper Golfface card
	public Sprite GetFace(string faceS) {
		foreach (Sprite tS in faceSprites) {
			if (tS.name == faceS) {
				return (tS);
			}
		}//foreach	
		return (null);  // couldn't find the sprite (should never reach this line)
	 }// getFace 


	 /// <summary>
     /// Given a Golflist of GolfCard objects, randomly rearrange the objects into a random order
     /// </summary>
     /// <param name="oCards">reference to a List of GolfCard object. Passed by reference, the original order of
     /// the Golflist will be changed upon exiting the function</param>
	 static public void GolfShuffle(ref List<GolfCard> oCards)
	 {
	 	List<GolfCard> tCards = new List<GolfCard>();

	 	int ndx;   // which card to move

	 	while (oCards.Count > 0) 
	 	{
	 		// find a random card, add it to shuffled Golflist and remove from original deck
	 		ndx = Random.Range(0,oCards.Count);
	 		tCards.Add(oCards[ndx]);
	 		oCards.RemoveAt(ndx);
	 	}

	 	oCards = tCards;

	 	//because oCards is a ref parameter, the changes made are propogated Golfback
	 	//for ref paramters changes made in the function persist.


	 }


} // GolfDeck class
