using System.Collections;
using System.Reflection;
using UnityEngine;

/// <summary>
/// <para>Version: 2.0.0</para>
/// <para>Author: Bob Berkebile (http://pixelplacement.com)</para>
/// <para>Contributors: Patrick Corkum (http://insquare.com)</para>
/// <para>Support: http://itween.pixelplacement.com</para>
/// </summary>
public class iTween : MonoBehaviour{	
	public static ArrayList tweens;
	
	//status members (for visual troubleshooting in the inspector):
	public string id;
	public string type;
	public string method;
	public LoopType loopType;
	public bool running;
	public bool paused;		
	
	/// <summary>
	/// The type of easing to use based on Robert Penner's open source easing equations (http://www.robertpenner.com/easing_terms_of_use.html).
	/// </summary>
	public enum EaseType{
		easeInQuad,
		easeOutQuad,
		easeInOutQuad,
		easeInCubic,
		easeOutCubic,
		easeInOutCubic,
		easeInQuart,
		easeOutQuart,
		easeInOutQuart,
		easeInQuint,
		easeOutQuint,
		easeInOutQuint,
		easeInSine,
		easeOutSine,
		easeInOutSine,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		easeInCirc,
		easeOutCirc,
		easeInOutCirc,
		linear,
		spring,
		bounce,
		easeInBack,
		easeOutBack,
		easeInOutBack
	}
	
	/// <summary>
	/// The type of loop (if any) to use.  
	/// </summary>
	public enum LoopType{
		/// <summary>
		/// Do not loop.
		/// </summary>
		none,
		/// <summary>
		/// Rewind and replay.
		/// </summary>
		loop,
		/// <summary>
		/// Ping pong the animation.
		/// </summary>
		pingPong
	}

	/// <summary>
	/// Sets the interpolation equation that the curveTo and curveFrom methods use to calculate how they create thier curves. 
	/// </summary>
	public enum CurveType{
		/// <summary>
		/// The path's curves will exaggerate in and out of control points depending on the amount of travel time available.
		/// </summary>
		bezier,
		/// <summary>
		/// The path's curves are set strictly via Hermite Curve interpolation (better description needed).
		/// </summary>
		hermite
	}
	
	/// <summary>
	/// A collection of baseline presets that iTween needs and utilizes if certain parameters are not provided. 
	/// </summary>
	public static class Defaults{
		//general defaults:
		public static float time = 1f;
		public static float delay = 0f;	
		public static LoopType loopType = LoopType.none;
		public static EaseType easeType = iTween.EaseType.easeInOutCubic;
		public static float lookSpeed = 3f;
		//move defaults:
		public static Space moveSpace = Space.World;
		public static bool moveOrientToPath = false;	
		//curve defaults:
		public static Space curveSpace = Space.World;
		public static bool curveOrientToPath = true;
		public static EaseType curveEaseType = iTween.EaseType.easeInOutSine;
		public static CurveType curveType = CurveType.bezier;
		//moveUpdate defaults:
		public static float moveUpdateTime = .05f;
		//lookUpdate defaults:
		public static float lookUpdateSpeed = 3f;
		//rotate defaults:
		public static Space rotateSpace = Space.Self;
		//shakePosition defaults:
		public static Space shakePositionSpace = Space.World;
		//shakeRotation defaults:
		public static Space shakeRotationSpace = Space.Self;
		//punchPosition defaults: 
		public static Space punchPositionSpace = Space.World;
		//punchRotation defaults:
		public static Space punchRotationSpace = Space.Self;
		//color defaults:
		public static EaseType colorEaseType = iTween.EaseType.linear;
		//audio defaults:
		public static EaseType audioEaseType = iTween.EaseType.linear;
		//cameraFade defaults:
		public static int cameraFadeDepth = 999999;
	}
	
	//ensure all property values are floats:
	static void CleanArgs(Hashtable args){
		Hashtable argsCopy = new Hashtable(args.Count);
		
		foreach (DictionaryEntry item in args) {
			argsCopy.Add(item.Key, item.Value);
		}
		
		foreach (DictionaryEntry item in argsCopy) {
			if(item.Value.GetType() == typeof(System.Int32)){
				int tempPull = (int)item.Value;
				float tempCast = (float)tempPull;
			}
		}		
	}
	
	public static Hashtable Hash(params object[] args){
		/*
		Hashtable hashTable = new Hashtable(args.Length/2);
		if (args.Length %2 != 0) {
			Debug.LogError("Tween Error: Hash requires an even number of arguments!"); 
			return null;
		}else{
			int i = 0;
			while(i < args.Length - 1) {
				hashTable.Add(args[i], args[i+1]);
				i += 2;
			}
			return hashTable;
		}
		*/
	}
			
	//hash interface:
	public static void MoveTo(GameObject target, Hashtable args){
		CleanArgs(args);
	}
	
	
	//GUIOptions method:
}