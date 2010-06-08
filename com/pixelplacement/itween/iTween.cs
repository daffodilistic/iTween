//Notes:
//C# version will have 3 methods for setting up tweens: typical hashtable entry (to keep the JS interface simple), typical hashtable with the use of the Hash method for "easier" entry, and finally the same method Unity uses for GUILayout (i.e. iTween.y(20))s
//should move defaults use orient by default - check on JS version as well??
//took out lookDefaults - if correct needs to be done in JS version as well
//limit what is visible in autocomplete to simplify interface usage
//ensure JS version has corect default cameraFade object depth (.999999 insted of 999999)
//JS version rename tween start application comment duplication

/*
iTween
Animation/tween framework for Unity based off the mechanisims and solutions established in the Flash tween world
Version 2.0.0
*/

/*
Licensed under the MIT License
Copyright (c) 2009-2010 Bob Berkebile, Patrick Corkum
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
http://itween.pixelplacement.com
http://code.google.com/p/itween/
*/

/*
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using UnityEngine;
using System.Collections;
using System.Reflection;

public class iTween : MonoBehaviour {	
	public enum LoopType{
		none,
		loop,
		pingPong
	}
	
	public enum Transition{
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
	};
	
	//public vars:
	public string id, type, method;
	public bool running, paused;
	public static ArrayList tweens = new ArrayList();
	public static GameObject cameraFade;
	
	//public defaults:
	public static float defaultTime = 1;
	public static float defaultDelay = 0;
	public static Transition defaultTransition = Transition.easeInOutCubic;
	public static bool defaultLocal = false;
	public static bool moveDefaultLocal = false;
	public static bool moveDefaultOrientToPath = false;
	public static bool curveDefaultLocal = false;
	public static bool curveDefaultOrientToPath = true;
	public static bool curveDefaultClassic = false;
	public static float curveDefaultLookSpeed = 8;
	public static Transition curveDefaultTransition = Transition.easeInOutSine;
	public static bool rotateDefaultLocal = true;
	public static bool shakePositionDefaultLocal = false;
	public static bool shakeRotationDefaultLocal = false;
	public static bool punchPositionDefaultLocal = false;
	public static bool punchRotationDefaultLocal = true;
	public static Transition colorDefaultTransition = Transition.linear;
	public static Transition audioDefaultTransition = Transition.linear;
	public static float lookToUpdateDefaultLookSpeed = 3;	
	public static bool moveToUpdateDefaultLocal = false;
	public static int cameraFadeDefaultDepth = 999999;
	
	//private vars:
	float time;
	float delay;
	Transition transition;
	Hashtable args;
	bool kinematicToggle, isLocal, impact;
	float delayStartedTime, calculatedInt, runningTime, percentage, calculatedFloat, startFloat, endFloat;
	Vector3 prevVector3, calculatedVector3, startVector3, endVector3;
	Vector2 calculatedVector2, startVector2, endVector2;
	Color calculatedColor, startColor, endColor;
	Rect calculatedRect, startRect, endRect;
	int startInt, endInt;
	AudioSource audioSource;
	Vector3[] points;
	BezierPointInfo[] parsedpoints;
	
	//##################
	//# MOVE REGISTERS #
	//##################
	
	public static void MoveTo(GameObject target, Hashtable args){
		args["target"]=target;
		args["type"]="move";
		if(!args.Contains("id")){
			args["id"]=generateID();
		}
		if(!args.Contains("method")){
			args["method"]="to";
		}
		if(!args.Contains("isLocal")){
			args["isLocal"]=moveDefaultLocal;
		}
		init(target,args);
	}
	
	//#########################
	//# INTERNAL INIT UTILITY #
	//#########################
	static void init(GameObject target, Hashtable args){	
		tweens.Insert(0,args);
		target.AddComponent("iTween");
	}
	
	//#########################################
	//# INTERNAL RANDOM ID GENERATION UTILITY #
	//#########################################
	
	static string generateID(){
		int strlen = 15;
		string[] chars={"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9"};
		int num_chars = chars.Length - 1;
		string randomChar = "";
		for (int i = 0; i < strlen; i++){
			randomChar += chars[(int)Mathf.Floor(Random.Range(0,num_chars))];
		}
		return randomChar;
	}	
	
	//#######################################
	//# INTERNAL ARGUMENT RETRIEVAL UTILITY #
	//#######################################
	
	void retrieveArgs(){
		for (int i = 0; i < tweens.Count; i++) {
			Hashtable currentTween = (Hashtable)tweens[i];
			if ((GameObject)currentTween["target"] == gameObject) {
				args=currentTween;
				break;
			}
		}

		id=(string)args["id"];
		type=(string)args["type"];
		method=(string)args["method"];
		
		if(args.Contains("time")){
			time=(float)args["time"];
		}else{
			time=defaultTime;
		}
		
		if(args.Contains("delay")){
			delay=(float)args["delay"];
		}else{
			delay=defaultDelay;
		}
		
		if(args.Contains("transition")){
			transition = (Transition)args["transition"];
		}else{
			transition = defaultTransition; 
		}
		
		if(args.Contains("isLocal")){
			isLocal=(bool)args["isLocal"];
		}else{
			isLocal=defaultLocal;	
		}
	}
	
	
	//###########################
	//# BEZIER POINT INFO CLASS #
	//###########################
	private class BezierPointInfo{
		public Vector3 starting, intermediate, end;
	}
	
	//##################################
	//# INTERNAL HASH CREATION UTILITY #
	//##################################
	/// <summary>
	/// Helper utility for creating Hashtables for parameters through a simple list of alternating properties and values.  Offered as a cleaner option than traditional Hashtable creation in C# and intended to be passed into an iTween call as the second paramter of an iTween overload.  Final length of supplied paramters must be even.
	/// </summary>
	/// 
	/// <example>
	/// <code>
	/// iTween.moveTo(gameObject,iTween.Hash("x",4,"time",2,"delay",2));
	/// </code>
	/// </example>
	/// 
	/// <param name="args">
	/// A property,value list of arguments to be compiled into a Hashtable to describe how an iTween method should function. Length of supplied paramteres must be even.
	/// </param>
	/// 
	/// <returns>
	/// A <see cref="Hashtable"/>
	/// </returns>
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
	
	//##########################
	//# TWEEN FROM APPLICATION #
	//##########################
	void tweenFrom(){
		
	}
	
	//###########################
	//# TWEEN DELAY APPLICATION #
	//###########################
	
	IEnumerator tweenDelay(){
		delayStartedTime = Time.time;
		yield return new WaitForSeconds (delay);
	}
	
	//###########################
	//# TWEEN START APPLICATION #
	//###########################
	
	void tweenStart(){
		callBack("onStart");
		conflictCheck();
		enableKinematic();
		generateTargets();
		running=true;
		
		switch (type){
			case "stab":
				time=audio.clip.length/audio.pitch;
				audio.PlayOneShot(audio.clip);          
			break;
			case "curve":
				if(!points){
					points = Array(args["points"]);
					points.Unshift(startVector3);   
				}       
				parsedpoints = ParsePoints(points,args["pingPonged"]);  
			break;
		}
	}

	
	
	//##############
	//# COMPONENTS #
	//##############
	
	void Awake(){
		retrieveArgs();
	}
	
	IEnumerator Start(){
		tweenFrom();
		yield return StartCoroutine("tweenDelay");
		tweenStart();
	}
}