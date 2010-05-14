/**
 * iTween
 * Animation/tween framework for Unity based off the mechanisims and solutions established in the Flash tween world
 *
 * @author Bob Berkebile, Patrick Corkum
 * @version 2.0.0
 */

/*
Licensed under the MIT License

Copyright (c) 2009-2010 Bob Berkebile, Patrick Corkum

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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

//########//# VARS #//########static var tweens : Array = [];static var defaults : Hashtable = {"time":1,"delay":0,"transition":"easeInOutCubic","isLocal":false};private var transitions : Hashtable = {"easeInQuad":easeInQuad, "easeOutQuad":easeOutQuad,"easeInOutQuad":easeInOutQuad, "easeInCubic":easeInCubic, "easeOutCubic":easeOutCubic, "easeInOutCubic":easeInOutCubic, "easeInQuart":easeInQuart, "easeOutQuart":easeOutQuart, "easeInOutQuart":easeInOutQuart, "easeInQuint":easeInQuint, "easeOutQuint":easeOutQuint, "easeInOutQuint":easeInOutQuint, "easeInSine":easeInSine, "easeOutSine":easeOutSine, "easeInOutSine":easeInOutSine, "easeInExpo":easeInExpo, "easeOutExpo":easeOutExpo, "easeInOutExpo":easeInOutExpo, "easeInCirc":easeInCirc, "easeOutCirc":easeOutCirc, "easeInOutCirc":easeInOutCirc, "linear":linear, "spring":spring, "bounce":bounce, "easeInBack":easeInBack, "easeOutBack":easeOutBack, "easeInOutBack":easeInOutBack}; var id : String;var type : String;var method : String;var running : boolean;
var time : float = defaults["time"];var delay : float = defaults["delay"];private var transition = linear;private var args : Hashtable;private var kinematicToggle : boolean;private var isLocal : boolean = defaults["isLocal"];private var runningTime : float = 0;private var percentage : float = 0;private var caluclatedFloat : float;private var prevVector3 : Vector3;private var calculatedVector3 : Vector3;private var startVector3 : Vector3;private var endVector3 : Vector3;private var startColor : Color;private var endColor : Color;//#############//# REGISTERS #//#############
//movement:static function moveTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="move";	if(!args.Contains("method")){		args["method"]="to";	}	init(target,args);}
static function moveFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	moveTo(target,args);}static function moveAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	moveTo(target,args);}static function moveBy(target : GameObject, args : Hashtable) : void{	args["method"]="add";	moveTo(target,args);}
//rotation:static function rotateTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="rotate";	if(!args.Contains("method")){		args["method"]="to";	}
	if(!args.Contains("isLocal")){		args["isLocal"]=true;	}	init(target,args);}
static function rotateFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	rotateTo(target,args);}static function rotateAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	rotateTo(target,args);}static function rotateBy(target : GameObject, args : Hashtable) : void{	args["method"]="by";	rotateTo(target,args);}

//scale:static function scaleTo(target : GameObject, args : Hashtable) : void{	args["target"]=target;	args["id"]=generateID();	args["type"]="scale";	if(!args.Contains("method")){		args["method"]="to";	}	init(target,args);}
static function scaleFrom(target : GameObject, args : Hashtable) : void{	args["method"]="from";	scaleTo(target,args);}static function scaleAdd(target : GameObject, args : Hashtable) : void{	args["method"]="add";	scaleTo(target,args);}static function scaleBy(target : GameObject, args : Hashtable) : void{	args["method"]="by";	scaleTo(target,args);}//#############//# UTILITIES #//#############

//count:
//Add type counts for everything like stops!

//get number of iTweens running on every child of a GameObject:
//JUST RUN A count(target : GameObject) on each child!

//get number of iTweens running in scene:
static function count() : int{	return tweens.length;}

//get number of iTweens running on GameObject:
static function count(target : GameObject) : int{	var tweens = target.GetComponents(iTween);	return tweens.length;}

//stop:
//stop all iTweens running on every child of a GameObject:
//JUST RUN A stop(target : GameObject) on each child!//stop all iTweens running on every child of a GameObject of type:
//JUST RUN A stop(target : GameObject, type : String) on each child!
//stop all iTweens in GameObject:static function stop(target : GameObject) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var script : iTween = target.GetComponent(iTween);		script.tweenDispose();	}}//stop all iTweens in GameObject of type:static function stop(target : GameObject, type : String) : void{	var tweens = target.GetComponents(iTween);	for (var tween : iTween in tweens) {		var targetType : String = tween.type+tween.method;		if(targetType==type.ToLower()){			var script : iTween = target.GetComponent(iTween);			script.tweenDispose();		}	}}//stop all iTweens in scene:static function stopAll() : void{	for(var i : int=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		var target : GameObject = currentTween["target"];		var script : iTween = target.GetComponent(iTween);		script.disableKinematic();		Destroy(script);	}	tweens.Clear();}//stop all iTweens in scene of type:static function stopAll(type : String) : void{	var removeArray : Array = [];	var i : int;	var target : GameObject;	for(i=0; i<tweens.length; i++){		var currentTween : Hashtable = tweens[i];		target  = currentTween["target"];		var targetType : String = (currentTween["type"] as String) + (currentTween["method"] as String);		if(targetType == type.ToLower()){			removeArray.Push(target);		}	}	for(i=0; i<removeArray.length; i++){		target = removeArray[i];		var script : iTween = target.GetComponent(iTween);		script.tweenDispose();	}}//######################//# INTERNAL UTILITIES #//######################
static function init(target : GameObject, args :  Hashtable) : void{	tweens.Unshift(args);	target.AddComponent("iTween");}static function generateID() : String {	var strlen : int = 15;	var chars:String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";	var num_chars:Number = chars.length - 1;	var randomChar:String = "";	for (var i:int = 0; i < strlen; i++){		randomChar += chars[Mathf.Floor(Random.Range(0,num_chars))];	}	return randomChar;}private function retrieveArgs() : void{	for(var i : int=0; i<tweens.length; i++){		if((tweens[i] as Hashtable)["target"] == gameObject){			args=tweens[i];			break;		}	}	id=args["id"];	type=args["type"];	method=args["method"];	if(args.Contains("time")){		time=args["time"];	}	if(args.Contains("delay")){		delay=args["delay"];	}	if(args.Contains("transition")){		transition = transitions[args["transition"]];	}else{		transition = transitions[defaults["transition"]];		}	if(args.Contains("isLocal")){		isLocal=args["isLocal"];	}}private function enableKinematic() : void{	if(gameObject.GetComponent(Rigidbody)){		if(!rigidbody.isKinematic){			kinematicToggle=true;			rigidbody.isKinematic=true;		}	}}private function disableKinematic() : void{	if(kinematicToggle){		rigidbody.isKinematic=false;	}}private function conflictCheck() : void{	var tweens = GetComponents(iTween);	for (var tween : iTween in tweens) {		if(tween.running && tween.type==type){			tween.tweenDispose();		}	}}private function generateTargets() : void{	switch (type){
		//move:		case "move":			//set foundation values:			if(isLocal){				startVector3=transform.localPosition;			}else{				startVector3=transform.position;			}			endVector3=startVector3;			prevVector3=startVector3;
						//set augmented values:			switch (method){				case "to":					if(args.Contains("position")){						endVector3 = args["position"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;			
									case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];						endVector3 += calculatedVector3;					}else{						if(args.Contains("x")){							caluclatedFloat = args["x"];							endVector3.x+=caluclatedFloat;						}						if(args.Contains("y")){							caluclatedFloat = args["y"];							endVector3.y+=caluclatedFloat;						}						if(args.Contains("z")){							caluclatedFloat = args["z"];							endVector3.z+=caluclatedFloat;						}					}				break;			}		break;
		
		//rotation:		case "rotate":			//set foundation values:			if(isLocal){				startVector3=transform.localEulerAngles;			}else{				startVector3=transform.eulerAngles;			}			endVector3=startVector3;			prevVector3=startVector3;
						//set augmented values:			switch (method){				case "to":					if(args.Contains("rotation")){						endVector3 = args["rotation"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}
					calculatedVector3=Vector3(clerp(startVector3.x,endVector3.x,1),clerp(startVector3.y,endVector3.y,1),clerp(startVector3.z,endVector3.z,1));
					endVector3=calculatedVector3;				break;		
										case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}
					endVector3+=calculatedVector3;				break;
								case "by":
					method="add";					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}
					calculatedVector3*=360;
					endVector3+=calculatedVector3;				break;			}		break;
		
		//scale:
		case "scale":			//set foundation values:			startVector3=transform.localScale;			endVector3=startVector3;
									//set augmented values:			switch (method){				case "to":					if(args.Contains("scale")){						endVector3 = args["scale"];					}else{						if(args.Contains("x")){							endVector3.x=args["x"];						}						if(args.Contains("y")){							endVector3.y=args["y"];						}						if(args.Contains("z")){							endVector3.z=args["z"];						}					}				break;	
											case "add":					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}
					endVector3+=calculatedVector3;				break;
								case "by":
					calculatedVector3=Vector3.one;
					method="add";					if(args.Contains("amount")){						calculatedVector3 = args["amount"];					}else{						if(args.Contains("x")){							calculatedVector3.x=args["x"];						}						if(args.Contains("y")){							calculatedVector3.y=args["y"];						}						if(args.Contains("z")){							calculatedVector3.z=args["z"];						}					}
					endVector3=Vector3(calculatedVector3.x*startVector3.x,calculatedVector3.y*startVector3.y,calculatedVector3.z*startVector3.z);				break;			}		break;	}}

private function callBack(version : String) : void{	if(args.Contains(version)){		var target : GameObject;		if(args.Contains(version+"Target")){			target=args[version+"Target"];		}else{			target=gameObject;		}		target.SendMessage(args[version],args[version+"Params"]);	}	}//######################//# TWEEN APPLICATIONS #//######################
private function tweenFrom() : void{	if(method=="from"){		method="to";		generateTargets();		switch (type){
			//move:			case "move":				if(isLocal){					transform.localPosition=endVector3;				}else{					transform.position=endVector3;				}
				args["position"]=startVector3;			break;
			
			//rotation:
			case "rotate":				if(isLocal){					transform.localEulerAngles=endVector3;				}else{					transform.eulerAngles=endVector3;				}
				args["rotation"]=startVector3;			break;
			
			//scale:
			case "scale":				transform.localScale=endVector3;
				args["scale"]=startVector3;			break;		}	}	}private function tweenStart() : void{	callBack("onStart");	conflictCheck();	enableKinematic();	generateTargets();	running=true;}private function tweenUpdate() : void{	switch (type){
		//move:		case "move":			calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);			calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);			calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);			if(isLocal){				transform.Translate(calculatedVector3-prevVector3,Space.Self);			}else{				transform.Translate(calculatedVector3-prevVector3,Space.World);			}			prevVector3=calculatedVector3;		break;
		
		//rotate:		case "rotate":
			switch(method){
				case "to":
					calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);					calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);					calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);
					if(isLocal){						transform.localRotation = Quaternion.Euler(calculatedVector3);					}else{						transform.rotation = Quaternion.Euler(calculatedVector3);					}		
				break;
				
				case "add":
					calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);					calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);					calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);
					if(isLocal){						transform.Rotate(calculatedVector3-prevVector3,Space.Self);					}else{						transform.Rotate(calculatedVector3-prevVector3,Space.World);					}							prevVector3=calculatedVector3;
				break;
			}		break;
		
		case "scale":
			calculatedVector3.x = transition(startVector3.x,endVector3.x,percentage);			calculatedVector3.y = transition(startVector3.y,endVector3.y,percentage);			calculatedVector3.z = transition(startVector3.z,endVector3.z,percentage);	
			transform.localScale=calculatedVector3;
		break;	}	runningTime+=Time.deltaTime;	percentage=runningTime/time;	callBack("onUpdate");}
private function tweenComplete() : void{	//value dial ins:	switch (type){		case "move":			if(isLocal){				transform.localPosition=endVector3;			}else{				transform.position=endVector3;			}		break;
				case "rotate":
			//Leaving this null for now; its difficult to figure out wihtout imperfections. You can deal with oddly displayed but dead-on values; life isn't perfect.		break;	}	callBack("onComplete");	disableKinematic();	tweenDispose();
	if(args.Contains("loopType")){		tweenLoop();	}}private function tweenLoop() : void{	args["method"]="to";	switch(args["loopType"]){		case "loop":			switch (type){
				//move:				case "move":					if(isLocal){						transform.localPosition=startVector3;					}else{						transform.position=startVector3;					}					args["position"]=endVector3;					iTween.moveTo(gameObject,args);				break;
				
				//rotate:				case "rotate":					if(isLocal){						transform.localEulerAngles=startVector3;					}else{						transform.eulerAngles=startVector3;					}					args["rotation"]=endVector3;					iTween.rotateTo(gameObject,args);				break;
				
				//scale:
				case "scale":					transform.localScale=startVector3;					args["scale"]=endVector3;					iTween.scaleTo(gameObject,args);				break;			}		break;		case "pingPong":			switch (type){
				//move:				case "move":					args["position"]=startVector3;					iTween.moveTo(gameObject,args);				break;	
				
				//rotate:				case "rotate":					args["rotation"]=startVector3;					iTween.rotateTo(gameObject,args);				break;
				
				//scale:				case "scale":					args["scale"]=startVector3;					iTween.scaleTo(gameObject,args);				break;			}		break;	}}public function tweenDispose() : void{	for(var i : int=0; i<tweens.length; i++){		if((tweens[i] as Hashtable)["id"] == id){			tweens.RemoveAt(i);			break;		}	}	Destroy(this);}//##############//# COMPONENTS #//##############
function Awake(){	retrieveArgs();}function Start(){	tweenFrom();	yield WaitForSeconds(delay);	tweenStart();}function Update(){	if(running){		if(percentage<=1){			tweenUpdate();		}else{			running=false;			tweenComplete();		}	}}//##########//# CURVES #//##########
private function linear(start:float,end:float,value:float):float{return Mathf.Lerp(start,end,value);}privateprivate function clerp(start:float,end:float,value:float):float{var min=0.0;var max=360.0;var half=Mathf.Abs((max-min)/2.0);var retval=0.0;var diff=0.0;if((end-start)<-half){diff=((max-start)+end)*value;retval=start+diff;}else if((end-start)>half){diff=-((max-end)+start)*value;retval=start+diff;}else retval=start+(end-start)*value;return retval;}privateprivate function spring(start:float,end:float,value:float):float{value=Mathf.Clamp01(value);value=(Mathf.Sin(value*Mathf.PI*(0.2+2.5*value*value*value))*Mathf.Pow(1-value,2.2)+value)*(1+(1.2*(1-value)));return start+(end-start)*value;}privateprivate function easeInQuad(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value+start;}privateprivate function easeOutQuad(start:float,end:float,value:float):float{value/=1;end-=start;return-end*value*(value-2)+start;}privateprivate function easeInOutQuad(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value+start;value--;return-end/2*(value*(value-2)-1)+start;};privateprivate function easeInCubic(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value+start;};privateprivate function easeOutCubic(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*(value*value*value+1)+start;};privateprivate function easeInOutCubic(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value+start;value-=2;return end/2*(value*value*value+2)+start;};privateprivate function easeInQuart(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value*value+start;};privateprivate function easeOutQuart(start:float,end:float,value:float):float{value/=1;value--;end-=start;return-end*(value*value*value*value-1)+start;};privateprivate function easeInOutQuart(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value*value+start;value-=2;return-end/2*(value*value*value*value-2)+start;};privateprivate function easeInQuint(start:float,end:float,value:float):float{value/=1;end-=start;return end*value*value*value*value*value+start;};privateprivate function easeOutQuint(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*(value*value*value*value*value+1)+start;};privateprivate function easeInOutQuint(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*value*value*value*value*value+start;value-=2;return end/2*(value*value*value*value*value+2)+start;};privateprivate function easeInSine(start:float,end:float,value:float):float{end-=start;return-end*Mathf.Cos(value/1*(Mathf.PI/2))+end+start;};privateprivate function easeOutSine(start:float,end:float,value:float):float{end-=start;return end*Mathf.Sin(value/1*(Mathf.PI/2))+start;};privateprivate function easeInOutSine(start:float,end:float,value:float):float{end-=start;return-end/2*(Mathf.Cos(Mathf.PI*value/1)-1)+start;};privateprivate function easeInExpo(start:float,end:float,value:float):float{end-=start;return end*Mathf.Pow(2,10*(value/1-1))+start;};privateprivate function easeOutExpo(start:float,end:float,value:float):float{end-=start;return end*(-Mathf.Pow(2,-10*value/1)+1)+start;};privateprivate function easeInOutExpo(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return end/2*Mathf.Pow(2,10*(value-1))+start;value--;return end/2*(-Mathf.Pow(2,-10*value)+2)+start;};privateprivate function easeInCirc(start:float,end:float,value:float):float{value/=1;end-=start;return-end*(Mathf.Sqrt(1-value*value)-1)+start;};privateprivate function easeOutCirc(start:float,end:float,value:float):float{value/=1;value--;end-=start;return end*Mathf.Sqrt(1-value*value)+start;};privateprivate function easeInOutCirc(start:float,end:float,value:float):float{value/=.5;end-=start;if(value<1)return-end/2*(Mathf.Sqrt(1-value*value)-1)+start;value-=2;return end/2*(Mathf.Sqrt(1-value*value)+1)+start;};privateprivate function bounce(start:float,end:float,value:float):float{value/=1;end-=start;if(value<(1/2.75)){return end*(7.5625*value*value)+start;}else if(value<(2/2.75)){value-=(1.5/2.75);return end*(7.5625*(value)*value+.75)+start;}else if(value<(2.5/2.75)){value-=(2.25/2.75);return end*(7.5625*(value)*value+.9375)+start;}else{value-=(2.625/2.75);return end*(7.5625*(value)*value+.984375)+start;}}privateprivate function easeInBack(start:float,end:float,value:float):float{end-=start;value/=1;s=1.70158;return end*(value)*value*((s+1)*value-s)+start;}privateprivate function easeOutBack(start:float,end:float,value:float):float{s=1.70158;end-=start;value=(value/1)-1;return end*((value)*value*((s+1)*value+s)+1)+start;}privateprivate function easeInOutBack(start:float,end:float,value:float):float{s=1.70158;end-=start;value/=.5;if((value)<1){s*=(1.525);return end/2*(value*value*(((s)+1)*value-s))+start;}value-=2;s*=(1.525);return end/2*((value)*value*(((s)+1)*value+s)+2)+start;}privateprivate function punch(amplitude:float,value:float):float{var start=0;var end=0;var s:float;if(value==0){return start;}value/=1;if(value==1){return start+end;}var period=1*.3;if(amplitude<Mathf.Abs(end)){amplitude=end;s=period/4;}else{s=period/(2*Mathf.PI)*Mathf.Asin(end/amplitude);return(amplitude*Mathf.Pow(2,-10*value)*Mathf.Sin((value*1-s)*(2*Mathf.PI)/period)+end+start);}}