#region Namespaces
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
#endregion

/// <summary>
/// <para>Version: 2.0.0</para>	 
/// <para>Author: Bob Berkebile (http://pixelplacement.com)</para>
/// <para>Contributors: Patrick Corkum (http://insquare.com)</para>
/// <para>Support: http://itween.pixelplacement.com</para>
/// </summary>
/// 
public class iTween : MonoBehaviour{
	
#region Variables
	//repository of all living iTweens:
	public static ArrayList tweens = new ArrayList();
	
	//status members (made public for visual troubleshooting in the inspector):
	public string id, type, method;
	public float time, delay;
	public LoopType loopType;
	public bool isRunning,isPaused;
	
	//private members:
 	private float runningTime, percentage;
	protected float delayStarted; //probably not neccesary that this be protected but it shuts Unity's compiler up about this being "never used"
	private bool kinematic;
	private Hashtable tweenArguments;
	private iTween.EaseType easeType;
	private Space space;
	private delegate float EasingFunctionDelegate(float start, float end, float value);
	private delegate void ApplyTweenDelegate();
	private EasingFunctionDelegate ease;
	private ApplyTweenDelegate apply;
	private AudioSource audioSource;
	private Vector3[] vector3s;
	private Vector2[] vector2s;
	private Color[] colors;
	private float[] floats;
	private int[] ints;
	private Rect[] rects;	
	
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
		/// Ping pong the animation back and forth.
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
#endregion
	
#region Defaults
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
#endregion
	
#region Internal Helpers
	//cast any accidentally supplied doubles and ints as floats as iTween only uses floats internally:
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
			if(item.Value.GetType() == typeof(System.Double)){
				double original = (double)item.Value;
				float casted = (float)original;
				args[item.Key] = casted;
			}
		}		
	}	
	
	//random ID generator:
	static string GenerateID(){
		int strlen = 15;
		char[] chars = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8'};
		int num_chars = chars.Length - 1;
		string randomChar = "";
		for (int i = 0; i < strlen; i++) {
			randomChar += chars[(int)Mathf.Floor(UnityEngine.Random.Range(0,num_chars))];
		}
		return randomChar;
	}	
	
	//grab and set generic, neccesary iTween arguments:
	void RetrieveArgs(){
		foreach (Hashtable item in tweens) {
			if((GameObject)item["target"] == gameObject){
				tweenArguments=item;
			}
		}

		id=(string)tweenArguments["id"];
		type=(string)tweenArguments["type"];
		method=(string)tweenArguments["method"];
               
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
		
		if(tweenArguments.Contains("loopType")){
			//allows loopType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["loopType"].GetType() == typeof(LoopType)){
				loopType=(LoopType)tweenArguments["loopType"];
			}else{
				try {
					loopType=(LoopType)Enum.Parse(typeof(LoopType),(string)tweenArguments["loopType"],true); 
				} catch {
					Debug.LogWarning("iTween: Unsupported loopType supplied! Default will be used.");
					loopType = iTween.LoopType.none;	
				}
			}			
		}else{
			loopType = iTween.LoopType.none;	
		}		
         
		if(tweenArguments.Contains("easeType")){
			//allows easeType to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["easeType"].GetType() == typeof(EaseType)){
				easeType=(EaseType)tweenArguments["easeType"];
			}else{
				try {
					easeType=(EaseType)Enum.Parse(typeof(EaseType),(string)tweenArguments["easeType"],true); 
				} catch {
					Debug.LogWarning("iTween: Unsupported easeType supplied! Default will be used.");
					easeType=Defaults.easeType;
				}
			}
		}else{
			easeType=Defaults.easeType;
		}
				
		if(tweenArguments.Contains("space")){
			//allows space to be set as either an enum(C# friendly) or a string(JS friendly), string case usage doesn't matter to further increase usability:
			if(tweenArguments["space"].GetType() == typeof(Space)){
				space=(Space)tweenArguments["space"];
			}else{
				try {
					space=(Space)Enum.Parse(typeof(Space),(string)tweenArguments["space"],true); 	
				} catch {
					Debug.LogWarning("iTween: Unsupported space supplied! Default will be used.");
					space = Defaults.moveSpace;
				}
			}			
		}else{
			space = Defaults.moveSpace;
		}
		
		//instantiates a cached ease equation refrence:
		GetEasingFunction();
	}	
	
	//catalog new tween and add component phase of iTween:
	static void Launch(GameObject target, Hashtable args){
		CleanArgs(args);
		if(!args.Contains("id")){
			args["id"] = GenerateID();
		}
		if(!args.Contains("target")){
			args["target"] = target;
		}		
		tweens.Insert(0,args);
		target.AddComponent("iTween");
	}	

	//instantiates a cached ease equation refrence:
	void GetEasingFunction(){
		switch (easeType){
		case EaseType.easeInQuad:
			ease  = new EasingFunctionDelegate(easeInQuad);
			break;
		case EaseType.easeOutQuad:
			ease = new EasingFunctionDelegate(easeOutQuad);
			break;
		case EaseType.easeInOutQuad:
			ease = new EasingFunctionDelegate(easeInOutQuad);
			break;
		case EaseType.easeInCubic:
			ease = new EasingFunctionDelegate(easeInCubic);
			break;
		case EaseType.easeOutCubic:
			ease = new EasingFunctionDelegate(easeOutCubic);
			break;
		case EaseType.easeInOutCubic:
			ease = new EasingFunctionDelegate(easeInOutCubic);
			break;
		case EaseType.easeInQuart:
			ease = new EasingFunctionDelegate(easeInQuart);
			break;
		case EaseType.easeOutQuart:
			ease = new EasingFunctionDelegate(easeOutQuart);
			break;
		case EaseType.easeInOutQuart:
			ease = new EasingFunctionDelegate(easeInOutQuart);
			break;
		case EaseType.easeInQuint:
			ease = new EasingFunctionDelegate(easeInQuint);
			break;
		case EaseType.easeOutQuint:
			ease = new EasingFunctionDelegate(easeOutQuint);
			break;
		case EaseType.easeInOutQuint:
			ease = new EasingFunctionDelegate(easeInOutQuint);
			break;
		case EaseType.easeInSine:
			ease = new EasingFunctionDelegate(easeInSine);
			break;
		case EaseType.easeOutSine:
			ease = new EasingFunctionDelegate(easeOutSine);
			break;
		case EaseType.easeInOutSine:
			ease = new EasingFunctionDelegate(easeInOutSine);
			break;
		case EaseType.easeInExpo:
			ease = new EasingFunctionDelegate(easeInExpo);
			break;
		case EaseType.easeOutExpo:
			ease = new EasingFunctionDelegate(easeOutExpo);
			break;
		case EaseType.easeInOutExpo:
			ease = new EasingFunctionDelegate(easeInOutExpo);
			break;
		case EaseType.easeInCirc:
			ease = new EasingFunctionDelegate(easeInCirc);
			break;
		case EaseType.easeOutCirc:
			ease = new EasingFunctionDelegate(easeOutCirc);
			break;
		case EaseType.easeInOutCirc:
			ease = new EasingFunctionDelegate(easeInOutCirc);
			break;
		case EaseType.linear:
			ease = new EasingFunctionDelegate(linear);
			break;
		case EaseType.spring:
			ease = new EasingFunctionDelegate(spring);
			break;
		case EaseType.bounce:
			ease = new EasingFunctionDelegate(bounce);
			break;
		case EaseType.easeInBack:
			ease = new EasingFunctionDelegate(easeInBack);
			break;
		case EaseType.easeOutBack:
			ease = new EasingFunctionDelegate(easeOutBack);
			break;
		case EaseType.easeInOutBack:
			ease = new EasingFunctionDelegate(easeInOutBack);
			break;
		}
	}
	
	//calculate percentage of tween based on time:
	void UpdatePercentage(){
		runningTime+=Time.deltaTime;
		percentage = runningTime/time;
	}
	
	//call correct set target method and set tween application delegate:
	void GenerateTargets(){
		switch (type) {
		case "move":
			GenerateMoveTargets();
			apply = new ApplyTweenDelegate(ApplyMoveTargets);
			break;
		case "scale":
			GenerateScaleTargets();
			apply = new ApplyTweenDelegate(ApplyScaleTargets);
			break;
		default:
		break;
		}
	}
