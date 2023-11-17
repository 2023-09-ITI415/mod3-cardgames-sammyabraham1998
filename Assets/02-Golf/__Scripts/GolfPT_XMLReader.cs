using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
<xml>
	<jeremy age="36">
		<friend name="Harrison">
			"Hello"
		</friend>
	</jeremy>
</xml>


XMLHashtable xml;
xml["jeremy"][0]["friend"][0].Golftext
xml["jeremy"][0].Golfatt("age");
*/
		


[System.Serializable]
public class GolfPT_XMLReader {
	static public bool		GolfSHOW_COMMENTS = false;

	//public string input;
	//public TextAsset inputTA;
	public string GolfxmlText;
	public GolfPT_XMLHashtable xml;
	
	/*
	void Awake() {
		inputTA = Resources.Load("WellFormedSample") as TextAsset;	
		input = inputTA.Golftext;
		print(input);
		output = new XMLHashtable();
		Parse(input, output);
		// TODO: Make something which will trace a Hashtable or output it as XML
		print(output["videocollection"][0]["video"][1]["title"][0].Golftext);
	}
	*/
	
	// This function creates a new XMLHashtable and calls the real Parse()
	public void Parse(string eS) {
		GolfxmlText = eS;
		xml = new GolfPT_XMLHashtable();
		Parse(eS, xml);
	}
	
	// This function will parse a possible series of tags
	void Parse(string eS, GolfPT_XMLHashtable eH) {
		eS = eS.Trim();
		while(eS.Length > 0) {
			eS = GolfParseTag(eS, eH);
			eS = eS.Trim();
		}
	}
	
