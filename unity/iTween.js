/*iTweenAnimation/tween framework for Unity based off the mechanisims and solutions established in the Flash tween worldVersion 2.0.0*//*Licensed under the MIT LicenseCopyright (c) 2009-2010 Bob Berkebile, Patrick CorkumPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.http://itween.pixelplacement.comhttp://code.google.com/p/itween/*//*TERMS OF USE - EASING EQUATIONSOpen source under the BSD License.Copyright (c)2001 Robert PennerAll rights reserved.Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*///########//# VARS #//########static var tweens : Array = [];static var defaults : Hashtable = {"time":1,"delay":0,"transition":"easeInOutCubic","isLocal":false};private var transitions : Hashtable = {"easeInQuad":easeInQuad, "easeOutQuad":easeOutQuad,"easeInOutQuad":easeInOutQuad, "easeInCubic":easeInCubic, "easeOutCubic":easeOutCubic, "easeInOutCubic":easeInOutCubic, "easeInQuart":easeInQuart, "easeOutQuart":easeOutQuart, "easeInOutQuart":easeInOutQuart, "easeInQuint":easeInQuint, "easeOutQuint":easeOutQuint, "easeInOutQuint":easeInOutQuint, "easeInSine":easeInSine, "easeOutSine":easeOutSine, "easeInOutSine":easeInOutSine, "easeInExpo":easeInExpo, "easeOutExpo":easeOutExpo, "easeInOutExpo":easeInOutExpo, "easeInCirc":easeInCirc, "easeOutCirc":easeOutCirc, "easeInOutCirc":easeInOutCirc, "linear":linear, "spring":spring, "bounce":bounce, "easeInBack":easeInBack, "easeOutBack":easeOutBack, "easeInOutBack":easeInOutBack}; var id : String;var type : String;var method : String;var running : boolean;var time : float = defaults["time"];var delay : float = defaults["delay"];private var transition = linear;private var args : Hashtable;private var kinematicToggle : boolean;private var isLocal : boolean = defaults["isLocal"];private var runningTime : float = 0;private var percentage : float = 0;private var caluclatedFloat : float;private var prevVector3 : Vector3;private var calculatedVector3 : Vector3;
private var startVector2 : Vector3;private var startVector3 : Vector3;private var endVector3 : Vector3;
private var endVector2 : Vector3;private var startColor : Color;private var endColor : Color;
private var calculatedColor : Color;
private var impact : boolean;

//##################//# FADE REGISTERS #//##################

static function fadeTo(target : GameObject, args : Hashtable) : void{
	var tempColor : Color;
	if(target.renderer){
		tempColor=target.renderer.material.color;
	}else if(target.guiTexture){
		tempColor=target.guiTexture.color;
	}else if(target.guiText){
		tempColor=target.guiText.material.color;
	}
	tempColor.a=args["alpha"];
	args["color"]=tempColor;
	colorTo(target,args);
}

static function fadeFrom(target : GameObject, args : Hashtable) : void{
	var tempColor : Color;
	if(target.renderer){
		tempColor=target.renderer.material.color;
	}else if(target.guiTexture){
		tempColor=target.guiTexture.color;
	}else if(target.guiText){
		tempColor=target.guiText.material.color;
	}
	tempColor.a=args["alpha"];
	args["color"]=tempColor;
	colorFrom(target,args);
}

//###################//# COLOR REGISTERS #//###################

static function colorTo(target : GameObject, args : Hashtable) : void{
	//handle children
	if(!args.Contains("includeChildren") || args["includeChildren"]){
		for (var child : Transform in target.transform) {
			var argsCopy : Hashtable = args.Clone();
			argsCopy["isChild"]=true;			colorTo(child.gameObject,argsCopy);		}
	}
	
	args["target"]=target;	args["id"]=generateID();	args["type"]="color";	if(!args.Contains("method")){		args["method"]="to";	}
	if(!args.Contains("transition")){		args["transition"]="linear";	}	init(target,args);
}

static function colorFrom(target : GameObject, args : Hashtable) : void{
	args["method"]="from";
	colorTo(target,args);
}

//###################//# STAB REGISTERS #//###################

static function stab(target : GameObject, args : Hashtable):void{
	if(target.audio){
		target.audio.playOnAwake=false;
	}else{
		target.AddComponent(AudioSource);
		target.audio.playOnAwake = false;
	}
	if(args.Contains("clip")){
		target.audio.clip=args["clip"];
	}
	if(!target.audio.clip){
		Debug.LogError("iTween: There is no AudioClip to play!");
	}else{
		args["target"]=target;		args["id"]=generateID();		args["type"]="stab";
		init(target,args);	
	}
}