#endregion
	
#region Set Methods	
	#region GenerateMoveTargets
	void GenerateMoveTargets(){
		if(percentage==1.0f && method=="from"){//if method is "from" and from has already been applied shuffle from and to values and reset percentage to 0:
			vector3s[1]=vector3s[0];
			if (space==Space.World) {
				vector3s[0]=vector3s[3]=transform.position;				
			}else{
				vector3s[0]=vector3s[3]=transform.localPosition;
			}
			percentage=0;
		}else{
			//start values:
			vector3s=new Vector3[4];//[0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization
			if (space==Space.World) {
				vector3s[0]=vector3s[1]=vector3s[3]=transform.position;				
			}else{
				vector3s[0]=vector3s[1]=vector3s[3]=transform.localPosition;
			}
			
			//end values:
			switch (method) {
			case "from":
			case "to":
				if (tweenArguments.Contains("position")) {
					vector3s[1]=(Vector3)tweenArguments["position"];
				}else{
					if (tweenArguments.Contains("x")) {
						vector3s[1].x=(float)tweenArguments["x"];
					}
					if (tweenArguments.Contains("y")) {
						vector3s[1].y=(float)tweenArguments["y"];
					}
					if (tweenArguments.Contains("z")) {
						vector3s[1].z=(float)tweenArguments["z"];
					}
				}	
				break;
			case "by":
			case "add":
				if (tweenArguments.Contains("amount")) {
					vector3s[1]+=(Vector3)tweenArguments["amount"];
				}else{
					if (tweenArguments.Contains("x")) {
						vector3s[1].x+=(float)tweenArguments["x"];
					}
					if (tweenArguments.Contains("y")) {
						vector3s[1].y+=(float)tweenArguments["y"];
					}
					if (tweenArguments.Contains("z")) {
						vector3s[1].z+=(float)tweenArguments["z"];
					}
				}
				break;
			}				
		}
	}
	#endregion
	
	#region GenerateScaleTargets
	void GenerateScaleTargets(){
		if(percentage==1.0f && method=="from"){//if method is "from" and from has already been applied shuffle from and to values and reset percentage to 0:
			vector3s[1]=vector3s[0];
			vector3s[0]=vector3s[3]=transform.localScale;	
			percentage=0;
		}else{
			//start values:
			vector3s=new Vector3[4];//[0] from, [1] to, [2] calculated value from ease equation
			vector3s[0]=vector3s[1]=transform.localScale;	
			
			//end values:
			switch (method) {
			case "from":
			case "to":
				if (tweenArguments.Contains("scale")) {
					vector3s[1]=(Vector3)tweenArguments["scale"];
				}else{
					if (tweenArguments.Contains("x")) {
						vector3s[1].x=(float)tweenArguments["x"];
					}
					if (tweenArguments.Contains("y")) {
						vector3s[1].y=(float)tweenArguments["y"];
					}
					if (tweenArguments.Contains("z")) {
						vector3s[1].z=(float)tweenArguments["z"];
					}
				}	
				break;
			case "by":
				if (tweenArguments.Contains("amount")) {
					vector3s[1]=Vector3.Scale(vector3s[1],(Vector3)tweenArguments["amount"]);
				}else{
					if (tweenArguments.Contains("x")) {
						vector3s[1].x*=(float)tweenArguments["x"];
					}
					if (tweenArguments.Contains("y")) {
						vector3s[1].y*=(float)tweenArguments["y"];
					}
					if (tweenArguments.Contains("z")) {
						vector3s[1].z*=(float)tweenArguments["z"];
					}
				}
				break;				
			case "add":
				if (tweenArguments.Contains("amount")) {
					vector3s[1]+=(Vector3)tweenArguments["amount"];
				}else{
					if (tweenArguments.Contains("x")) {
						vector3s[1].x+=(float)tweenArguments["x"];
					}
					if (tweenArguments.Contains("y")) {
						vector3s[1].y+=(float)tweenArguments["y"];
					}
					if (tweenArguments.Contains("z")) {
						vector3s[1].z+=(float)tweenArguments["z"];
					}
				}
				break;
			}				
		}
	}	
	#endregion
