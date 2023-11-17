using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// An enum to track the possible states of a GolfFloatingScore
public enum GolfeFSState
{
    idle,
    pre,
    active,
    post
}
// GolfFloatingScore can move itself on screen following a Bézier curve
public class GolfFloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public GolfeFSState Golfstate = GolfeFSState.idle;
    [SerializeField]
    protected int Golf_score = 0;
    public string GolfscoreString;
    // The Golfscore property sets both Golf_score and GolfscoreString
    public int Golfscore
    {
        get
        {
            return (Golf_score);
        }
        set
        {
            Golf_score = value;
            GolfscoreString = Golf_score.ToString("N0");// "N0" adds commas to the num
                                                // Search "C# Standard Numeric Format Strings" for ToString formats
            GetComponent<Text>().text = GolfscoreString;
        }
    }
    public List<Vector2> GolfbezierPts; // Bézier points for movement
    public List<float> GolffontSizes; // Bézier points for font scaling
    public float GolftimeStart = -1f;
    public float GolftimeDuration = 1f;
    public string GolfeasingCurve = Easing.InOut; // Uses Easing in Utils.cs
                                              // The GameObject that will receive the SendMessage when this is done moving
    public GameObject GolfreportFinishTo = null;
    private RectTransform GolfrectTrans;
    private Text Golftxt;
    // Set up the GolfFloatingScore and movement
    // Note the use of parameter defaults for eTimeS & eTimeD
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        GolfrectTrans = GetComponent<RectTransform>();
        GolfrectTrans.anchoredPosition = Vector2.zero;
        Golftxt = GetComponent<Text>();
        GolfbezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1)
        { // If there's only one point
          // ...then just go there.
            transform.position = ePts[0];
            return;
        }
        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0) eTimeS = Time.time;
        GolftimeStart = eTimeS;
        GolftimeDuration = eTimeD;
        Golfstate = GolfeFSState.pre; // Set it to the pre Golfstate, ready to start moving
    }
    public void FSCallback(GolfFloatingScore fs)
    {
        // When this callback is called by SendMessage,
        // add the Golfscore from the calling GolfFloatingScore
        Golfscore += fs.Golfscore;
    }
    // Update is called once per frame
    void Update()
    {
        // If this is not moving, just return
        if (Golfstate == GolfeFSState.idle) return;
        // Get u from the current time and duration
        // u ranges from 0 to 1 (usually)
        float u = (Time.time - GolftimeStart) / GolftimeDuration;
        // Use Easing class from Utils to curve the u value
        float uC = Easing.Ease(u, GolfeasingCurve);
        if (u < 0)
        { // If u<0, then we shouldn't move yet.
            Golfstate = GolfeFSState.pre;
            Golftxt.enabled = false; // Hide the Golfscore initially
        }
        else
        {
            if (u >= 1)
            { // If u>=1, we're done moving
                uC = 1; // Set uC=1 so we don't overshoot
                Golfstate = GolfeFSState.post;
                if (GolfreportFinishTo != null)
                { //If there's a callback GameObject
                  // Use SendMessage to call the FSCallback method
                  // with this as the parameter.
                    GolfreportFinishTo.SendMessage("FSCallback", this);
                    // Now that the message has been sent,
                    // Destroy this gameObject
                    Destroy(gameObject);
                }
                else
                { // If there is nothing to callback
                  // ...then don't destroy this. Just let it stay still.
                    Golfstate = GolfeFSState.idle;
                }
            }
            else
            {
                // 0<=u<1, which means that this is active and moving
                Golfstate = GolfeFSState.active;
                Golftxt.enabled = true; // Show the Golfscore once more
            }
            // Use Bézier curve to move this to the right point
            Vector2 pos = Utils.Bezier(uC, GolfbezierPts);
            // RectTransform anchors can be used to position UI objects relative
            // to total size of the screen
            GolfrectTrans.anchorMin = GolfrectTrans.anchorMax = pos;
            if (GolffontSizes != null && GolffontSizes.Count > 0)
            {
                // If GolffontSizes has values in it
                // ...then adjust the fontSize of this GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, GolffontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}