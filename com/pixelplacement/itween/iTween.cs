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
	private bool kinematic, isLocal, loop, reverse;
	private Hashtable tweenArguments;
	private iTween.EaseType easeType;
	private Space space;
	private delegate float EasingFunction(float start, float end, float value);
	private delegate void ApplyTween();
	private EasingFunction ease;
	private ApplyTween apply;
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
		easeInOutBack,
		punch
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
		public static bool isLocal = false;
		public static Space space = Space.Self;
		public static bool orientToPath = false;
		public static CurveType curveType = CurveType.bezier; // clean this up!
		//moveUpdate defaults:
		public static float moveUpdateTime = .05f;
		//lookUpdate defaults:
		public static float lookUpdateSpeed = 3f;
		//color defaults:
		public static EaseType colorEaseType = iTween.EaseType.linear;
		//audio defaults:
		public static EaseType audioEaseType = iTween.EaseType.linear;
		//cameraFade defaults:
		public static int cameraFadeDepth = 999999;
	}
	
	#endregion
				
	#region Static Registers
	
	/// <summary>
	/// Changes a GameObject's position over time.
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
	/// <param name="isLocal">
	/// A <see cref="System.Boolean"/>
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
	public static void MoveTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Instantly changes a GameObject's position then returns it to it's starting position over time.
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
	/// <param name="isLocal">
	/// A <see cref="System.Boolean"/>
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
	public static void MoveFrom(GameObject target, Hashtable args){
		Vector3 tempPosition;
		Vector3 fromPosition;
		bool tempIsLocal;
	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set tempIsLocal:
		if(args.Contains("isLocal")){
			tempIsLocal = (bool)args["isLocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}

		//set tempPosition and base fromPosition:
		if(tempIsLocal){
			tempPosition=fromPosition=target.transform.localPosition;
		}else{
			tempPosition=fromPosition=target.transform.position;	
		}
		
		//set augmented fromPosition:
		if(args.Contains("position")){
			fromPosition=(Vector3)args["position"];
		}else{
			if (args.Contains("x")) {
				fromPosition.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromPosition.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromPosition.z=(float)args["z"];
			}
		}
		
		//apply fromPosition:
		if(tempIsLocal){
			target.transform.localPosition = fromPosition;
		}else{
			target.transform.position = fromPosition;	
		}
		
		//set new position arg:
		args["position"]=tempPosition;
		
		//establish iTween:
		args["type"]="move";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Translates a GameObject's position over time.
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
	public static void MoveAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="move";
		args["method"]="add";
		Launch(target,args);
	}
	
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
	public static void MoveBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="move";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Changes a GameObject's scale over time.
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
	public static void ScaleTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Instantly changes a GameObject's scale then returns it to it's starting scale over time.
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
	public static void ScaleFrom(GameObject target, Hashtable args){
		Vector3 tempScale;
		Vector3 fromScale;
	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set base fromScale:
		tempScale=fromScale=target.transform.localScale;
		
		//set augmented fromScale:
		if(args.Contains("scale")){
			fromScale=(Vector3)args["scale"];
		}else{
			if (args.Contains("x")) {
				fromScale.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromScale.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromScale.z=(float)args["z"];
			}
		}
		
		//apply fromScale:
		target.transform.localScale = fromScale;	
		
		//set new scale arg:
		args["scale"]=tempScale;
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="to";
		Launch(target,args);
	}
	
	/// <summary>
	/// Adds to a GameObject's scale over time.
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
	public static void ScaleAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="add";
		Launch(target,args);
	}
	
	/// <summary>
	/// Multiplies a GameObject's scale over time.
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
	public static void ScaleBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="scale";
		args["method"]="by";
		Launch(target,args);
	}
	
	/// <summary>
	/// Rotates a GameObject to the supplied angles in degrees over time (if allowed). 
	/// </summary>
	/// <param name="rotation">
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
	/// <param name="isLocal">
	/// A <see cref="System.Boolean"/>
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
	public static void RotateTo(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Instantly changes a GameObject's rotation then returns it to it's starting rotation over time (if allowed).
	/// </summary>
	/// <param name="rotation">
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
	/// <param name="isLocal">
	/// A <see cref="System.Boolean"/>
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
	public static void RotateFrom(GameObject target, Hashtable args){
		Vector3 tempRotation;
		Vector3 fromRotation;
		bool tempIsLocal;
	
		//clean args:
		args = iTween.CleanArgs(args);
		
		//set tempIsLocal:
		if(args.Contains("isLocal")){
			tempIsLocal = (bool)args["isLocal"];
		}else{
			tempIsLocal = Defaults.isLocal;	
		}

		//set tempRotation and base fromRotation:
		if(tempIsLocal){
			tempRotation=fromRotation=target.transform.localEulerAngles;
		}else{
			tempRotation=fromRotation=target.transform.eulerAngles;	
		}
		
		//set augmented fromRotation:
		if(args.Contains("rotation")){
			fromRotation=(Vector3)args["rotation"];
		}else{
			if (args.Contains("x")) {
				fromRotation.x=(float)args["x"];
			}
			if (args.Contains("y")) {
				fromRotation.y=(float)args["y"];
			}
			if (args.Contains("z")) {
				fromRotation.z=(float)args["z"];
			}
		}
		
		//apply fromRotation:
		if(tempIsLocal){
			target.transform.localEulerAngles = fromRotation;
		}else{
			target.transform.eulerAngles = fromRotation;	
		}
		
		//set new rotation arg:
		args["rotation"]=tempRotation;
		
		//establish iTween:
		args["type"]="rotate";
		args["method"]="to";
		Launch(target,args);
	}	
	
	/// <summary>
	/// Adds supplied values to a GameObject's rotation over time (if allowed).
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
	/// A <see cr	
	public static void RotateAdd(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween:
		args["type"]="rotate";
		args["method"]="add";
		Launch(target,args);
	}
	
	/// <summary>
	/// Multiplies supplied values by 360 and rotates a GameObject by calculated amount over time (if allowed). 
	/// </summary>
	/// <param name="rotation">
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
	/// A <see cref="Space"/>
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
	public static void RotateBy(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="rotate";
		args["method"]="by";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's position by a diminishing amount over time.
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
	/// A <see cref="Space"/>
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
	public static void ShakePosition(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="position";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's scale by a diminishing amount over time.
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
	public static void ShakeScale(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="scale";
		Launch(target,args);
	}		
	
	/// <summary>
	/// Randomly shakes a GameObject's rotation by a diminishing amount over time.
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
	/// <param name="space">
	/// A <see cref="Space"/>
	/// </param>
	/// <param name="delay">
	/// A <see cref="System.Single"/> or <see cref="System.Double"/>
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
	public static void ShakeRotation(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="shake";
		args["method"]="rotation";
		Launch(target,args);
	}			
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's position and wobbles it back to its initial position.
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
	/// A <see cref="Space"/>
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
	public static void PunchPosition(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="position";
		args["easeType"]=EaseType.punch;
		Launch(target,args);
	}		
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's rotation and wobbles it back to its initial rotation.
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
	/// A <see cref="Space"/>
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
	public static void PunchRotation(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="rotation";
		args["easeType"]=EaseType.punch;
		Launch(target,args);
	}	
	
	/// <summary>
	/// Applies a jolt of force to a GameObject's scale and wobbles it back to its initial scale.
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
	/// A <see cref="Space"/>
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
	public static void PunchScale(GameObject target, Hashtable args){
		//clean args:
		args = iTween.CleanArgs(args);
		
		//establish iTween
		args["type"]="punch";
		args["method"]="scale";
		args["easeType"]=EaseType.punch;
		Launch(target,args);
	}	
	
	#endregion
	
	#region Generate Targets
	
	//call correct set target method and set tween application delegate:
	void GenerateTargets(){
		switch (type) {
			case "move":
				switch (method) {
					case "to":
						GenerateMoveToTargets();
						apply = new ApplyTween(ApplyMoveToTargets);
					break;
					case "by":
					case "add":
						GenerateMoveByTargets();
						apply = new ApplyTween(ApplyMoveByTargets);
					break;
				}
			break;
			case "scale":
				switch (method){
					case "to":
						GenerateScaleToTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
					case "by":
						GenerateScaleByTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
					case "add":
						GenerateScaleAddTargets();
						apply = new ApplyTween(ApplyScaleToTargets);
					break;
				}
			break;
			case "rotate":
				switch (method) {
					case "to":
						GenerateRotateToTargets();
						apply = new ApplyTween(ApplyRotateToTargets);
					break;
					case "add":
						GenerateRotateAddTargets();
						apply = new ApplyTween(ApplyRotateAddTargets);
					break;
					case "by":
						GenerateRotateByTargets();
						apply = new ApplyTween(ApplyRotateAddTargets);
					break;				
				}
			break;
			case "shake":
				switch (method) {
					case "position":
						GenerateShakePositionTargets();
						apply = new ApplyTween(ApplyShakePositionTargets);
					break;		
					case "scale":
						GenerateShakeScaleTargets();
						apply = new ApplyTween(ApplyShakeScaleTargets);
					break;
					case "rotation":
						GenerateShakeRotationTargets();
						apply = new ApplyTween(ApplyShakeRotationTargets);
					break;
				}
			break;			
			case "punch":
				switch (method) {
					case "position":
						GeneratePunchPositionTargets();
						apply = new ApplyTween(ApplyPunchPositionTargets);
					break;	
					case "rotation":
						GeneratePunchRotationTargets();
						apply = new ApplyTween(ApplyPunchRotationTargets);
					break;	
					case "scale":
						GeneratePunchScaleTargets();
						apply = new ApplyTween(ApplyPunchScaleTargets);
					break;
				}
			break;
		}
	}
	
	void GenerateMoveToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=transform.localPosition;				
		}else{
			vector3s[0]=vector3s[1]=transform.position;
		}
		
		//to values:
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
	}
	
	void GenerateMoveByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.position;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}
	
	void GenerateScaleToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
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
	}
	
	void GenerateScaleByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
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
	}
	
	void GenerateScaleAddTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=vector3s[1]=transform.localScale;				

		//to values:
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
	}
	
	void GenerateRotateToTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		if (isLocal) {
			vector3s[0]=vector3s[1]=transform.localEulerAngles;				
		}else{
			vector3s[0]=vector3s[1]=transform.eulerAngles;
		}
		
		//to values:
		if (tweenArguments.Contains("rotation")) {
			vector3s[1]=(Vector3)tweenArguments["rotation"];
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
		vector3s[1]=new Vector3(clerp(vector3s[0].x,vector3s[1].x,1),clerp(vector3s[0].y,vector3s[1].y,1),clerp(vector3s[0].z,vector3s[1].z,1));
	}
	
	void GenerateRotateAddTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Rotate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.eulerAngles;
		
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}		
	
	void GenerateRotateByTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Rotate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=vector3s[1]=vector3s[3]=transform.eulerAngles;
		
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]+=Vector3.Scale((Vector3)tweenArguments["amount"],new Vector3(360,360,360));
		}else{
			if (tweenArguments.Contains("x")) {
				vector3s[1].x+=360 * (float)tweenArguments["x"];
			}
			if (tweenArguments.Contains("y")) {
				vector3s[1].y+=360 * (float)tweenArguments["y"];
			}
			if (tweenArguments.Contains("z")) {
				vector3s[1].z+=360 * (float)tweenArguments["z"];
			}
		}
	}		
	
	void GenerateShakePositionTargets(){
		//values holder [0] root value, [1] amount, [2] generated amount:
		vector3s=new Vector3[3];
		
		//root:
		vector3s[0]=transform.position;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}		
	
	void GenerateShakeScaleTargets(){
		//values holder [0] root value, [1] amount, [2] generated amount:
		vector3s=new Vector3[3];
		
		//root:
		vector3s[0]=transform.localScale;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}		
		
	void GenerateShakeRotationTargets(){
		//values holder [0] root value, [1] amount, [2] generated amount:
		vector3s=new Vector3[3];
		
		//root:
		vector3s[0]=transform.eulerAngles;
		
		//amount:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}	
	
	void GeneratePunchPositionTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=transform.position;
		vector3s[1]=vector3s[3]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}	
	
	void GeneratePunchRotationTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation, [3] previous value for Translate usage to allow Space utilization:
		vector3s=new Vector3[4];
		
		//from values:
		vector3s[0]=transform.eulerAngles;
		vector3s[1]=vector3s[3]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}		
	
	void GeneratePunchScaleTargets(){
		//values holder [0] from, [1] to, [2] calculated value from ease equation:
		vector3s=new Vector3[3];
		
		//from values:
		vector3s[0]=transform.localScale;
		vector3s[1]=Vector3.zero;
				
		//to values:
		if (tweenArguments.Contains("amount")) {
			vector3s[1]=(Vector3)tweenArguments["amount"];
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
	}
	
	#endregion
	
	#region Apply Targets
	
	void ApplyMoveToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			transform.localPosition=vector3s[2];		
		}else{
			transform.position=vector3s[2];
		}	
	}	
	
	void ApplyMoveByTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.Translate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}	
	
	void ApplyScaleToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.localScale=vector3s[2];	
	}
	
	void ApplyRotateToTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		if (isLocal) {
			transform.localRotation = Quaternion.Euler(vector3s[2]);
		}else{
			transform.rotation = Quaternion.Euler(vector3s[2]);
		};	
	}
	
	void ApplyRotateAddTargets(){
		//calculate:
		vector3s[2].x = ease(vector3s[0].x,vector3s[1].x,percentage);
		vector3s[2].y = ease(vector3s[0].y,vector3s[1].y,percentage);
		vector3s[2].z = ease(vector3s[0].z,vector3s[1].z,percentage);
		
		//apply:
		transform.Rotate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}	
	
	void ApplyShakePositionTargets(){
		//impact:
		if (percentage==0) {
			transform.Translate(vector3s[1],space);
		}
		
		//reset:
		transform.position=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.Translate(vector3s[2],space);	
	}	
	
	void ApplyShakeScaleTargets(){
		//impact:
		if (percentage==0) {
			transform.localScale=vector3s[1];
		}
		
		//reset:
		transform.localScale=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.localScale+=vector3s[2];
	}		
	
	void ApplyShakeRotationTargets(){
		//impact:
		if (percentage==0) {
			transform.Rotate(vector3s[1],space);
		}
		
		//reset:
		transform.eulerAngles=vector3s[0];
		
		//generate:
		float diminishingControl = 1-percentage;
		vector3s[2].x= UnityEngine.Random.Range(-vector3s[1].x*diminishingControl, vector3s[1].x*diminishingControl);
		vector3s[2].y= UnityEngine.Random.Range(-vector3s[1].y*diminishingControl, vector3s[1].y*diminishingControl);
		vector3s[2].z= UnityEngine.Random.Range(-vector3s[1].z*diminishingControl, vector3s[1].z*diminishingControl);

		//apply:
		transform.Rotate(vector3s[2],space);	
	}		
	
	void ApplyPunchPositionTargets(){
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.Translate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}		
	
	void ApplyPunchRotationTargets(){
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.Rotate(vector3s[2]-vector3s[3],space);

		//record:
		vector3s[3]=vector3s[2];
	}	
	
	void ApplyPunchScaleTargets(){
		//calculate:
		if(vector3s[1].x>0){
			vector3s[2].x = punch(vector3s[1].x,percentage);
		}else if(vector3s[1].x<0){
			vector3s[2].x=-punch(Mathf.Abs(vector3s[1].x),percentage); 
		}
		if(vector3s[1].y>0){
			vector3s[2].y=punch(vector3s[1].y,percentage);
		}else if(vector3s[1].y<0){
			vector3s[2].y=-punch(Mathf.Abs(vector3s[1].y),percentage); 
		}
		if(vector3s[1].z>0){
			vector3s[2].z=punch(vector3s[1].z,percentage);
		}else if(vector3s[1].z<0){
			vector3s[2].z=-punch(Mathf.Abs(vector3s[1].z),percentage); 
		}
		
		//apply:
		transform.localScale=vector3s[0]+vector3s[2];
	}		
	
	#endregion	
	
	#region Tween Steps
	
	IEnumerator TweenDelay(){
		delayStarted = Time.time;
		yield return new WaitForSeconds (delay);
	}	
	
	void TweenStart(){
		//run stab and anything else that doesn't loop?
		//setup curve crap?
		if(!loop){//only if this is not a loop
			ConflictCheck();
			GenerateTargets();
		}
		EnableKinematic();
		CallBack("onStart");
		isRunning = true;
	}
	
	IEnumerator TweenRestart(){
		if(delay > 0){
			delayStarted = Time.time;
			yield return new WaitForSeconds (delay);
		}
		loop=true;
		TweenStart();
	}	
	
	void TweenUpdate(){
		CallBack("onUpdate");
		apply();
		UpdatePercentage();		
	}
			
	void TweenComplete(){
		CallBack("onComplete");
		isRunning=false;
		
		//dial in percentage to 1 or 0 for final run:
		if(percentage>.5){
			percentage=1;
		}else{
			percentage=0;	
		}
		
		//apply dial in final run:
        apply();
		
		//loop or dispose?
		if(loopType==LoopType.none){
			Dispose();
		}else{
			TweenLoop();
		}
	}
	
	void TweenLoop(){
		DisableKinematic(); //give physics control again
		switch(loopType){
			case LoopType.loop:
				//rewind:
				percentage=0;
				runningTime=0;
				apply();
				
				//replay:
				StartCoroutine("TweenRestart");
				break;
			case LoopType.pingPong:
				reverse = !reverse;
				runningTime=0;
			
				//replay:
				StartCoroutine("TweenRestart");
				break;
		}
	}	
	
	#endregion

	#region Component Segments
	
	void Awake(){
		RetrieveArgs();
	}
	
	IEnumerator Start(){
		if(delay > 0){
			yield return StartCoroutine("TweenDelay");
		}
		TweenStart();
	}	
	
	void Update(){
		if(isRunning){
			if(!reverse){
				if(percentage<1f){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}else{
				if(percentage>0){
					TweenUpdate();
				}else{
					TweenComplete();	
				}
			}
		}
	}

	void OnEnable(){
		if(isRunning){
			EnableKinematic();
		}
		//resume delay:
		if(isPaused && delay>0){
			isPaused=false;
			ResumeDelay();
		}
	}

	void OnDisable(){
		DisableKinematic();
	}
	
	#endregion
	
	#region External Utilities
//	stops
//	pauses
//	completes
//	rewinds
//	counts
//	public static Hashtable Hash(params object[] args){
//		Hashtable hashTable = new Hashtable(args.Length/2);
//		if (args.Length %2 != 0) {
//			Debug.LogError("Tween Error: Hash requires an even number of arguments!"); 
//			return null;
//		}else{
//			int i = 0;
//			while(i < args.Length - 1) {
//				hashTable.Add(args[i], args[i+1]);
//				i += 2;
//			}
//			return hashTable;
//		}
//	}
	#endregion	
	
	#region Internal Helpers
	
	//catalog new tween and add component phase of iTween:
	static void Launch(GameObject target, Hashtable args){
		if(!args.Contains("id")){
			args["id"] = GenerateID();
		}
		if(!args.Contains("target")){
			args["target"] = target;
		}		
		tweens.Insert(0,args);
		target.AddComponent("iTween");
	}		
	
	//cast any accidentally supplied doubles and ints as floats as iTween only uses floats internally:
	static Hashtable CleanArgs(Hashtable args){
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
		
		return args;
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
				break;
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
					space = Defaults.space;
				}
			}			
		}else{
			space = Defaults.space;
		}
		
		if(tweenArguments.Contains("isLocal")){
			isLocal = (bool)tweenArguments["isLocal"];
		}else{
			isLocal = Defaults.isLocal;
		}
		
		//instantiates a cached ease equation reference:
		GetEasingFunction();
	}	
	
	//instantiates a cached ease equation refrence:
	void GetEasingFunction(){
		switch (easeType){
		case EaseType.easeInQuad:
			ease  = new EasingFunction(easeInQuad);
			break;
		case EaseType.easeOutQuad:
			ease = new EasingFunction(easeOutQuad);
			break;
		case EaseType.easeInOutQuad:
			ease = new EasingFunction(easeInOutQuad);
			break;
		case EaseType.easeInCubic:
			ease = new EasingFunction(easeInCubic);
			break;
		case EaseType.easeOutCubic:
			ease = new EasingFunction(easeOutCubic);
			break;
		case EaseType.easeInOutCubic:
			ease = new EasingFunction(easeInOutCubic);
			break;
		case EaseType.easeInQuart:
			ease = new EasingFunction(easeInQuart);
			break;
		case EaseType.easeOutQuart:
			ease = new EasingFunction(easeOutQuart);
			break;
		case EaseType.easeInOutQuart:
			ease = new EasingFunction(easeInOutQuart);
			break;
		case EaseType.easeInQuint:
			ease = new EasingFunction(easeInQuint);
			break;
		case EaseType.easeOutQuint:
			ease = new EasingFunction(easeOutQuint);
			break;
		case EaseType.easeInOutQuint:
			ease = new EasingFunction(easeInOutQuint);
			break;
		case EaseType.easeInSine:
			ease = new EasingFunction(easeInSine);
			break;
		case EaseType.easeOutSine:
			ease = new EasingFunction(easeOutSine);
			break;
		case EaseType.easeInOutSine:
			ease = new EasingFunction(easeInOutSine);
			break;
		case EaseType.easeInExpo:
			ease = new EasingFunction(easeInExpo);
			break;
		case EaseType.easeOutExpo:
			ease = new EasingFunction(easeOutExpo);
			break;
		case EaseType.easeInOutExpo:
			ease = new EasingFunction(easeInOutExpo);
			break;
		case EaseType.easeInCirc:
			ease = new EasingFunction(easeInCirc);
			break;
		case EaseType.easeOutCirc:
			ease = new EasingFunction(easeOutCirc);
			break;
		case EaseType.easeInOutCirc:
			ease = new EasingFunction(easeInOutCirc);
			break;
		case EaseType.linear:
			ease = new EasingFunction(linear);
			break;
		case EaseType.spring:
			ease = new EasingFunction(spring);
			break;
		case EaseType.bounce:
			ease = new EasingFunction(bounce);
			break;
		case EaseType.easeInBack:
			ease = new EasingFunction(easeInBack);
			break;
		case EaseType.easeOutBack:
			ease = new EasingFunction(easeOutBack);
			break;
		case EaseType.easeInOutBack:
			ease = new EasingFunction(easeInOutBack);
			break;
		}
	}
	
	//calculate percentage of tween based on time:
	void UpdatePercentage(){
		runningTime+=Time.deltaTime;
		if(reverse){
			percentage = 1 - runningTime/time;	
		}else{
			percentage = runningTime/time;	
		}
	}
	
	void CallBack(string callbackType){
		if (tweenArguments.Contains(callbackType) && !tweenArguments.Contains("isChild")) {
			//establish target:
			GameObject target;
			if (tweenArguments.Contains(callbackType+"Target")) {
				target=(GameObject)tweenArguments[callbackType+"Target"];
			}else{
				target=gameObject;	
			}
			
			//throw an error if a string wasn't passed for callback:
			if (tweenArguments[callbackType].GetType() == typeof(System.String)) {
				target.SendMessage((string)tweenArguments[callbackType],(object)tweenArguments[callbackType+"Params"],SendMessageOptions.DontRequireReceiver);
			}else{
				Debug.LogError("iTween Error: Callback method references must be passed as a String!");
				Destroy (this);
			}
		}
	}
	
	void Dispose(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable tweenEntry = (Hashtable)tweens[i];
			if ((string)tweenEntry["id"] == id){
				tweens.RemoveAt(i);
				break;
			}
		}
		Destroy(this);
	}	
	
	void ConflictCheck(){
		Component[] tweens = GetComponents(typeof(iTween));
		foreach (iTween item in tweens) {
			if(item.isRunning && item.type==type){
				switch(type){
					//exception for types that have "sub" methods, given the extreme method differences and lack of argumentative transform modifications per method:
					case "value":
					case "punch":
					case "shake":
						if(item.method == method){
							item.Dispose();
						}
						break;
					case "audio":
						if (item.audioSource==audioSource) {
							item.Dispose();
						}
						break;
				default:
					item.DisableKinematic(); //rushed isKinematic swapping to avoid sequence issues when handing off isKinematic status
					item.Dispose();
					break;
				}			
			}
		}
	}
	
	void EnableKinematic(){
		if(gameObject.GetComponent(typeof(Rigidbody))){
			if(!rigidbody.isKinematic){
				kinematic=true;
				rigidbody.isKinematic=true;
			}
		}
	}
	
	void DisableKinematic(){
		if(kinematic && rigidbody.isKinematic==true){
			kinematic=false;
			rigidbody.isKinematic=false;
		}
	}
	
	IEnumerator ResumeDelay(){	
		yield return StartCoroutine("TweenDelay");
		TweenStart();
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