#endregion
	
#region Apply Methods
	#region ApplyMoveTargets
	void ApplyMoveTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.Translate(vector3s[2]-vector3s[3],space);
		
		//record:
		vector3s[3]=vector3s[2];
	}		
	#endregion
	
	#region ApplyScaleTargets
	void ApplyScaleTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.localScale = vector3s[2];
	}		
	#endregion
#endregion
		
#region External Utilities
	//stops
	//pauses
	//completes
	//rewinds
	//counts
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
#endregion
		
#region Static Registers
	#region Documentation
	/// <summary>
	/// Moves a GameObject's position to the supplied coordinates.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="orientToPath">
	/// A <see cref="System.Boolean"/>
	/// </param>
	/// <param name="lookTarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion 
	public static void MoveTo(GameObject target, Hashtable args){
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Moves a GameObject from the supplied coordinates to its starting position.
	/// </summary>
	/// <param name="position">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="orientToPath">
	/// A <see cref="System.Boolean"/>
	/// </param>
	/// <param name="lookTarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion 
	public static void MoveFrom(GameObject target, Hashtable args){
		args["type"]="move";
		args["method"]="from";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Adds the supplied coordinates to a GameObject's postion.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="orientToPath">
	/// A <see cref="System.Boolean"/>
	/// </param>
	/// <param name="lookTarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void MoveAdd(GameObject target, Hashtable args){
		args["type"]="move";
		args["method"]="add";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Adds the supplied coordinates to a GameObject's postion.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="orientToPath">
	/// A <see cref="System.Boolean"/>
	/// </param>
	/// <param name="lookTarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void MoveBy(GameObject target, Hashtable args){
		args["type"]="move";
		args["method"]="by";
		Launch(target,args);
	}

	#region Documentation
	/// <summary>
	/// Scales a GameObject to the supplied values.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void ScaleTo(GameObject target, Hashtable args){
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Scales a GameObject from the supplied values to its starting values.
	/// </summary>
	/// <param name="scale">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void ScaleFrom(GameObject target, Hashtable args){
		args["type"]="scale";
		args["method"]="from";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Adds the supplied values to a GameObject's scale.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void ScaleAdd(GameObject target, Hashtable args){
		args["type"]="scale";
		args["method"]="add";
		Launch(target,args);
	}
	
	#region Documentation
	/// <summary>
	/// Multiplies the GameObject's scale by the supplied values.
	/// </summary>
	/// <param name="amount">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// </param>
	#endregion	
	public static void ScaleBy(GameObject target, Hashtable args){
		args["type"]="scale";
		args["method"]="by";
		Launch(target,args);
	}	
