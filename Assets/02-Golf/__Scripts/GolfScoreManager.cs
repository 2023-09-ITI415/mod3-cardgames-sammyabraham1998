using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// An enum to handle all the possible scoring events
public enum GolfeScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
// GolfScoreManager handles all of the scoring
public class GolfScoreManager : MonoBehaviour
{ // a
    static private GolfScoreManager GolfS; // b
    static public int GolfSCORE_FROM_PREV_ROUND = 0;
    static public int GolfHIGH_SCORE = 0;
    [Header("Set Dynamically")]
    // Fields to track
    //
    //
    //
    // info
    public int Golfchain = 0;
    public int GolfscoreRun = 0;
    public int Golfscore = 0;
    void Awake()
    {
        if (GolfS == null)
        { // c
            GolfS = this; // Set the private singleton
        }
        else
        {
            Debug.LogError("ERROR: GolfScoreManager.Awake(): S is already set!");
        }
        // Check for a high score in PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            GolfHIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        // Add the score from last round, which will be >0 if it was a win
        Golfscore += GolfSCORE_FROM_PREV_ROUND;
        // And reset the GolfSCORE_FROM_PREV_ROUND
        GolfSCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(GolfeScoreEvent evt)
    { // d
        try
        { // try-catch stops an error from breaking your program
            GolfS.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("GolfScoreManager:EVENT() called while S=null.\n" + nre );
        }
    }
    void Event(GolfeScoreEvent evt)
    {
        switch (evt)
        {
            // Same things need to happen whether it's a draw, a win, or a loss
            case GolfeScoreEvent.draw: // Drawing a card
            case GolfeScoreEvent.gameWin: // Won the round
            case GolfeScoreEvent.gameLoss: // Lost the round
                Golfchain = 0; // resets the score Golfchain
                Golfscore += GolfscoreRun; // add GolfscoreRun to total score
                GolfscoreRun = 0; // reset GolfscoreRun
                break;
            case GolfeScoreEvent.mine: // Remove a mine card
                Golfchain++; // increase the score Golfchain
                GolfscoreRun += Golfchain; // add score for this card to run
                break;
        }
        // This second switch statement handles round wins and losses
        switch (evt)
        {
            case GolfeScoreEvent.gameWin:
                // If it's a win, add the score to the next round
                // static fields are NOT reset by SceneManager.LoadScene()
                GolfSCORE_FROM_PREV_ROUND = Golfscore;
                print("You won this round! Round score: " + Golfscore);
                break;
            case GolfeScoreEvent.gameLoss:
                // If it's a loss, check against the high score
                if (GolfHIGH_SCORE <= Golfscore)
                {
                    print("You got the high score! High score: " + Golfscore);
                    GolfHIGH_SCORE = Golfscore;
                    PlayerPrefs.SetInt("ProspectorHighScore", Golfscore);
                }
                else
                {
                    print("Your final score for the game was: " + Golfscore);
                }
                break;
            default:
                print("score: " + Golfscore + " GolfscoreRun:" + GolfscoreRun + " Golfchain:" + Golfchain);
                break;
        }
    }
    static public int CHAIN { get { return GolfS.Golfchain; } } // e
    static public int SCORE { get { return GolfS.Golfscore; } }
    static public int SCORE_RUN { get { return GolfS.GolfscoreRun; } }
}