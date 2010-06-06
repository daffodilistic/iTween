using UnityEngine;
using System.Collections;
using System.Reflection;

public class iTween : MonoBehaviour {
	//C# version will have 3 methods for setting up tweens: typical hashtable entry (to keep the JS interface simple), typical hashtable with the use of the Hash method for "easier" entry, and finally the same method Unity uses for GUILayout (i.e. iTween.y(20))
	
	//##################################
	//# INTERNAL HASH CREATION UTILITY #
	//##################################
	
	public static Hashtable Hash(params object[] args){
		Hashtable hashTable = new Hashtable(args.Length/2);
		if (args.Length %2 != 0) {
			Debug.LogError("Tween Error: Hash requires an even number of arguments!"); 
		}else{
			int i = 0;
			while(i < args.Length - 1) {
				hashTable.Add(args[i], args[i+1]);
				i += 2;
			}
		}
		return hashTable;
	}
}