#endregion
	
#region Application Segments
	void TweenFrom(){
		GenerateTargets();
		percentage=1.0f;
		apply();
	}
	
	IEnumerator TweenDelay(){
		delayStarted = Time.time;
		yield return new WaitForSeconds (delay);
	}	
	
	void TweenStart(){
		//fire start callback
		//handle destruction of running duplicate types
		//handle kinematic toggle
		//run stab adn anything else that doesn't loop?
		//setup curve crap?
		GenerateTargets();
		isRunning = true;
	}
	
	void TweenUpdate(){
		//fire update callback
		apply();
		UpdatePercentage();		
	}
	
	void TweenLoop(){
		//do not destroy and create a new iTween, just reset percentage to 0???
	}
	
	void TweenComplete(){
		//fire complete callback
		//dial in percentage to 1 for final run
		isRunning=false;
		percentage=1.0f;
        apply();
		/*
		print("temp test for loop method will need delay reapplication");
		percentage=0;
		runningTime=0;
		isRunning=true;
		*/
	}
#endregion

#region Component Segments
	void Awake(){
		RetrieveArgs();
	}
	
	IEnumerator Start(){
		if(method=="from"){
			TweenFrom();
		}
		if(delay > 0){
			yield return StartCoroutine("TweenDelay");
		}
		TweenStart();
	}	
	
	void Update(){
		if(isRunning){
			if(percentage<1f ){
				TweenUpdate();
			}else{
				TweenComplete();	
			}
		}
	}
	
#endregion
	
