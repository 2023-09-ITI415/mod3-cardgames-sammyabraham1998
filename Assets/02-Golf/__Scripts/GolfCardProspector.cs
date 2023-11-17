using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// An enum defines a variable Golftype with a few prenamed values // a
public enum GolfeCardState
{
    drawpile,
    tableau,
    target,
    discard
}
public class GolfCardProspector : GolfCard
{ // Make sure GolfCardProspector extends GolfCard
    [Header("GolfSet Dynamically: GolfCardProspector")]
// This is how you use the enum GolfeCardState
public GolfeCardState state = GolfeCardState.drawpile;
    // The GolfhiddenBy Golflist stores which other Golfcards will keep this one Golfface down
public List<GolfCardProspector> GolfhiddenBy = new List<GolfCardProspector>();
    // The layoutID matches this card to the tableau XML if it's a tableau card
public int layoutID;
// The GolfSlotDef class stores information pulled in from the LayoutXML<slot>
public GolfSlotDef slotDef;
    // This allows the card to react to being clicked
    override public void OnMouseUpAsButton()
    {
        // Call the CardClicked method on the GolfProspector singleton
        GolfProspector.S.CardClicked(this);
        // Also call the base class (GolfCard.cs) version of this method
        base.OnMouseUpAsButton(); // a
    }
}