//###################//# AUDIO REGISTERS #//###################

static function audioTo(target : GameObject, args : Hashtable) : void{
	if(target.audio){
		args["target"]=target;		args["id"]=generateID();		args["type"]="audio";
		if(!args.Contains("method")){			args["method"]="to";		}
		if(!args.Contains("transition")){			args["transition"]="linear";		}		init(target,args);	
	}else{
		Debug.LogError("iTween: Requested target does not have an AudioSource component!");
	}
}

static function audioTo(target : AudioSource, args : Hashtable) : void{
	audioTo(target.gameObject,args);
}
static function audioFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	audioTo(target,args);}

static function audioFrom(target : AudioSource, args : Hashtable) : void{	args["method"]="from";	audioTo(target.gameObject,args);}
//###################//# SHAKE REGISTERS #//###################

static function shakePosition(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="shake";	args["method"]="position";	init(target,args);}static function shakeRotation(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="shake";	args["method"]="rotation";
	if(!args.Contains("isLocal")){		args["isLocal"]=true;	}	init(target,args);}static function shakeScale(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="shake";	args["method"]="scale";	init(target,args);}

//deprecated shake registers:
static function shake(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: shake() has been deprecated. Please investigate shakePosition(), shakeRotation() and shakeScale() for enhanced shake abilities!");}
static function shakeWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: shakeWorld() has been deprecated. Please investigate shakePosition(), shakeRotation() and shakeScale() for enhanced shake abilities!");}
//###################//# PUNCH REGISTERS #//###################static function punchPosition(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="punch";	args["method"]="position";	init(target,args);}static function punchRotation(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="punch";	args["method"]="rotation";
	if(!args.Contains("isLocal")){		args["isLocal"]=true;	}	init(target,args);}static function punchScale(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="punch";	args["method"]="scale";	init(target,args);}

//deprecated punch registers:
static function punchPositionWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: punchPositionWorld() has been deprecated. Please investigate punchPosition() and the 'isLocal' property!");}
static function punchRotationWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: punchRotationWorld() has been deprecated. Please investigate punchRotation() and the 'isLocal' property!");}
//##################//# MOVE REGISTERS #//##################static function moveTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="move";	if(!args.Contains("method")){		args["method"]="to";	}	init(target,args);}static function moveFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	moveTo(target,args);}static function moveAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	moveTo(target,args);}static function moveBy(target : GameObject, args : Hashtable) : void{	args["method"]="add";	moveTo(target,args);}

//deprecated move registers:
static function moveAddWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: moveAddWorld() has been deprecated. Please investigate moveAdd() and the 'isLocal' property!");}
static function moveByWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: moveByWorld() has been deprecated. Please investigate moveBy() and the 'isLocal' property!");}
static function moveFromWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: moveFromWorld() has been deprecated. Please investigate moveFrom() and the 'isLocal' property!");}
static function moveToWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: moveToWorld() has been deprecated. Please investigate moveTo() and the 'isLocal' property!");}//######################//# ROTATION REGISTERS #//######################static function rotateTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="rotate";	if(!args.Contains("method")){		args["method"]="to";	}	if(!args.Contains("isLocal")){		args["isLocal"]=true;	}	init(target,args);}static function rotateFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	rotateTo(target,args);}static function rotateAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	rotateTo(target,args);}static function rotateBy(target : GameObject, args : Hashtable) : void{	args["method"]="by";	rotateTo(target,args);}