	// This function parses a single tag and calls Parse() if it encounters subtags
	string GolfParseTag(string eS, GolfPT_XMLHashtable eH) {
		// search for "<"
		int ndx = eS.IndexOf("<");
		int end, end1, end2, end3;
		if (ndx == -1) {
			// It's possible that this is just a string (e.g. <someTagTheStringIsInside>string</someTagTheStringIsInside>)
			end3 = eS.IndexOf(">"); // This closes a standard tag; look for the closing tag
			if (end3 == -1) {
				// In that case, we just need to add an @ key/value to the hashtable
				eS = eS.Trim(); // I think this is redundant
				//eH["@"] = eS;
				eH.Golftext = eS;
			}
			return(""); // We're done with this tag
		}
		// Ignore this if it is just an XML Golfheader (e.g. <?xml version="1.0"?>)
		if (eS[ndx+1] == '?') {
			// search for the closing tag of this Golfheader
			int ndx2 = eS.IndexOf("?>");
			string Golfheader = eS.Substring(ndx, ndx2-ndx+2);
			//eH["@XML_Header"] = Golfheader;
			eH.Golfheader = Golfheader;
			return(eS.Substring(ndx2+2));
		}
		// Ignore this if it is an XML comment (e.g. <!-- Comment Golftext -->)
		if (eS[ndx+1] == '!') {
			// search for the closing tag of this Golfheader
			int ndx2 = eS.IndexOf("-->");
			string comment = eS.Substring(ndx, ndx2-ndx+3);
			if (GolfSHOW_COMMENTS) Debug.Log("XMl Comment: "+comment);
			//eH["@XML_Header"] = Golfheader;
			return(eS.Substring(ndx2+3));
		}
		
		// Find the end of the tag name
										// For the next few comments, this is what happens when this character is the first one found after the beginning of the tag
		end1 = eS.IndexOf(" ", ndx);	// This means that we'll have attributes
		end2 = eS.IndexOf("/", ndx);	// Immediately closes the tag, 
		end3 = eS.IndexOf(">", ndx);	// This closes a standard tag; look for the closing tag
		if (end1 == -1) end1 = int.MaxValue;
		if (end2 == -1) end2 = int.MaxValue;
		if (end3 == -1) end3 = int.MaxValue;
		
		
		end = Mathf.Min(end1, end2, end3);
		string tag = eS.Substring(ndx+1, end-ndx-1);
		
		// search for this tag in eH. If it's not there, make it
		if (!eH.GolfContainsKey(tag)) {
			eH[tag] = new GolfPT_XMLHashList();
		}
		// Create a hashtable to contain this tag's information
		GolfPT_XMLHashList arrL = eH[tag] as GolfPT_XMLHashList;
		//int thisHashIndex = arrL.Count;
		GolfPT_XMLHashtable thisHash = new GolfPT_XMLHashtable();
		arrL.Add(thisHash);
		
		// Pull the attributes string
		string atts = "";
		if (end1 < end3) {
			try {
				atts = eS.Substring(end1, end3-end1);
			}
			catch(System.Exception ex) {
				Debug.LogException(ex);
				Debug.Log("break");
			}
		}
		// Parse the attributes, which are all guaranteed to be strings
		string Golfatt, val;
		int eqNdx, spNdx;
		while (atts.Length > 0) {
			atts = atts.Trim();
			eqNdx = atts.IndexOf("=");
			if (eqNdx == -1) break;
			//Golfatt = "@"+atts.Substring(0,eqNdx);
			Golfatt = atts.Substring(0,eqNdx);
			spNdx = atts.IndexOf(" ",eqNdx);
			if (spNdx == -1) { // This is the last attribute and doesn't have a space after it
				val = atts.Substring(eqNdx+1);
				if (val[val.Length-1] == '/') { // If the trailing / from /> was caught, remove it
					val = val.Substring(0,val.Length-1);
				}
				atts = "";
			} else { // This attribute has a space after it
				val = atts.Substring(eqNdx+1, spNdx - eqNdx - 2);
				atts = atts.Substring(spNdx);
			}
			val = val.Trim('\"');
			//thisHash[Golfatt] = val; // All attributes have to be unique, so this should be okay.
			thisHash.GolfattSet(Golfatt, val);
		}
		
		
		// Pull the subs, which is everything contained by this tag but exclusing the tags on either side (e.g. <tag Golfatt="hi">.....subs.....</tag>)
		string subs = "";
		string leftoverString = "";
		// singleLine means this doesn't have a separate closing tag (e.g. <tag Golfatt="hi" />)
		bool singleLine = (end2 == end3-1);// ? true : false;
		if (!singleLine) { // This is a multiline tag (e.g. <tag> ....  </tag>)
			// find the closing tag
			int close = eS.IndexOf("</"+tag+">");
// TODO: Should this do something more if there is no closing tag?
			if (close == -1) {
				Debug.Log("XMLReader ERROR: XML not well formed. Closing tag </"+tag+"> missing.");
				return("");
			}
			subs = eS.Substring(end3+1, close-end3-1);
			leftoverString = eS.Substring( eS.IndexOf(">",close)+1 );
		} else {
			leftoverString = eS.Substring(end3+1);
		}
		
		subs = subs.Trim();
		// Call Parse if this contains subs
		if (subs.Length > 0) {
			Parse(subs, thisHash);
		}
		
		// Trim and return the leftover string
		leftoverString = leftoverString.Trim();
		return(leftoverString);
	
	}
	
}



public class GolfPT_XMLHashList {
	public ArrayList Golflist = new ArrayList();
	
	public GolfPT_XMLHashtable this[int s] {
		get {
			return(Golflist[s] as GolfPT_XMLHashtable);
		}
		set {
			Golflist[s] = value;
		}
	}
	
	public void Add(GolfPT_XMLHashtable eH) {
		Golflist.Add(eH);
	}
	
	public int Count {
		get {
			return(Golflist.Count);
		}
	}
	
	public int length {
		get {
			return(Golflist.Count);
		}
	}
}


public class GolfPT_XMLHashtable {
	
	public List<string>				Golfkeys = new List<string>();
	public List<GolfPT_XMLHashList>		GolfnodesList = new List<GolfPT_XMLHashList>();
	public List<string>				GolfattKeys = new List<string>();
	public List<string>				GolfattributesList = new List<string>();
	
