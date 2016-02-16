using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using VisualVariableMonitoring;
//========================
//	Author: Cyrille PAULHIAC
//	Email: contact@cosmogonies.net
//	WebSite: www.cosmogonies.net
//========================

//namespace VisualVariableMonitoring
//	{
	//Margin layout abilities
	public enum eMarginSide {LeftSide=-1, NoMargin=0, RightSide=1};

	//Curves layout mode
	public enum eLayoutMode {Stacked=1, Overlapped=2};	//only accurate if more than one value to track right ?


	public class CG_VisualVariableMonitoring  : MonoBehaviour
	{	//The Component that needs to be attached to your playing Camera.

		public eMarginSide MarginSide = eMarginSide.LeftSide;		// Side for the Data's Margin
		public float MarginWidth = 0.15f; 							// in screen ratio

		public eLayoutMode LayoutMode;								// Curve drawing method

		public float Opacity = 1.0f;								// The Opacity of the Curves.

		Dictionary<string, DBG_DataCollector> WatchDict;     // All Trackables

		public bool AutoStart = false;

		private bool isRecording = false;
		private float TimeOfRecording = -1f;

		Dictionary<float, string> EventDict;	//seconds since start of coillecting data

		private GUIStyle EventStyle;


		void Start()
			{
			WatchDict = new Dictionary<string, DBG_DataCollector>();
			EventDict = new Dictionary<float, string>();

			//Find what objects to inspect TODO: maybe add a way not to parse everything (layers, tags) ?
			MonoBehaviour[] MonoBehaviourArray = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
			for ( int i = 0 ; i < MonoBehaviourArray.Length ; i++ )
				{
				MonoBehaviour currentBehaviour = MonoBehaviourArray[i];
				//Debug.Log ("Introspecting current class :" +currentBehaviour.name+" of type "+currentBehaviour.GetType().Name);
				System.Reflection.FieldInfo[] FieldArray = currentBehaviour.GetType().GetFields();
				for ( int j = 0 ; j < FieldArray.Length ; j++ )
					{
					System.Reflection.FieldInfo currentField = FieldArray[j];
					object[] CustomAttributeArray = currentField.GetCustomAttributes( true );
					if ( CustomAttributeArray.Length > 0 )
						{
						for ( int k = 0 ; k < CustomAttributeArray.Length ; k++ )
							{
							if ( CustomAttributeArray[k].GetType() == typeof( DBG_Track ) )
								{
								Debug.Log( "\tFound trackable variable @ class :" + currentBehaviour.name + " typeof " + currentBehaviour.GetType().Name + " FieldName = " + currentField.Name );
								WatchDict[currentField.Name] = new DBG_DataCollector( currentField, currentBehaviour, ((DBG_Track) CustomAttributeArray[k]).VariableColor );
								}
							}
						}
					}
				}

			if ( AutoStart )
				StartRecording();

			}

		void OnGUI()
		{
			//If No Margin is selected, exiting.
			if(this.MarginSide== eMarginSide.NoMargin)
				return;

			float FontHeight =18.0f;
			float PanelWidth = MarginWidth * Screen.width;

			float XPos=0.0f;
			if(this.MarginSide== eMarginSide.RightSide)
				XPos = Screen.width - (PanelWidth);
			if(this.MarginSide== eMarginSide.LeftSide)
				XPos = 0.0f;

			float SlicedPanelHeight = Screen.height / WatchDict.Keys.Count;

			// Displaying values analysis in Margin
			#region Values
			int VariableIteration = 1;
			foreach (KeyValuePair<string, DBG_DataCollector> kvp in WatchDict)
			{ 
				int LineIteration = 1;
				float YPos = Screen.height - SlicedPanelHeight*VariableIteration;

				DBG_DataCollector current = kvp.Value;

				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "["+current.Field.Name+"]", current.guiStyle );
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Cur="+current.getCurrentValue().ToString()+"]", current.guiStyle );
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Min="+current.MinimumValue.ToString()+"]", current.guiStyle );
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Max="+current.MaximumValue.ToString()+"]", current.guiStyle );
				LineIteration++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*LineIteration, (MarginWidth * Screen.width),FontHeight), "[Avrg="+current.Average.ToString()+"]", current.guiStyle );
				LineIteration++;
				LineIteration++;

				VariableIteration++;
			}
			#endregion

			#region Footer
			this.Opacity = GUI.HorizontalSlider (new Rect (XPos, Screen.height - FontHeight * 3, PanelWidth, FontHeight), this.Opacity, 0.0f, 1.0f);
			if( GUI.Button( new Rect(XPos, Screen.height - FontHeight*2, PanelWidth*0.5f,FontHeight), this.LayoutMode.ToString() ) )
			{
				if(this.LayoutMode == eLayoutMode.Stacked)
					this.LayoutMode = eLayoutMode.Overlapped;
				else
					this.LayoutMode = eLayoutMode.Stacked;
			}

			if ( GUI.Button( new Rect( XPos + PanelWidth * 0.5f, Screen.height - FontHeight * 2, PanelWidth * 0.5f, FontHeight ), (this.isRecording ? "STOP" : "START") ) )
			{
				ToggleRecordingState( !this.isRecording );
			}
			
			if( GUI.Button( new Rect(XPos, Screen.height - FontHeight, PanelWidth,FontHeight), "Clear" ) )
			{
				foreach (KeyValuePair<string, DBG_DataCollector> kvp in WatchDict)
				{
					kvp.Value.clearData();
				}
			}
			#endregion

			//Displaying Events
			foreach ( KeyValuePair<float, string> kvp in EventDict )
				{
				float eventDuration = kvp.Key;
				//we have to calculate the currrent rtation the duration represent:
				float currentRatio = eventDuration / GetCurrentRecordDuration();
				DisplayEvent( kvp.Value, currentRatio );
				}
		}


		void LateUpdate()
		{
			if ( this.isRecording )
				{
				//Adding current Values into the WatchDict.
				foreach ( KeyValuePair<string, DBG_DataCollector> kvp in WatchDict )
					{
					float currentValue = (float) kvp.Value.Field.GetValue( kvp.Value.Behaviour ); //TODO: be sure the cast is possible
					kvp.Value.addValue( currentValue );
					}

				//Drawing the curves :
				if ( this.Opacity > 0.0f )
					{
					int i = 0;
					foreach ( KeyValuePair<string, DBG_DataCollector> kvp in WatchDict )
						{
						DrawCurve( kvp.Value, i, WatchDict.Keys.Count, this.Opacity );
						i++;
						}
					}
				}
		}


		public void StartRecording()
			{
			ToggleRecordingState( true );
			}
		public void EndRecording()
			{
			ToggleRecordingState( false );
			}
		void ToggleRecordingState( bool StateToVerride )
			{
			this.isRecording = StateToVerride;
			if ( isRecording )
				{
				TimeOfRecording = Time.time;
				}
			}
		float GetCurrentRecordDuration()
			{
			return Time.time - this.TimeOfRecording;
			}


		public void RegisterEvent( string EventName )
			{
			//if ( this.isRecording )
			if ( true )
				{
				float EventDateSinceRecording = GetCurrentRecordDuration();
				EventDict[EventDateSinceRecording] = EventName;
				}
			else
				{
				UnityEngine.Debug.LogWarning("User try to register an event outside a recording window, event NOT registered, aborting");
				}
			}








		void DrawCurve( DBG_DataCollector _DataCollector, int _SliceIteration, int _SliceCount, float _Opacity )
			{   //Draw the curve for the given DataCollector
			float XPosAsRatio = 0.0f;
			float value = 0.0f;

			////////////////////////////////////////////////////////////float SlicedPanelHeight = Screen.height / this.WatchDict.Keys.Count;

			List<Vector3> ViewPortBuffer = new List<Vector3>();

			Color TheColor = _DataCollector.VariableColor;
			if ( _Opacity < 1.0f )
				TheColor = new Color( TheColor.r, TheColor.g, TheColor.b, _Opacity );

			for ( int i = 0 ; i < _DataCollector.Data.Count ; i++ )
				{
				value = _DataCollector.Data[i];

				//Determining the XPos as a ratio for the screen:
				if ( this.MarginSide == eMarginSide.RightSide )
					XPosAsRatio = Mathf.Lerp( 0.0f, 1.0f - MarginWidth, (float) i / (_DataCollector.Data.Count - 1) );
				else if ( this.MarginSide == eMarginSide.LeftSide )
					XPosAsRatio = Mathf.Lerp( MarginWidth, 1.0f, (float) i / (_DataCollector.Data.Count - 1) );
				else
					XPosAsRatio = Mathf.Lerp( 0.0f, 1.0f, (float) i / (_DataCollector.Data.Count - 1) );

				//Determining the YPos as a ratio for the screen:
				float YPosAsRatio = 0.0f;
				float range = _DataCollector.MaximumValue - _DataCollector.MinimumValue; // range fit 100% of SlicedPanelHeight and is 1.0f as ratio
				YPosAsRatio = (value - _DataCollector.MinimumValue) / range;

				Vector3 Point2D_Value = new Vector3( XPosAsRatio, YPosAsRatio, Camera.main.nearClipPlane );
				ViewPortBuffer.Add( Point2D_Value );

				if ( i > 0 )
					{
					Vector3 Point3D_ValuePrevious = Camera.main.ViewportToWorldPoint( ViewPortBuffer[i - 1] );
					Vector3 Point3D_Value = Camera.main.ViewportToWorldPoint( Point2D_Value );

					Debug.DrawLine( Point3D_ValuePrevious, Point3D_Value, TheColor, 0f );   //duration à 0f(seconds) => 1 frame
					}
				}
			}

		/*
		void DrawColumns()
		{

			float ratio = 0.0f;
			float value = 0.0f;

			//float ratioPrevious = 0.0f;
			//float valuePrevious = 0.0f;

			Vector3 Point2D_ValuePrevious;

			for(int i=0; i< this.DataValue.Count; i++)
			{
				//ratioPrevious = ratio;
				//valuePrevious = value;

				ratio =  Mathf.Lerp(MarginWidth,1.0f-MarginWidth, (float) i / (this.DataValue.Count -1) ) ;

				value = this.DataValue[i];
				float valueRatio = value/this.MaximumValue;

				Vector3 Point2D_Base = new Vector3(ratio, 0.0f, Camera.main.nearClipPlane*2  );
				Vector3 Point2D_Value = new Vector3(ratio, valueRatio, Camera.main.nearClipPlane*2  );

				Vector3 Point3D_Base = Camera.main.ViewportToWorldPoint(Point2D_Base);
				Vector3 Point3D_Value = Camera.main.ViewportToWorldPoint(Point2D_Value);

				Debug.DrawLine(Point3D_Base, Point3D_Value, Color.yellow,0f);	//duration à 0f(seconds) => 1 frame

			}
		}
		*/

			/*
		void DrawEvent()
			{
			float angle = -90f;
			Matrix4x4 m1 = Matrix4x4.identity;
			Matrix4x4 m2 = Matrix4x4.identity;
			m2.SetTRS( new Vector3( 200, 200, 0 ), Quaternion.Euler( 0, 0, angle ), Vector3.one );
			angle += 10f * Time.deltaTime; // aux is just to see it spin
			if ( angle > 360f )
				angle = 0.0f;
			m1.SetTRS( new Vector3( -50, -50, 0 ), Quaternion.identity, Vector3.one );
			GUI.matrix = m2 * m1;
			GUI.Box( new Rect( 0, 0, 100, 100 ), "Hello My Friends!" );
			GUI.matrix = Matrix4x4.identity;
			}
			*/

		void DisplayEvent(string _Label, float _XPosAsRatio)
			{
			int boxWidth = 132;


			float Yposition = Screen.height - (boxWidth*0.5f);

			float PanelWidth = MarginWidth * Screen.width;

			float Xposition = (Screen.width- PanelWidth) * _XPosAsRatio ;
			if ( MarginSide == eMarginSide.LeftSide )
				Xposition = PanelWidth + Xposition;

			GUIStyle EventStyle = new GUIStyle( GUI.skin.box );
			EventStyle.alignment = TextAnchor.MiddleLeft;
			EventStyle.padding.left = 4;
			//EventStyle.padding.top = 0;
			//EventStyle.normal.background = null;

			//GUI.Box( new Rect( Xposition, Yposition, boxWidth, 24 ), "-=> " + _Label, EventStyle );
			 //Rotate by angle
			float angle = -90f;
			Matrix4x4 m1 = Matrix4x4.identity;
			Matrix4x4 m2 = Matrix4x4.identity;
			m2.SetTRS( new Vector3( Xposition, Yposition, 0 ), Quaternion.Euler( 0, 0, angle ), Vector3.one );
			angle += 10f * Time.deltaTime; // aux is just to see it spin
			if ( angle > 360f )
				angle = 0.0f;
			m1.SetTRS( new Vector3( -50, -50, 0 ), Quaternion.identity, Vector3.one );
			GUI.matrix = m2 * m1;
			GUI.Box( new Rect( 0, 0, boxWidth, 24 ), "-=> "+_Label , EventStyle );
			GUI.matrix = Matrix4x4.identity;
			
			}

		}
	//}







