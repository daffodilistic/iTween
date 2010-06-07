//Notes:
//C# version will have 3 methods for setting up tweens: typical hashtable entry (to keep the JS interface simple), typical hashtable with the use of the Hash method for "easier" entry, and finally the same method Unity uses for GUILayout (i.e. iTween.y(20))s
//should move defaults use orient by default - check on JS version as well??
//took out lookDefaults - if correct needs to be done in JS version as well
//limit what is visible in autocomplete to simplify interface usage

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
	public enum loopType{
		none,
		loop,
		pingPong
	}
	
	public enum transitions{
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
	public static ArrayList tweens;
	public static Hashtable globalDefaults = new Hashtable();
	public static Hashtable moveDefaults = new Hashtable();
	public static Hashtable curveDefaults = new Hashtable();	
	public static Hashtable rotateDefaults = new Hashtable();	
	public static Hashtable shakePositionDefaults = new Hashtable();
	public static Hashtable shakeRotationDefaults = new Hashtable();
	public static Hashtable punchPositionDefaults = new Hashtable();
	public static Hashtable punchRotationDefaults = new Hashtable();
	public static Hashtable colorDefaults = new Hashtable();
	public static Hashtable audioDefaults = new Hashtable();
	public static Hashtable lookToUpdateDefaults = new Hashtable();
	public static Hashtable moveToUpdateDefaults = new Hashtable();
	public static Hashtable cameraFadeDefaults = new Hashtable();
	public static GameObject cameraFade;
	
	//private vars:
	private float time = (float)globalDefaults["time"];
	private float delay = (float)globalDefaults["delay"];
	private transitions transition;
	private Hashtable args;
	private bool kinematicToggle, isLocal, impact;
	private float delayStartedTime, calculatedInt, runningTime, percentage, calculatedFloat, startFloat, endFloat = 0;
	private Vector3 prevVector3, calculatedVector3, startVector3, endVector3;
	private Vector2 calculatedVector2, startVector2, endVector2;
	private Color calculatedColor, startColor, endColor;
	private Rect calculatedRect, startRect, endRect;
	private int startInt, endInt;
	private AudioSource audioSource;
	private Vector3[] points;
	private BezierPointInfo[] parsedpoints;
	
	static iTween(){
		globalDefaults.Add("time",1);
		globalDefaults.Add("delay",0);
		globalDefaults.Add("transition", transitions.easeInOutCubic);
		globalDefaults.Add("isLocal", false);
		moveDefaults.Add("isLocal", false);
		moveDefaults.Add("orientToPath", true);
		curveDefaults.Add("isLocal", false);
		curveDefaults.Add("orientToPath", true);
		curveDefaults.Add("classic", false);
		curveDefaults.Add("lookSpeed", 8);
		curveDefaults.Add("transition", transitions.easeInOutSine);
		rotateDefaults.Add("isLocal", true);
		shakePositionDefaults.Add("isLocal", false);
		shakeRotationDefaults.Add("isLocal", true);
		punchPositionDefaults.Add("isLocal", false);
		punchRotationDefaults.Add("isLocal", true);
		colorDefaults.Add("transition", transitions.linear);
		audioDefaults.Add("transition", transitions.linear);
		lookToUpdateDefaults.Add("lookSpeed", 3);
		moveToUpdateDefaults.Add("isLocal", false);
		cameraFadeDefaults.Add("defaultDepth", .999999);
	}
	
	//##################
	//# MOVE REGISTERS #
	//##################
	
	static void MoveTo(GameObject target, Hashtable args){
		args["target"]=target;
		
		if(!args.Contains("id")){
			args["id"]=generateID();
		}
		args["type"]="move";
		if(!args.Contains("method")){
			args["method"]="to";
		}
		if(!args.Contains("isLocal")){
			args["isLocal"]=moveDefaults["isLocal"];
		}
		init(target,args);
	}
	
	//#########################
	//# INTERNAL INIT UTILITY #
	//#########################
	
	static void init(GameObject target , Hashtable args){
		//tweens.Unshift(args);
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
}