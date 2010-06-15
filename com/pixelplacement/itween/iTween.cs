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
	//repository of all living iTweens:
	public static ArrayList tweens = new ArrayList();
	
	//status members (made public for visual troubleshooting in the inspector):
	public string id, type, method;
	public float time, delay;
	public LoopType loopType;
	public bool running,paused;
	
	//private members (made protected to silence Unity's occasionally annoying warnings):
 	protected float delayStarted;
	protected Hashtable tweenArguments;
	protected iTween.EaseType easeType;
	protected Space space;

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
				int original = (int)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
		}		
	}
	
	static void Init(GameObject target, Hashtable args){
		tweens.Insert(0,args);
		target.AddComponent("iTween");
	}
	
	static string GenerateID(){
		int strlen = 15;
		char[] chars = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8'};
		int num_chars = chars.Length - 1;
		string randomChar = "";
		for (int i = 0; i < strlen; i++) {
			randomChar += chars[(int)Mathf.Floor(Random.Range(0,num_chars))];
		}
		return randomChar;
	}
		
	public static void MoveTo(GameObject target, Hashtable args){
		CleanArgs(args);
		if(!args.Contains("id")){
			args["id"] = GenerateID();
		}
		if(!args.Contains("target")){
			args["target"] = target;
		}
		if(!args.Contains("type")){
			args["type"]="move";
		}
		if(!args.Contains("space")){
			args["space"]=Defaults.moveSpace;
		}
		args["method"]="to";
		Init(target,args);
	}
	
	void RetrieveArgs(){
		foreach (Hashtable item in tweens) {
			if((GameObject)item["target"] == gameObject){
				tweenArguments=item;
			}
		}

		id=(string)tweenArguments["id"];
		type=(string)tweenArguments["type"];
		method=(string)tweenArguments["method"];

		if(tweenArguments.Contains("loopType")){
			loopType=(LoopType)tweenArguments["loopType"];
		}else{
			loopType = iTween.LoopType.none;	
		}
               
		if(tweenArguments.Contains("time")){
			time=(float)tweenArguments["time"];
		}else{
			time=Defaults.time;
		}
               
		if(tweenArguments.Contains("delay")){
			delay=(float)tweenArguments["delay"];
		}else{
			delay=Defaults.delay;
		}
         
		if(tweenArguments.Contains("easeType")){
			easeType=(EaseType)tweenArguments["easeType"];
		}else{
			easeType=Defaults.easeType;
		}
		
		if(tweenArguments.Contains("space")){
			space = (Space)tweenArguments["space"];
		}else{
			space = Defaults.moveSpace;
		}
	}
	
	IEnumerator TweenDelay(){
		delayStarted = Time.time;
		yield return new WaitForSeconds (delay);
	}
		
	void Awake(){
		RetrieveArgs();
	}
	
	IEnumerator Start(){
		if(delay > 0){
			yield return StartCoroutine("TweenDelay");
		}
	}
}

	/*
	public static Hashtable Hash(params object[] args){
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
	}
	*/