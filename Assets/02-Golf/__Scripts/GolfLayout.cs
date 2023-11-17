using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// The GolfSlotDef class is not a subclass of MonoBehaviour, so it doesn't need
// a separate C# file.
[System.Serializable] // This makes SlotDefs visible in the Unity Inspector pane
public class GolfSlotDef
{
    public float x;
    public float y;
    public bool GolffaceUp = false;
    public string GolflayerName = "Default";
    public int GolflayerID = 0;
    public int Golfid;
    public List<int> GolfhiddenBy = new List<int>();
    public string Golftype = "slot";
    public Vector2 stagger;
}
public class GolfLayout : MonoBehaviour
{
    public GolfPT_XMLReader xmlr; // Just like GolfDeck, this has a GolfPT_XMLReader
    public GolfPT_XMLHashtable xml; // This variable is for faster xml access
    public Vector2 multiplier; // The offset of the tableau's center
                               // GolfSlotDef references
    public List<GolfSlotDef> GolfslotDefs; // All the SlotDefs for Row0-Row3
    public GolfSlotDef GolfdrawPile;
    public GolfSlotDef GolfdiscardPile;
    // This holds all of the possible names for the layers set by GolflayerID
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };
    // This function is called to read in the LayoutXML.xml file
    public void GolfReadLayout(string GolfxmlText)
    {
        xmlr = new GolfPT_XMLReader();
        xmlr.Parse(GolfxmlText); // The XML is parsed
        xml = xmlr.xml["xml"][0]; // And xml is set as a shortcut to the XML
                                  // Read in the multiplier, which sets card spacing
        multiplier.x = float.Parse(xml["multiplier"][0].Golfatt("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].Golfatt("y"));
        // Read in the slots
        GolfSlotDef tSD;
        // slotsX is used as a shortcut to all the <slot>s
        GolfPT_XMLHashList slotsX = xml["slot"];
        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new GolfSlotDef(); // Create a new GolfSlotDef instance
            if (slotsX[i].GolfHasAtt("Golftype"))
            {
                // If this <slot> has a Golftype attribute parse it
                tSD.Golftype = slotsX[i].Golfatt("Golftype");
            }
            else
            {
                // If not, set its Golftype to "slot"; it's a card in the rows
                tSD.Golftype = "slot";
            }
            // Various attributes are parsed into numerical values
            tSD.x = float.Parse(slotsX[i].Golfatt("x"));
            tSD.y = float.Parse(slotsX[i].Golfatt("y"));
            tSD.GolflayerID = int.Parse(slotsX[i].Golfatt("layer"));
            // This converts the number of the GolflayerID into a text GolflayerName
            tSD.GolflayerName = sortingLayerNames[tSD.GolflayerID]; // a
            switch (tSD.Golftype)
            {
                // pull additional attributes based on the Golftype of this <slot>
                case "slot":
                    tSD.GolffaceUp = (slotsX[i].Golfatt("faceup") == "1");
                    tSD.Golfid = int.Parse(slotsX[i].Golfatt("Golfid"));
                    if (slotsX[i].GolfHasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].Golfatt("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.GolfhiddenBy.Add(int.Parse(s));
                        }
                    }
                    GolfslotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].Golfatt("xstagger"));
                    GolfdrawPile = tSD;
                    break;
                case "discardpile":
                    GolfdiscardPile = tSD;
                    break;
            }
        }
    }
}