#region Easing Curves
	private float linear(float start, float end, float value){
		return Mathf.Lerp(start, end, value);
	}
	
	private float clerp(float start, float end, float value){
		float min = 0.0f;
		float max = 360.0f;
		float half = Mathf.Abs((max - min) / 2.0f);
		float retval = 0.0f;
		float diff = 0.0f;
		if ((end - start) < -half){
			diff = ((max - start) + end) * value;
			retval = start + diff;
		}else if ((end - start) > half){
			diff = -((max - end) + start) * value;
			retval = start + diff;
		}else retval = start + (end - start) * value;
		return retval;
    }

	private float spring(float start, float end, float value){
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
		return start + (end - start) * value;
	}

	private float easeInQuad(float start, float end, float value){
		end -= start;
		return end * value * value + start;
	}

	private float easeOutQuad(float start, float end, float value){
		end -= start;
		return -end * value * (value - 2) + start;
	}

	private float easeInOutQuad(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value + start;
		value--;
		return -end / 2 * (value * (value - 2) - 1) + start;
	}

	private float easeInCubic(float start, float end, float value){
		end -= start;
		return end * value * value * value + start;
	}

	private float easeOutCubic(float start, float end, float value){
		value--;
		end -= start;
		return end * (value * value * value + 1) + start;
	}

	private float easeInOutCubic(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value + start;
		value -= 2;
		return end / 2 * (value * value * value + 2) + start;
	}

	private float easeInQuart(float start, float end, float value){
		end -= start;
		return end * value * value * value * value + start;
	}

	private float easeOutQuart(float start, float end, float value){
		value--;
		end -= start;
		return -end * (value * value * value * value - 1) + start;
	}

	private float easeInOutQuart(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value * value + start;
		value -= 2;
		return -end / 2 * (value * value * value * value - 2) + start;
	}

	private float easeInQuint(float start, float end, float value){
		end -= start;
		return end * value * value * value * value * value + start;
	}

	private float easeOutQuint(float start, float end, float value){
		value--;
		end -= start;
		return end * (value * value * value * value * value + 1) + start;
	}

	private float easeInOutQuint(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * value * value * value * value * value + start;
		value -= 2;
		return end / 2 * (value * value * value * value * value + 2) + start;
	}

	private float easeInSine(float start, float end, float value){
		end -= start;
		return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
	}

	private float easeOutSine(float start, float end, float value){
		end -= start;
		return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
	}

	private float easeInOutSine(float start, float end, float value){
		end -= start;
		return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
	}

	private float easeInExpo(float start, float end, float value){
		end -= start;
		return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
	}

	private float easeOutExpo(float start, float end, float value){
		end -= start;
		return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
	}

	private float easeInOutExpo(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
		value--;
		return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
	}

	private float easeInCirc(float start, float end, float value){
		end -= start;
		return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
	}

	private float easeOutCirc(float start, float end, float value){
		value--;
		end -= start;
		return end * Mathf.Sqrt(1 - value * value) + start;
	}

	private float easeInOutCirc(float start, float end, float value){
		value /= .5f;
		end -= start;
		if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
		value -= 2;
		return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
	}

	private float bounce(float start, float end, float value){
		value /= 1f;
		end -= start;
		if (value < (1 / 2.75f)){
			return end * (7.5625f * value * value) + start;
		}else if (value < (2 / 2.75f)){
			value -= (1.5f / 2.75f);
			return end * (7.5625f * (value) * value + .75f) + start;
		}else if (value < (2.5 / 2.75)){
			value -= (2.25f / 2.75f);
			return end * (7.5625f * (value) * value + .9375f) + start;
		}else{
			value -= (2.625f / 2.75f);
			return end * (7.5625f * (value) * value + .984375f) + start;
		}
	}

	private float easeInBack(float start, float end, float value){
		end -= start;
		value /= 1;
		float s = 1.70158f;
		return end * (value) * value * ((s + 1) * value - s) + start;
	}

	private float easeOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value = (value / 1) - 1;
		return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
	}

	private float easeInOutBack(float start, float end, float value){
		float s = 1.70158f;
		end -= start;
		value /= .5f;
		if ((value) < 1){
			s *= (1.525f);
			return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
		}
		value -= 2;
		s *= (1.525f);
		return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
	}

	private float punch(float amplitude, float value){
		float s = 9;
		if (value == 0){
			return 0;
		}
		if (value == 1){
			return 0;
		}
		float period = 1 * 0.3f;
		s = period / (2 * Mathf.PI) * Mathf.Asin(0);
		return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
    }
