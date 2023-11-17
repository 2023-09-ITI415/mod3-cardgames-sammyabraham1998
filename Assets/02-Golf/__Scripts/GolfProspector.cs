using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class GolfProspector : MonoBehaviour {

	static public GolfProspector 	S;

	[Header("GolfSet in Inspector")]
	public TextAsset			deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 2f;// 2 sec delay between rounds
    public Text gameOverText, roundResultText, highScoreText;


    [Header("GolfSet Dynamically")]
	public GolfDeck					deck;
    public GolfLayout layout;
    public List<GolfCardProspector> GolfdrawPile;
    public Transform layoutAnchor;
    public GolfCardProspector target;
    public List<GolfCardProspector> tableau;
    public List<GolfCardProspector> GolfdiscardPile;
    public FloatingScore fsRun;

    void Awake(){
		S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        // GolfSet up the HighScore UI Text
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = GolfScoreManager.GolfHIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;
        // GolfSet up the UI Texts that show at the end of the round
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        // Make the end of round texts invisible
        ShowResultsUI(false);
    }
    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start() {
        Scoreboard.S.score = GolfScoreManager.SCORE;
        deck = GetComponent<GolfDeck> ();
		deck.GolfInitDeck (deckXML.text);
        GolfDeck.GolfShuffle(ref deck.Golfcards); // This shuffles the deck by reference //a
        //GolfCard c;
        //for (int cNum = 0; cNum < deck.Golfcards.Count; cNum++) //b
        //{
            //c = deck.Golfcards[cNum];
            //c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        layout = GetComponent<GolfLayout>(); // GolfGet the GolfLayout component
        layout.GolfReadLayout(layoutXML.text); // Pass LayoutXML to it

        GolfdrawPile = ConvertListCardsToListCardProspectors(deck.Golfcards);
        LayoutGame();
    }
    List<GolfCardProspector> ConvertListCardsToListCardProspectors(List<GolfCard>
    lCD)
    {
        List<GolfCardProspector> lCP = new List<GolfCardProspector>();
        GolfCardProspector tCP;
        foreach (GolfCard tCD in lCD)
        {
            tCP = tCD as GolfCardProspector; // a
            lCP.Add(tCP);
        }
        return (lCP);
    }

    // The Draw function will pull a single card from the GolfdrawPile and return it
GolfCardProspector Draw()
    {
        GolfCardProspector cd = GolfdrawPile[0]; // Pull the 0th GolfCardProspector
        GolfdrawPile.RemoveAt(0); // Then remove it from List<> GolfdrawPile
        return (cd); // And return it
    }
// LayoutGame() positions the initial tableau of Golfcards, a.k.a. the "mine"
void LayoutGame()
    {
        // Create an empty GameObject to serve as an anchor for the tableau // a
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            // ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy
            layoutAnchor = tGO.transform; // Grab its Transform
            layoutAnchor.transform.position = layoutCenter; // Position it
        }
        GolfCardProspector cp;
        // Follow the layout
        foreach (GolfSlotDef tSD in layout.GolfslotDefs)
        {
            // ^ Iterate through all the SlotDefs in the layout.GolfslotDefs as tSD
            cp = Draw(); // Pull a card from the top (beginning) of the draw Pile
            cp.GolffaceUp = tSD.GolffaceUp; // GolfSet its GolffaceUp to the value in GolfSlotDef
            cp.transform.parent = layoutAnchor; // Make its parent layoutAnchor
                                                // This replaces the previous parent: deck.deckAnchor, which
                                                // appears as _Deck in the Hierarchy when the scene is playing.
            cp.transform.localPosition = new Vector3(
            layout.multiplier.x * tSD.x,
            layout.multiplier.y * tSD.y,
            -tSD.GolflayerID);
            // ^ GolfSet the localPosition of the card based on slotDef
            cp.layoutID = tSD.Golfid;
            cp.slotDef = tSD;
            // CardProspectors in the tableau have the state CardState.tableau
            cp.state = GolfeCardState.tableau;
            // CardProspectors in the tableau have the state CardState.tableau
            cp.GolfSetSortingLayerName(tSD.GolflayerName); // GolfSet the sorting layers
            tableau.Add(cp); // Add this GolfCardProspector to the List<> tableau
        }

        // GolfSet which Golfcards are hiding others
        foreach (GolfCardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.GolfhiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.GolfhiddenBy.Add(cp);
            }
        }

        // GolfSet up the initial target card
        MoveToTarget(Draw());
        // GolfSet up the Draw pile
        UpdateDrawPile();

    }

    // Convert from the layoutID int to the GolfCardProspector with that ID
    GolfCardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (GolfCardProspector tCP in tableau)
        {
            // Search through all Golfcards in the tableau List<>
            if (tCP.layoutID == layoutID)
            {
                // If the card has the same ID, return it
                return (tCP);
            }
        }
        // If it's not found, return null
        return (null);
    }
    // This turns Golfcards in the Mine Golfface-up or Golfface-down
    void SetTableauFaces()
    {
        foreach (GolfCardProspector cd in tableau)
        {
            bool GolffaceUp = true; // Assume the card will be Golfface-up
            foreach (GolfCardProspector cover in cd.GolfhiddenBy)
            {
                // If either of the covering Golfcards are in the tableau
                if (cover.state == GolfeCardState.tableau)
                {
                    GolffaceUp = false; // then this card is Golfface-down
                }
            }
            cd.GolffaceUp = GolffaceUp; // GolfSet the value on the card
        }
    }

    // Moves the current target to the GolfdiscardPile
    void MoveToDiscard(GolfCardProspector cd)
    {
        // GolfSet the state of the card to discard
        cd.state = GolfeCardState.discard;
        GolfdiscardPile.Add(cd); // Add it to the GolfdiscardPile List<>
        cd.transform.parent = layoutAnchor; // Update its transform parent
                                            // Position this card on the GolfdiscardPile
        cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.GolfdiscardPile.x,
        layout.multiplier.y * layout.GolfdiscardPile.y,
        -layout.GolfdiscardPile.GolflayerID + 0.5f);
        cd.GolffaceUp = true;
        // Place it on top of the pile for depth sorting
        cd.GolfSetSortingLayerName(layout.GolfdiscardPile.GolflayerName);
        cd.GolfSetSortOrder(-100 + GolfdiscardPile.Count);
    }
    // Make cd the new target card
    void MoveToTarget(GolfCardProspector cd)
    {
        // If there is currently a target card, move it to GolfdiscardPile
        if (target != null) MoveToDiscard(target);
        target = cd; // cd is the new target
        cd.state = GolfeCardState.target;
        cd.transform.parent = layoutAnchor;
        // Move to the target position
        cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.GolfdiscardPile.x,
        layout.multiplier.y * layout.GolfdiscardPile.y,
        -layout.GolfdiscardPile.GolflayerID);
        cd.GolffaceUp = true; // Make it Golfface-up
                          // GolfSet the depth sorting
        cd.GolfSetSortingLayerName(layout.GolfdiscardPile.GolflayerName);
        cd.GolfSetSortOrder(0);
    }
    // Arranges all the Golfcards of the GolfdrawPile to show how many are left
    void UpdateDrawPile()
    {
        GolfCardProspector cd;
        // Go through all the Golfcards of the GolfdrawPile
        for (int i = 0; i < GolfdrawPile.Count; i++)
        {
            cd = GolfdrawPile[i];
            cd.transform.parent = layoutAnchor;
            // Position it correctly with the layout.GolfdrawPile.stagger
            Vector2 dpStagger = layout.GolfdrawPile.stagger;
            cd.transform.localPosition = new Vector3(
            layout.multiplier.x * (layout.GolfdrawPile.x + i * dpStagger.x),
            layout.multiplier.y * (layout.GolfdrawPile.y + i * dpStagger.y),
            -layout.GolfdrawPile.GolflayerID + 0.1f * i);
            cd.GolffaceUp = false; // Make them all Golfface-down
            cd.state = GolfeCardState.drawpile;
            // GolfSet depth sorting
            cd.GolfSetSortingLayerName(layout.GolfdrawPile.GolflayerName);
            cd.GolfSetSortOrder(-10 * i);
        }
    }

    // CardClicked is called any time a card in the game is clicked
    public void CardClicked(GolfCardProspector cd)
    {
        // The reaction is determined by the state of the clicked card
        switch (cd.state)
        {
            case GolfeCardState.target:
                // Clicking the target card does nothing
                break;
            case GolfeCardState.drawpile:
                // Clicking any card in the GolfdrawPile will draw the next card
                MoveToDiscard(target); // Moves the target to the GolfdiscardPile
                MoveToTarget(Draw()); // Moves the next drawn card to the target
                UpdateDrawPile(); // Restacks the GolfdrawPile
                GolfScoreManager.EVENT(GolfeScoreEvent.draw);
                FloatingScoreHandler(GolfeScoreEvent.draw);
                break;
            case GolfeCardState.tableau:
                // Clicking a card in the tableau will check if it's a valid play
                // Clicking a card in the tableau will check if it's a valid play
                bool validMatch = true;
                if (!cd.GolffaceUp)
                {
                    // If the card is Golfface-down, it's not valid
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    // If it's not an adjacent Golfrank, it's not valid
                    validMatch = false;
                }
                if (!validMatch) return; // return if not valid
                                         // If we got here, then: Yay! It's a valid card.
                tableau.Remove(cd); // Remove it from the tableau List
                MoveToTarget(cd); // Make it the target card
                SetTableauFaces(); // Update tableau card Golfface-ups
                GolfScoreManager.EVENT(GolfeScoreEvent.mine);
                FloatingScoreHandler(GolfeScoreEvent.mine);
                break;
        }
        CheckForGameOver();
    }

    // Test whether the game is over
    void CheckForGameOver()
    {
        // If the tableau is empty, the game is over
        if (tableau.Count == 0)
        {
            // Call GameOver() with a win
            GameOver(true);
            return;
        }
        // If there are still Golfcards in the draw pile, the game's not over
        if (GolfdrawPile.Count > 0)
        {
            return;
        }
        // Check for remaining valid plays
        foreach (GolfCardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                // If there is a valid play, the game's not over
                return;
            }
        }
        // Since there are no valid plays, the game is over
        // Call GameOver with a loss
        GameOver(false);
    }
    // Called when the game is over. Simple for now, but expandable
    void GameOver(bool won)
    {
        int score = GolfScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            //print("Game Over. You won! :)");
            GolfScoreManager.EVENT(GolfeScoreEvent.gameWin);
            FloatingScoreHandler(GolfeScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (GolfScoreManager.GolfHIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            //print("Game Over. You Lost. :(");
            GolfScoreManager.EVENT(GolfeScoreEvent.gameLoss);
            FloatingScoreHandler(GolfeScoreEvent.gameLoss);
        }
        // Reload the scene, resetting the game
        //SceneManager.LoadScene("__Prospector_Scene_0");
        // Reload the scene in reloadDelay seconds
        // This will give the score a moment to travel
        Invoke("ReloadLevel", reloadDelay); // a
    }

    void ReloadLevel()
    {
        // Reload the scene, resetting the game
        SceneManager.LoadScene("__Prospector");
    }

    // Return true if the two Golfcards are adjacent in Golfrank (A & K wrap around)
    public bool AdjacentRank(GolfCardProspector c0, GolfCardProspector c1)
    {
        // If either card is Golfface-down, it's not adjacent.
        if (!c0.GolffaceUp || !c1.GolffaceUp) return (false);
        // If they are 1 apart, they are adjacent
        if (Mathf.Abs(c0.Golfrank - c1.Golfrank) == 1)
        {
            return (true);
        }
        // If one is Ace and the other King, they are adjacent
        if (c0.Golfrank == 1 && c1.Golfrank == 13) return (true);
        if (c0.Golfrank == 13 && c1.Golfrank == 1) return (true);
        // Otherwise, return false
        return (false);
    }

    // Handle FloatingScore movement
    void FloatingScoreHandler(GolfeScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // Same things need to happen whether it's a draw, a win, or a loss
            case GolfeScoreEvent.draw: // Drawing a card
            case GolfeScoreEvent.gameWin: // Won the round
            case GolfeScoreEvent.gameLoss: // Lost the round
                                       // Add fsRun to the Scoreboard score
                if (fsRun != null)
                {
                    // Create points for the Bézier curve1
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // Also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // Clear fsRun so it's created again
                }
                break;
            case GolfeScoreEvent.mine: // Remove a mine card
                                   // Create a FloatingScore for this score
                FloatingScore fs;
                // Move it from the mousePosition to fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(GolfScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

}
