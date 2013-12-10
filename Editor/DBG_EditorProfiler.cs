using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DBG_EditorProfiler : UnityEditor.EditorWindow
{
	public Dictionary<string,AnimationCurve> WatchedData;


	List<Color> ColorRange;

	[MenuItem("gonTool/DBG_EditorProfiler")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(DBG_EditorProfiler));
	}

	
	void OnEnable ()
	{
		this.WatchedData = new Dictionary<string,AnimationCurve>();
		this.WatchedData["AdjustmentAngle"] = AnimationCurve.Linear(0,0,1,0);
		//this.WatchedData["Velocity"] = AnimationCurve.Linear(0,0,1,0);
		//this.WatchedData["PointOccluded"] = AnimationCurve.Linear(0,0,1,0);

		this.ColorRange = new List<Color>();
		this.ColorRange.Add(Color.green);
		this.ColorRange.Add(Color.yellow);
		this.ColorRange.Add(Color.cyan);
		this.ColorRange.Add(Color.blue);
		this.ColorRange.Add(Color.magenta);
	}
	
	
	void OnGUI() 
	{
		int i=0;

		foreach( KeyValuePair<string,AnimationCurve> kvp in this.WatchedData)
		{
			//	To force Refresh of GUI, we have to create a totaly new curve (from: http://answers.unity3d.com/questions/385889/refresh-curve-field-previews.html )
			AnimationCurve newCurve = new AnimationCurve();
			foreach(Keyframe currentKeyFrame in kvp.Value.keys)
				newCurve.AddKey(currentKeyFrame.time, currentKeyFrame.value ) ;


			//EditorGUI.CurveField(new Rect(0,128*i,1024,128),kvp.Key, newCurve);

			//EditorGUI.CurveField(new Rect(0,128*i,1024,128),kvp.Key, newCurve, this.ColorRange[i], new Rect(0,0,1024,128) );

			EditorGUILayout.CurveField( kvp.Key, newCurve, this.ColorRange[i], new Rect(0,0,1024,18),  GUILayout.Height(300) );

			i++;
		}

	}
	
	void Update()
	{
		
		refresh();
		//OnGUI();
	}
	

	void refresh()
	{
		if( EditorApplication.isPlaying )
		{
			int frame = Time.renderedFrameCount;

			//CameraControl comp = Camera.main.camera.GetComponent<CameraControl>() as CameraControl;			

			//	Angle
			//float value = Mathf.Abs( comp.gonCameraMotion.PreviousAngle );
			//this.WatchedData["AdjustmentAngle"].AddKey( (float) frame, (float)value );

			//this.WatchedData["Velocity"].AddKey( (float) frame, (float) comp.velocity );

			//this.WatchedData["PointOccluded"].AddKey( (float) frame, (float) comp.PointOccluded );

			this.Repaint();
		}
	}
	

}