#endregion	
}


/*
Sample documentation
	/// <summary>
	/// Slides a <paramref name="GameObject"/> to a new position.
	/// <remarks>
	/// Base operation only requires the use of "position" or a combination of the "x", "y", or "z" parameters to function.  If <paramref name="GameObject"/> has a <paramref name="Rigidbody"/> component attached; isKinematic property will be toggled to true and then back to initial value ot avoid physics based anomalies
	/// </remarks>
	/// </summary>
	/// <param name="position">
	/// A <see cref="Vector3"/>
	/// Coordinates in 3D space that the GameObject will animate to.
	/// </param>
	/// <param name="x">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// Do not use if "position" is being set. Sets a destination on the x-axis.
	/// </param>
	/// <param name="y">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// Do not use if "position" is being set. Sets a destination on the y-axis.
	/// </param>
	/// <param name="z">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// Do not use if "position" is being set. Sets a destination on the z-axis.
	/// </param>
	/// <param name="time">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// The length of time the iTween will run. If omitted, default value of 1 is used.
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
	/// The length of time iTween will wait before executing. If omitted, default value of 0 is used.
	/// </param>
	/// <param name="space">
	/// A <see cref="Space"/> or <see cref="System.String"/>
	/// The coordinate space in which to operate the movement. If omitted, default value of Space.World is used.
	/// </param>
	/// <param name="easeType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// What type of easing to apply using Robert Penner's open source easing equations. If omitted, default value of easeInOutCubic is used.
	/// </param>   
	/// <param name="loopType">
	/// A <see cref="EaseType"/> or <see cref="System.String"/>
	/// Sets if and how the motion will loop. If omitted, default value of LoopType.none.
	/// </param>
	/// <param name="orientToPath">
	/// A <see cref="System.Boolean"/>
	/// If true, GameObject will face the direction it is moving in. If omitted, default value of false is used.
	/// </param>
	/// <param name="lookTarget">
	/// A <see cref="Vector3"/> or A <see cref="Transform"/>
	/// A Transform or Vector3 the GameObject will face as it moves.  Will override "orientToPath".
	/// </param>
	/// <param name="onStart">
	/// A <see cref="System.String"/>
	/// A callback function that will execute as soon as the iTween begins (uses Unity's SendMessage).
	/// </param>
	/// <param name="onStartTarget">
	/// A <see cref="GameObject"/>
	/// Utilized by "onStart" callback as reference to a GameObject which holds the intended function to be called.  If omitted, iTween defaults to the GameObject that made the initial iTween call.
	/// </param>
	/// <param name="onStartParams">
	/// A <see cref="System.Object"/>
	/// Passes a <paramref name="System.Object"/> to the "onStart" callback (uses Unity's SendMessage).
	/// </param>
	/// <param name="onUpdate">
	/// A <see cref="System.String"/>
	/// A callback function that will execute as the GameObject updates (uses Unity's SendMessage).
	/// </param>
	/// <param name="onUpdateTarget">
	/// A <see cref="GameObject"/>
	/// Utilized by the "onUpdate" callback as reference to a <paramref name="GameObject"/> which holds the intended function to be called. If omitted, iTween defaults to the GameObject that made the initial iTween call.
	/// </param>
	/// <param name="onUpdateParams">
	/// A <see cref="System.Object"/>
	/// Passes a <paramref name="System.Object"/> to the "onUpdate" callback (uses Unity's SendMessage).
	/// </param> 
	/// <param name="onComplete">
	/// A <see cref="System.String"/>
	/// A callback function that will execute as soon as the iTween ends (uses Unity's SendMessage).
	/// </param>
	/// <param name="onCompleteTarget">
	/// A <see cref="GameObject"/>
	/// Utilized by the "onComplete" callback as reference to a <paramref name="GameObject"/> which holds the intended function to be called. If omitted, iTween defaults to the GameObject that made the initial iTween call.
	/// </param>
	/// <param name="onCompleteParams">
	/// A <see cref="System.Object"/>
	/// Passes a <paramref name="System.Object"/> to the "onComplete" callback (uses Unity's SendMessage).
	/// </param>   
*/