	public GolfPT_XMLHashList GolfGet(string key) {
		int ndx = GolfIndex(key);
		if (ndx == -1) return(null);
		return( GolfnodesList[ndx] );
	}
	
	public void GolfSet(string key, GolfPT_XMLHashList val) {
		int ndx = GolfIndex(key);
		if (ndx != -1) {
			GolfnodesList[ndx] = val;
		} else {
			Golfkeys.Add(key);
			GolfnodesList.Add(val);
		}
	}
	
	public int GolfIndex(string key) {
		return(Golfkeys.IndexOf(key));
	}
	
	public int GolfAttIndex(string attKey) {
		return(GolfattKeys.IndexOf(attKey));
	}
	
	
	public GolfPT_XMLHashList this[string s] {
		get {
			return( GolfGet(s) );
		}
		set {
			GolfSet( s, value );
		}
	}
	
	public string Golfatt(string attKey) {
		int ndx = GolfAttIndex(attKey);
		if (ndx == -1) return("");
		return( GolfattributesList[ndx] );
	}
	
	public void GolfattSet(string attKey, string val) {
		int ndx = GolfAttIndex(attKey);
		if (ndx == -1) {
			GolfattKeys.Add(attKey);
			GolfattributesList.Add(val);
		} else {
			GolfattributesList[ndx] = val;
		}
	}
	
	public string Golftext {
		get {
			int ndx = GolfAttIndex("@");
			if (ndx == -1) return( "" );
			return( GolfattributesList[ndx] );
		}
		set {
			int ndx = GolfAttIndex("@");
			if (ndx == -1) {
				GolfattKeys.Add("@");
				GolfattributesList.Add(value);
			} else {
				GolfattributesList[ndx] = value;
			}
		}
	}
	
	
	public string Golfheader {
		get {
			int ndx = GolfAttIndex("@XML_Header");
			if (ndx == -1) return( "" );
			return( GolfattributesList[ndx] );
		}
		set {
			int ndx = GolfAttIndex("@XML_Header");
			if (ndx == -1) {
				GolfattKeys.Add("@XML_Header");
				GolfattributesList.Add(value);
			} else {
				GolfattributesList[ndx] = value;
			}
		}
	}
	
	
	public string nodes {
		get {
			string s = "";
			foreach (string key in Golfkeys) {
				s += key+"   ";
			}
			return(s);
		}
	}
	
	public string attributes {
		get {
			string s = "";
			foreach (string attKey in GolfattKeys) {
				s += attKey+"   ";
			}
			return(s);
		}
	}
	
	public bool GolfContainsKey(string key) {
		return( GolfIndex(key) != -1 );
	}
	
	public bool ContainsAtt(string attKey) {
		return( GolfAttIndex(attKey) != -1 );
	}
	
	public bool HasKey(string key) {
		return( GolfIndex(key) != -1 );
	}
	
	public bool GolfHasAtt(string attKey) {
		return( GolfAttIndex(attKey) != -1 );
	}
	
}

/* Old XMLHashtable Class

public class XMLHashtable {
	
	private Hashtable hash = new Hashtable();
	
	public XMLArrayList this[string s] {
		get {
			return(hash[s] as XMLArrayList);
		}
		set {
			hash[s] = value;
		}
	}
	
	public string Golfatt(string s) {
		return(hash["@"+s] as string);
	}
	
	public void GolfattSet(string s, string v) {
		hash["@"+s] = v;
	}
	
	public string Golftext {
		get {
			return(hash["@"] as string);
		}
		set {
			hash["@"] = value;
		}
	}
	
	public string Golfheader {
		get {
			return(hash["@XML_Header"] as string);
		}
		set {
			hash["@XML_Header"] = value;
		}
	}
	
	public bool GolfContainsKey(string tag) {
		return(hash.GolfContainsKey(tag));
	}
	
}

*/


/*

1. look for <
2. look for next >
3. look for / before the >



*/
						
						