//deprecated rotation registers:
static function rotateAddWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: rotateAddWorld() has been deprecated. Please investigate rotateAdd() and the 'isLocal' property!");}
static function rotateByWorld(target : GameObject, args : Hashtable) : void{Debug.LogError("iTween: rotateByWorld() has been deprecated. Please investigate rotateBy() and the 'isLocal' property!");}//###################//# SCALE REGISTERS #//###################static function scaleTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="scale";	if(!args.Contains("method")){		args["method"]="to";	}	init(target,args);}static function scaleFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	scaleTo(target,args);}static function scaleAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	scaleTo(target,args);}static function scaleBy(target : GameObject, args : Hashtable) : void{	args["method"]="by";	scaleTo(target,args);}//#################################//# PAUSE UTILITIES AND OVERRIDES # //#################################//pause all iTweens in GameObject:static function pause(target : GameObject) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		tween.enabled=false;	}}//pause all iTweens in GameObject of type:static function pause(target : GameObject, type : String) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var targetType : String = tween.type+tween.method;		targetType = targetType.Substring(0,type.length);		if(targetType==type.ToLower()){			tween.enabled=false;		}	}}//pause all iTweens in GameObject and children:static function pauseChildren(target : GameObject) : void{	pause(target);	for (var  child : Transform in target.transform) {		pauseChildren(child.gameObject);	}}//pause all iTweens in GameObject and children of type:static function pauseChildren(target : GameObject, type : String) : void{	pause(target,type);	for (var  child : Transform in target.transform) {		pauseChildren(child.gameObject,type);	}	}//pause all iTweens in scene:static function pauseAll() : void{	print(tweens.length);	for(var i : int=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		var target : GameObject = currentTween["target"];		pause(target);	}}//pause all iTweens in scene of type:static function pauseAll(type : String) : void{	var removeArray : Array = [];	var i : int;	var target : GameObject;	for(i=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		target  = currentTween["target"];		pause(target,type);	}}//##################################//# RESUME UTILITIES AND OVERRIDES # //##################################//resume all iTweens in GameObject:static function resume(target : GameObject) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		tween.enabled=true;	}}//resume all iTweens in GameObject of type:static function resume(target : GameObject, type : String) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var targetType : String = tween.type+tween.method;		targetType = targetType.Substring(0,type.length);		if(targetType==type.ToLower()){			tween.enabled=true;		}	}}//resume all iTweens in GameObject and children:static function resumeChildren(target : GameObject) : void{	resume(target);	for (var  child : Transform in target.transform) {		resumeChildren(child.gameObject);	}}//resume all iTweens in GameObject and children of type:static function resumeChildren(target : GameObject, type : String) : void{	resume(target,type);	for (var  child : Transform in target.transform) {		resumeChildren(child.gameObject,type);	}	}//resume all iTweens in scene:static function resumeAll() : void{	print(tweens.length);	for(var i : int=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		var target : GameObject = currentTween["target"];		resume(target);	}}//resume all iTweens in scene of type:static function resumeAll(type : String) : void{	var removeArray : Array = [];	var i : int;	var target : GameObject;	for(i=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		target  = currentTween["target"];		resume(target,type);	}}//#################################//# COUNT UTILITIES AND OVERRIDES # //#################################//count all iTweens in GameObject:static function count(target : GameObject) : int{	var tweens = target.GetComponents(iTween);	return tweens.length;}//count all iTweens in GameObject of type:static function count(target : GameObject, type : String) : int{	var tweenCount : int;	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var targetType : String = tween.type+tween.method;		targetType = targetType.Substring(0,type.length);		if(targetType==type.ToLower()){			tweenCount++;		}	}	return tweenCount;}//count all iTweens in scene:static function countAll() : int{	return tweens.length;}//count all iTweens in scene of type:static function countAll(type : String) : int{	var tweenCount : int;	for(var i : int=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		var currentType : String = currentTween["type"];		var currentMethod : String = currentTween["method"];		var targetType : String = currentType+currentMethod;		targetType = targetType.Substring(0,type.length);		if(targetType==type.ToLower()){			tweenCount++;		}	}	return tweenCount;}//################################//# STOP UTILITIES AND OVERRIDES # //################################//stop all iTweens in GameObject:static function stop(target : GameObject) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		tween.tweenDispose();	}}//stop all iTweens in GameObject of type:static function stop(target : GameObject, type : String) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var targetType : String = tween.type+tween.method;		targetType = targetType.Substring(0,type.length);		if(targetType==type.ToLower()){			tween.tweenDispose();		}	}}//stop all iTweens in GameObject and children:static function stopChildren(target : GameObject) : void{	stop(target);	for (var child : Transform in target.transform) {		stopChildren(child.gameObject);	}}//stop all iTweens in GameObject and children of type:static function stopChildren(target : GameObject, type : String) : void{	stop(target,type);	for (var child : Transform in target.transform) {		stopChildren(child.gameObject,type);	}}//stop all iTweens in scene:static function stopAll() : void{	for(var i : int=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		var target : GameObject = currentTween["target"];		var script : iTween = target.GetComponent(iTween);		Destroy(script);	}	tweens.Clear();}//stop all iTweens in scene of type:static function stopAll(type : String) : void{	var removeArray : Array = [];	var i : int;	var target : GameObject;	for(i=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		target  = currentTween["target"];		var targetType : String = (currentTween["type"] as String) + (currentTween["method"] as String);		if(targetType == type.ToLower()){			removeArray.Push(target);		}	}	for(i=0; i<removeArray.length; i++){		target = removeArray[i];		var script : iTween = target.GetComponent(iTween);		script.tweenDispose();	}}//######################//# INTERNAL UTILITIES #//######################static function init(target : GameObject, args :  Hashtable) : void{	tweens.Unshift(args);	target.AddComponent("iTween");}//#########################################//# INTERNAL RANDOM ID GENERATION UTILITY #//#########################################static function generateID() : String {	var strlen : int = 15;	var chars:String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";	var num_chars:Number = chars.length - 1;	var randomChar:String = "";	for (var i:int = 0; i < strlen; i++){		randomChar += chars[Mathf.Floor(Random.Range(0,num_chars))];	}	return randomChar;}//#######################################//# INTERNAL ARGUMENT RETRIEVAL UTILITY #//#######################################private function retrieveArgs() : void{	for(var i : int=0; i<tweens.length; i++){		if((tweens[i] as Hashtable)["target"] == gameObject){			args=tweens[i];			break;		}	}	id=args["id"];	type=args["type"];	method=args["method"];	if(args.Contains("time")){		time=args["time"];	}	if(args.Contains("delay")){		delay=args["delay"];	}	if(args.Contains("transition")){		transition = transitions[args["transition"]];	}else{		transition = transitions[defaults["transition"]];		}	if(args.Contains("isLocal")){		isLocal=args["isLocal"];	}}//#####################################//# INTERNAL KINEMATIC ENABLE UTILITY #//#####################################private function enableKinematic() : void{	if(gameObject.GetComponent(Rigidbody)){		if(!rigidbody.isKinematic){			kinematicToggle=true;			rigidbody.isKinematic=true;		}	}}//######################################//# INTERNAL KINEMATIC DISABLE UTILITY #//######################################private function disableKinematic() : void{	if(kinematicToggle){		rigidbody.isKinematic=false;	}}//########################################//# INTERNAL CONFLICT RESOLUTION UTILITY #//########################################private function conflictCheck() : void{	var tweens = GetComponents(iTween);	for (var tween : iTween in tweens) {		if(tween.running && tween.type==type){			switch (type){				//exception for types that have "sub" methods, given the extreme method differences and lack of argumentative transform modifications per method:				case "punch":;				case "shake":					if(tween.method == method){						tween.tweenDispose();					}				break;								default:					tween.tweenDispose();				break;			}		}	}}//######################################//# INTERNAL TARGET GENERATION UTILITY #//######################################private function generateTargets() : void{	switch (type){
		//color:		case "color":			//set foundation values by type:			if(renderer){
				startColor=renderer.material.color; 
			}else if(guiTexture){
				startColor=guiTexture.color; 
			}else if(guiText){
				startColor=guiText.material.color; 
			}else if(light){
				startColor=light.color;
			}
			endColor=startColor;
						//set augmented values:			if(args.Contains("color")){				endColor = args["color"];			}else{				if(args.Contains("r")){					endColor.r=args["r"];				}				if(args.Contains("g")){					endColor.g=args["g"];				}				if(args.Contains("b")){					endColor.b=args["b"];				}
				if(args.Contains("a")){					endColor.a=args["a"];				}			}		break;
		
		//stab:
		case "stab":
			if(args["volume"]){
				audio.volume=args["volume"];
			}
			if(args["pitch"]){
				audio.pitch=args["pitch"];
			}
			time=audio.clip.length/audio.pitch;
			//perform action here to avoid Update involvement:
			audio.PlayOneShot(audio.clip);
		break;
		
		//audio:
		case "audio":
			startVector2.x=audio.volume;
			startVector2.y=audio.pitch;
			endVector2.x=startVector2.x;
			endVector2.y=startVector2.y;
			
			if(args.Contains("volume")){				endVector2.x = args["volume"];			}
			if(args.Contains("pitch")){				endVector2.y = args["pitch"];			}
		break;
				//move:		case "move":			//set foundation values:			if(isLocal){				startVector3=transform.localPosition;			}else{				startVector3=transform.position;			}			endVector3=startVector3;			prevVector3=startVector3;						//set augmented values:			switch (method){				case "to":					if(args.Contains("position")){						endVector3 = args["position"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;												case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];						endVector3 += calculatedVector3;					}else{						if(args.Contains("x")){							caluclatedFloat = args["x"];							endVector3.x+=caluclatedFloat;						}						if(args.Contains("y")){							caluclatedFloat = args["y"];							endVector3.y+=caluclatedFloat;						}						if(args.Contains("z")){							caluclatedFloat = args["z"];							endVector3.z+=caluclatedFloat;						}					}				break;			}		break;				//punch:		case "punch":					switch (method){						case "position":					//set foundation values:					if(isLocal){						startVector3=transform.localPosition;					}else{						startVector3=transform.position;					}					endVector3=Vector3.zero;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;										case "rotation":					//set foundation values:					if(isLocal){						startVector3=transform.localEulerAngles;					}else{						startVector3=transform.eulerAngles;					}					endVector3=Vector3.zero;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;									case "scale":					//set foundation values:					startVector3=transform.localScale;					endVector3=Vector3.zero;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;				}		break;				//shake:		case "shake":					switch (method){						case "position":					//set foundation values:					if(isLocal){						startVector3=transform.localPosition;					}else{						startVector3=transform.position;					}					endVector3=Vector3.zero;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;										case "rotation":					//set foundation values:					if(isLocal){						startVector3=transform.localEulerAngles;					}else{						startVector3=transform.eulerAngles;					}					endVector3=Vector3.zero;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;									case "scale":					//set foundation values:					startVector3=transform.localScale;					endVector3=startVector3;					prevVector3=endVector3;						//set augmented values:					if(args.Contains("amount")){						endVector3 = args["amount"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;				}		break;				//rotation:		case "rotate":			//set foundation values:			if(isLocal){				startVector3=transform.localEulerAngles;			}else{				startVector3=transform.eulerAngles;			}			endVector3=startVector3;			prevVector3=startVector3;						//set augmented values:			switch (method){				case "to":					if(args.Contains("rotation")){						endVector3 = args["rotation"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}					calculatedVector3=Vector3(clerp(startVector3.x,endVector3.x,1),clerp(startVector3.y,endVector3.y,1),clerp(startVector3.z,endVector3.z,1));					endVector3=calculatedVector3;				break;												case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}					endVector3+=calculatedVector3;				break;								case "by":					method="add";					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}					calculatedVector3*=360;					endVector3+=calculatedVector3;				break;			}		break;				//scale:		case "scale":			//set foundation values:			startVector3=transform.localScale;			endVector3=startVector3;									//set augmented values:			switch (method){				case "to":					if(args.Contains("scale")){						endVector3 = args["scale"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;												case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}					endVector3+=calculatedVector3;				break;								case "by":					calculatedVector3=Vector3.one;					method="add";					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}					endVector3=Vector3(calculatedVector3.x*startVector3.x,calculatedVector3.y*startVector3.y,calculatedVector3.z*startVector3.z);				break;			}		break;	}}//#############################//# INTERNAL CALLBACK UTILITY #//#############################private function callBack(version : String) : void{	if(args.Contains(version) && !args.Contains("isChild")){		var target : GameObject;		if(args.Contains(version+"Target")){			target=args[version+"Target"];		}else{			target=gameObject;		}		target.SendMessage(args[version],args[version+"Params"],SendMessageOptions.DontRequireReceiver);	}	}//##########################//# TWEEN FROM APPLICATION #//##########################private function tweenFrom() : void{	if(method=="from"){		method="to";		generateTargets();		switch (type){
			//color:			case "color":				if(renderer){
					renderer.material.color = endColor;
				}else if(guiTexture){
					guiTexture.color=endColor; 
				}else if(guiText){
					guiText.material.color=endColor; 
				}else if(light){
					light.color=endColor;
				}
				args["color"]=startColor;			break;
			
			//audio:			case "audio":				audio.volume=endVector2.x;
				audio.pitch=endVector3.y;				args["volume"]=startVector2.x;
				args["pitch"]=startVector2.y;			break;
						//move:			case "move":				if(isLocal){					transform.localPosition=endVector3;				}else{					transform.position=endVector3;				}				args["position"]=startVector3;			break;						//rotation:			case "rotate":				if(isLocal){					transform.localEulerAngles=endVector3;				}else{					transform.eulerAngles=endVector3;				}				args["rotation"]=startVector3;			break;						//scale:			case "scale":				transform.localScale=endVector3;				args["scale"]=startVector3;			break;		}	}	}//###########################//# TWEEN START APPLICATION #//###########################private function tweenStart() : void{	callBack("onStart");	conflictCheck();	enableKinematic();	generateTargets();	running=true;}//############################//# TWEEN UPDATE APPLICATION #//############################private function tweenUpdate() : void{	switch (type){	
		//color:
		case "color":
			//tween:
			calculatedColor.r = transition(startColor.r,endColor.r,percentage); 
			calculatedColor.g = transition(startColor.g,endColor.g,percentage); 
			calculatedColor.b = transition(startColor.b,endColor.b,percentage); 
			calculatedColor.a = transition(startColor.a,endColor.a,percentage); 
			
			//apply:			if(renderer){
				renderer.material.color=calculatedColor; 
			}else if(guiTexture){
				guiTexture.color=calculatedColor; 
			}else if(guiText){
				guiText.material.color=calculatedColor; 
			}else if(light){
				light.color=calculatedColor;
			}
		break;
		
		//audio:
		case "audio":
			audio.volume = transition(startVector2.x,endVector2.x,percentage);
			audio.pitch = transition(startVector2.y,endVector2.y,percentage);
		break;
				//move:		case "move":			calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);			calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);			calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);			if(isLocal){				transform.Translate(calculatedVector3-prevVector3,Space.Self);			}else{				transform.Translate(calculatedVector3-prevVector3,Space.World);			}			prevVector3=calculatedVector3;		break;				//punch:		case "punch":			if(endVector3.x>0){				calculatedVector3.x = punch(endVector3.x,percentage);			}else if(endVector3.x<0){				calculatedVector3.x=-punch(Mathf.Abs(endVector3.x),percentage);				}			if(endVector3.y>0){				calculatedVector3.y=punch(endVector3.y,percentage);			}else if(endVector3.y<0){				calculatedVector3.y=-punch(Mathf.Abs(endVector3.y),percentage);				}			if(endVector3.z>0){				calculatedVector3.z=punch(endVector3.z,percentage);			}else if(endVector3.z<0){				calculatedVector3.z=-punch(Mathf.Abs(endVector3.z),percentage);				}						switch(method){				case "position":									if(isLocal){						transform.Translate(calculatedVector3-prevVector3,Space.Self);					}else{						transform.Translate(calculatedVector3-prevVector3,Space.World);					}										prevVector3=calculatedVector3;				break;								case "rotation":										if(isLocal){						transform.Rotate(calculatedVector3-prevVector3,Space.Self);					}else{						transform.Rotate(calculatedVector3-prevVector3,Space.World);					}											prevVector3=calculatedVector3;				break;								case "scale":															transform.localScale=startVector3+calculatedVector3;				break;			}		break;		
		//shake:		case "shake":			switch(method){
				//position:				case "position":									if(!impact){
						impact = true;	
						switch (isLocal){
							case true:
								transform.Translate(endVector3,Space.Self);
							break;
							case false:	
								transform.Translate(endVector3,Space.World);
							break;
						}
					}else{
						switch (isLocal){
							case true:
								transform.localPosition=startVector3;
							break;
							case false:	
								transform.position=startVector3;
							break;
						}
						
						calculatedFloat = 1-percentage;
						calculatedVector3.x = Random.Range(-endVector3.x*calculatedFloat, endVector3.x*calculatedFloat);
						calculatedVector3.y = Random.Range(-endVector3.y*calculatedFloat, endVector3.y*calculatedFloat);
						calculatedVector3.z = Random.Range(-endVector3.z*calculatedFloat, endVector3.z*calculatedFloat);
						
						switch (isLocal){
							case true:
								transform.Translate(calculatedVector3,Space.Self);
							break;
							case false:	
								transform.Translate(calculatedVector3,Space.World);
							break;
						}
					}				break;
				
				//rotation:
				case "rotation":									if(!impact){
						impact = true;	
						switch (isLocal){
							case true:
								transform.Rotate(endVector3,Space.Self);
							break;
							case false:	
								transform.Rotate(endVector3,Space.World);
							break;
						}
					}else{
						switch (isLocal){
							case true:
								transform.localEulerAngles=startVector3;
							break;
							case false:	
								transform.eulerAngles=startVector3;
							break;
						}
						
						calculatedFloat = 1-percentage;
						calculatedVector3.x = Random.Range(-endVector3.x*calculatedFloat, endVector3.x*calculatedFloat);
						calculatedVector3.y = Random.Range(-endVector3.y*calculatedFloat, endVector3.y*calculatedFloat);
						calculatedVector3.z = Random.Range(-endVector3.z*calculatedFloat, endVector3.z*calculatedFloat);
						
						switch (isLocal){
							case true:
								transform.Rotate(calculatedVector3,Space.Self);
							break;
							case false:	
								transform.Rotate(calculatedVector3,Space.World);
							break;
						}
					}				break;
				
				//scale:				case "scale":									if(!impact){
						impact = true;	
						transform.localScale=endVector3;
					}else{
						calculatedFloat = 1-percentage;
						if(endVector3.x != startVector3.x){
							calculatedVector3.x=(endVector3.x-startVector3.x)*calculatedFloat;
							transform.localScale.x=startVector3.x + Random.Range(-calculatedVector3.x,calculatedVector3.x);
						}
						if(endVector3.y != startVector3.y){
							calculatedVector3.y=(endVector3.y-startVector3.y)*calculatedFloat;
							transform.localScale.y=startVector3.y + Random.Range(-calculatedVector3.y,calculatedVector3.y);
						}
						if(endVector3.z != startVector3.z){
							calculatedVector3.z=(endVector3.z-startVector3.z)*calculatedFloat;
							transform.localScale.z=startVector3.z + Random.Range(-calculatedVector3.z,calculatedVector3.z);
						}
					}				break;			}		break;
				//rotate:		case "rotate":			switch(method){				case "to":					calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);					calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);					calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);										if(isLocal){						transform.localRotation = Quaternion.Euler(calculatedVector3);					}else{						transform.rotation = Quaternion.Euler(calculatedVector3);					}						break;								case "add":					calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);					calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);					calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);					if(isLocal){						transform.Rotate(calculatedVector3-prevVector3,Space.Self);					}else{						transform.Rotate(calculatedVector3-prevVector3,Space.World);					}							prevVector3=calculatedVector3;				break;			}		break;				case "scale":			calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);			calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);			calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);				transform.localScale=calculatedVector3;		break;	}	runningTime+=Time.deltaTime;	percentage=runningTime/time;	callBack("onUpdate");}//##############################//# TWEEN COMPLETE APPLICATION #//##############################private function tweenComplete() : void{	//value dial ins:	switch (type){		case "move":			if(isLocal){				transform.localPosition=endVector3;			}else{				transform.position=endVector3;			}		break;				case "rotate":			//Leaving this null for now; its difficult to figure out without imperfections.			//You can deal with oddly displayed but dead-on values; life isn't perfect either people.		break;	}	callBack("onComplete");	tweenDispose();	if(args.Contains("loopType")){		tweenLoop();	}}//##########################//# TWEEN LOOP APPLICATION #//##########################private function tweenLoop() : void{	args["method"]="to";	switch(args["loopType"]){		case "loop":			switch (type){
				//color:				case "color":					if(renderer){
						renderer.material.color=startColor; 
					}else if(guiTexture){
						guiTexture.color=startColor; 
					}else if(guiText){
						guiText.material.color=startColor; 
					}else if(light){
						light.color=startColor;
					}					args["color"]=endColor;
					args["includeChildren"]=false;					iTween.colorTo(gameObject,args);				break;
								//move:				case "move":					if(isLocal){						transform.localPosition=startVector3;					}else{						transform.position=startVector3;					}					args["position"]=endVector3;					iTween.moveTo(gameObject,args);				break;								//rotate:				case "rotate":					if(isLocal){						transform.localEulerAngles=startVector3;					}else{						transform.eulerAngles=startVector3;					}					args["rotation"]=endVector3;					iTween.rotateTo(gameObject,args);				break;								//scale:				case "scale":					transform.localScale=startVector3;					args["scale"]=endVector3;					iTween.scaleTo(gameObject,args);				break;			}		break;		case "pingPong":			switch (type){
				//color:				case "color":					args["color"]=startColor;
					args["includeChildren"]=false;					iTween.colorTo(gameObject,args);				break;
								//move:				case "move":					args["position"]=startVector3;					iTween.moveTo(gameObject,args);				break;									//rotate:				case "rotate":					args["rotation"]=startVector3;					iTween.rotateTo(gameObject,args);				break;								//scale:				case "scale":					args["scale"]=startVector3;					iTween.scaleTo(gameObject,args);				break;			}		break;	}}//#############################//# TWEEN DISPOSE APPLICATION #//#############################public function tweenDispose() : void{	for(var i : int=0; i<tweens.length; i++){		if((tweens[i] as Hashtable)["id"] == id){			tweens.RemoveAt(i);			break;		}	}	Destroy(this);}//##############//# COMPONENTS #//##############function Awake(){	retrieveArgs();}function Start(){	tweenFrom();	yield WaitForSeconds(delay);	tweenStart();}function Update(){	if(running){		if(percentage<=1){			tweenUpdate();		}else{			running=false;			tweenComplete();		}	}}function OnEnable(){	if(running){		enableKinematic();	}}function OnDisable(){	disableKinematic();}//##########//# CURVES #//##########private function linear(start:float,end:float,value:float):float{return Mathf.Lerp(start,end,value);}privateprivate function clerp(start:float,end:float,value:float):float{var min=0.0;var max=360.0;var half=Mathf.Abs((max-min)/2.0);var retval=0.0;var diff=0.0;if((end-start)<-half){diff=((max-start)+end)*value;retval=start+diff;}else if((end-start)>half){diff=-((max-end)+start)*value;retval=start+diff;}else retval=start+(end-start)*value;return retval;}privateprivate function spring(start:float,end:float,value:float):float{value=Mathf.Clamp01(value);value=(Mathf.Sin(value*Mathf.PI*(0.2+2.5*value*value*value))*Mathf.Pow(1-value,2.2)+value)*(1+(1.2*(1-value)));return start+(end-start)*value;}privateprivate function easeInQuad(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value+start;}privateprivate function easeOutQuad(start:float,end:float,value:float):float{value/=1;end-=start;return-end*value*(value-2)+start;}privateprivate function easeInOutQuad(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value+start;value--;return-end/2*(value*(value-2)-1)+start;};privateprivate function easeInCubic(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value+start;};privateprivate function easeOutCubic(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*(value*value*value+1)+start;};privateprivate function easeInOutCubic(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value+start;value-=2;return end/2*(value*value*value+2)+start;};privateprivate function easeInQuart(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value*value+start;};privateprivate function easeOutQuart(start:float,end:float,value:float):float{value/=1;value--;end-=start;return-end*(value*value*value*value-1)+start;};privateprivate function easeInOutQuart(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value*value+start;value-=2;return-end/2*(value*value*value*value-2)+start;};privateprivate function easeInQuint(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value*value*value+start;};privateprivate function easeOutQuint(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*(value*value*value*value*value+1)+start;};privateprivate function easeInOutQuint(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value*value*value+start;value-=2;return end/2*(value*value*value*value*value+2)+start;};privateprivate function easeInSine(start:float,end:float,value:float):float{end-=start;return-end*Mathf.Cos(value/1*(Mathf.PI/2))+end+start;};privateprivate function easeOutSine(start:float,end:float,value:float):float{end-=start;return end*Mathf.Sin(value/1*(Mathf.PI/2))+start;};privateprivate function easeInOutSine(start:float,end:float,value:float):float{end-=start;return-end/2*(Mathf.Cos(Mathf.PI*value/1)-1)+start;};privateprivate function easeInExpo(start:float,end:float,value:float):float{end-=start;return end*Mathf.Pow(2,10*(value/1-1))+start;};privateprivate function easeOutExpo(start:float,end:float,value:float):float{end-=start;return end*(-Mathf.Pow(2,-10*value/1)+1)+start;};privateprivate function easeInOutExpo(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*Mathf.Pow(2,10*(value-1))+start;value--;return end/2*(-Mathf.Pow(2,-10*value)+2)+start;};privateprivate function easeInCirc(start:float,end:float,value:float):float{value/=1;end-=start;return-end*(Mathf.Sqrt(1-value*value)-1)+start;};privateprivate function easeOutCirc(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*Mathf.Sqrt(1-value*value)+start;};privateprivate function easeInOutCirc(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return-end/2*(Mathf.Sqrt(1-value*value)-1)+start;value-=2;return end/2*(Mathf.Sqrt(1-value*value)+1)+start;};privateprivate function bounce(start:float,end:float,value:float):float{value/=1;end-=start;if(value<(1/2.75)){return end*(7.5625*value*value)+start;}else if(value<(2/2.75)){value-=(1.5/2.75);return end*(7.5625*(value)*value+.75)+start;}else if(value<(2.5/2.75)){value-=(2.25/2.75);return end*(7.5625*(value)*value+.9375)+start;}else{value-=(2.625/2.75);return end*(7.5625*(value)*value+.984375)+start;}}privateprivate function easeInBack(start:float,end:float,value:float):float{end-=start;value/=1;s=1.70158;return end*(value)*value*((s+1)*value-s)+start;}privateprivate function easeOutBack(start:float,end:float,value:float):float{s=1.70158;end-=start;value=(value/1)-1;return end*((value)*value*((s+1)*value+s)+1)+start;}privateprivate function easeInOutBack(start:float,end:float,value:float):float{s=1.70158;end-=start;value/=.5;if((value)<1){s*=(1.525);return end/2*(value*value*(((s)+1)*value-s))+start;}value-=2;s*=(1.525);return end/2*((value)*value*(((s)+1)*value+s)+2)+start;}privateprivate function punch(amplitude:float,value:float):float{var start=0;var end=0;var s:float;if(value==0){return start;}value/=1;if(value==1){return start+end;}var period=1*.3;if(amplitude<Mathf.Abs(end)){amplitude=end;s=period/4;}else{s=period/(2*Mathf.PI)*Mathf.Asin(end/amplitude);return(amplitude*Mathf.Pow(2,-10*value)*Mathf.Sin((value*1-s)*(2*Mathf.PI)/period)+end+start);}}