using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;



namespace CG_VisualVariableMonitoring
{
	public enum eMarginSide {LeftSide=-1, NoMargin=0, RightSide=1};

	public enum eLayoutMode {Stacked=1, Overlapped=2};
	
	//public enum eDrawingStyle {Columns=1, Curve=2};

	public class DBG_DataCollector
	{
		internal System.Reflection.FieldInfo Field;
		internal MonoBehaviour Behaviour;

		internal Color VariableColor = Color.white;

		public List<float> Data;

		public float MaximumValue= 0.0f;
		public float MinimumValue= 0.0f;

		public float Average= 0.0f;

		public DBG_DataCollector(System.Reflection.FieldInfo _Field, MonoBehaviour _Behaviour, Color _Color)
		{
			this.Field = _Field;
			this.Behaviour = _Behaviour;
			this.VariableColor = _Color;
			Data = new List<float>();
		}

		public void addValue(float _NewValue)
		{
			if( _NewValue > this.MaximumValue ) 
				this.MaximumValue = _NewValue;
			if( _NewValue < this.MinimumValue ) 
				this.MinimumValue = _NewValue;

			this.Data.Add(_NewValue);

			float sum=0.0f;
			for(int i=0; i< this.Data.Count; i++)
				sum+=this.Data[i];
			this.Average = sum / this.Data.Count;
		}

		public float getCurrentValue()
		{
			if(this.Data.Count==0)//First frame or we just done a clear
				return 0.0f;
			return this.Data[ this.Data.Count-1 ];
		}

		public void clearData()
		{
			this.Data.Clear ();
			this.MaximumValue= 0.0f;
			this.MinimumValue= 0.0f;
			this.Average= 0.0f;
		}

	}



	public class CG_VisualVariableMonitoring  : MonoBehaviour
	{
		public eMarginSide MarginSide = eMarginSide.LeftSide;
		public float MarginWidth = 0.1f; // in screen ratio

		public eLayoutMode LayoutMode;

		public bool AbsoluteMode =true;

		public Dictionary<string, DBG_DataCollector> WatchDict;

		float GUI_Opacity = 1.0f;
		
		public CG_VisualVariableMonitoring()
		{
			this.WatchDict = new Dictionary<string, DBG_DataCollector>();
			//this.LayoutMode = eLayoutMode.Stacked;
		}

		void Start()
		{
			//find tagged public fields:

			//Find what objects to inspect TODO: maybe add a way not to parse everything (layers, tags) ?
			MonoBehaviour[] MonoBehaviourArray =  UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

			//TOADD: Maybe also detect class instances

			for (int i = 0; i < MonoBehaviourArray.Length; i ++) 
			{
				//Debug.Log ( MonoBehaviourArray[i].name );
				MonoBehaviour currentBehaviour = MonoBehaviourArray[i];
				//Debug.Log ("Introspecting current class :" +currentBehaviour.name+" of type "+currentBehaviour.GetType().Name);

				System.Reflection.FieldInfo[] FieldArray = currentBehaviour.GetType().GetFields();
				for (int j = 0; j < FieldArray.Length; j ++) 
				{
					System.Reflection.FieldInfo currentField = FieldArray[j];

					object[] CustomAttributeArray = currentField.GetCustomAttributes(true);
					if( CustomAttributeArray.Length>0 )
					{

						for (int k = 0; k < CustomAttributeArray.Length; k ++) 
						{
							//Debug.Log("Found attributes on attribute "+currentField.Name);
							
							if( CustomAttributeArray[k].GetType() == typeof(DBG_Track) )
							{
								//Debug.Log ("\tFound trackable variable @ class :" +currentBehaviour.name+" typeof "+currentBehaviour.GetType().Name +" FieldName = "+ currentField.Name);
								this.WatchDict[currentField.Name] = new DBG_DataCollector(currentField, currentBehaviour,  ((DBG_Track) CustomAttributeArray[k]).VariableColor);
							}
						}
					}
					
				}
			}

		}

		void OnGUI()
		{
			//If No Margin is selected, exiting.
			if(this.MarginSide== eMarginSide.NoMargin)
				return;

			float FontHeight =18.0f;
			float PanelWidth = MarginWidth * Screen.width;

			float XPos=0.0f;
			if(this.MarginSide== eMarginSide.LeftSide)
				XPos = Screen.width - (PanelWidth);
			if(this.MarginSide== eMarginSide.RightSide)
				XPos = 0.0f;

			float SlicedPanelHeight =  (1f/this.WatchDict.Keys.Count) * Screen.height;

			// Displaying values analysis in Margin
			int j=1;
			foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict)
			{
				int i=1;
				float YPos = Screen.height - SlicedPanelHeight*j;

				DBG_DataCollector current = kvp.Value;

				GUIStyle TextStyle = new GUIStyle();
				TextStyle.normal.textColor = current.VariableColor;

				GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MarginWidth * Screen.width),FontHeight), "["+current.Field.Name+"]",TextStyle);
				i++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MarginWidth * Screen.width),FontHeight), "[Cur="+current.getCurrentValue().ToString()+"]",TextStyle);
				i++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MarginWidth * Screen.width),FontHeight), "[Min="+current.MinimumValue.ToString()+"]",TextStyle);
				i++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MarginWidth * Screen.width),FontHeight), "[Max="+current.MaximumValue.ToString()+"]",TextStyle);
				i++;
				GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MarginWidth * Screen.width),FontHeight), "[Avrg="+current.Average.ToString()+"]",TextStyle);
				i++;
				i++;

				j++;
			}

			//Footer
			this.GUI_Opacity = GUI.HorizontalSlider (new Rect (XPos, Screen.height - FontHeight * 2, PanelWidth, FontHeight), this.GUI_Opacity, 0.0f, 1.0f);
			if( GUI.Button( new Rect(XPos, Screen.height - FontHeight, PanelWidth*0.5f,FontHeight), this.LayoutMode.ToString() ) )
			{
				if(this.LayoutMode == eLayoutMode.Stacked)
					this.LayoutMode = eLayoutMode.Overlapped;
				else
					this.LayoutMode = eLayoutMode.Stacked;
			}
			if( GUI.Button( new Rect(XPos+ PanelWidth*0.5f, Screen.height - FontHeight, PanelWidth*0.5f,FontHeight), "Clear" ) )
			{
				foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict)
				{
					kvp.Value.clearData();
				}
			}
		}


		void LateUpdate()
		{
			//Adding current Values into the WatchDict.
			foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict) 
			{
				float currentValue = (float) kvp.Value.Field.GetValue (kvp.Value.Behaviour); //TODO: be sure the cast is possible

				if(this.AbsoluteMode)
					currentValue = Mathf.Abs (currentValue);

				kvp.Value.addValue(currentValue);
			}

			if (this.GUI_Opacity > 0.0f) 
			{
				int i = 0;
				foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict) 
				{
					DrawCurve (kvp.Value, i, this.WatchDict.Keys.Count, this.GUI_Opacity);
					i++;
				}
			}
		}






		void DrawCurve(DBG_DataCollector _DataCollector, int _SliceIteration, int _SliceCount, float _Opacity)
		{
			float ratio = 0.0f;
			float value = 0.0f;
			
			List< Vector3 > ViewPortBuffer = new List<Vector3>();

			Color TheColor = _DataCollector.VariableColor;
			if(_Opacity<1.0f)
				TheColor = new Color(TheColor.r, TheColor.g, TheColor.b, _Opacity);

			for(int i=0; i< _DataCollector.Data.Count; i++)
			{
				//ratio =  Mathf.Lerp(MarginWidth,1.0f-MarginWidth, (float) i / (_DataCollector.Data.Count -1) ) ;
				//ratio =  Mathf.Lerp(0.0f,1.0f-MarginWidth, (float) i / (_DataCollector.Data.Count -1) ) ;

				if(this.MarginSide == eMarginSide.LeftSide)
					ratio =  Mathf.Lerp(0.0f,1.0f-MarginWidth, (float) i / (_DataCollector.Data.Count -1) ) ;
				else if(this.MarginSide == eMarginSide.RightSide)
					ratio =  Mathf.Lerp(MarginWidth,1.0f, (float) i / (_DataCollector.Data.Count -1) ) ;
				else
					ratio =  Mathf.Lerp(0.0f,1.0f, (float) i / (_DataCollector.Data.Count -1) ) ;
				
				value = _DataCollector.Data[i];

				float valueRatio = 0.0f;
				if(_DataCollector.MaximumValue!=0.0f)
					valueRatio = value/_DataCollector.MaximumValue;

				if( this.LayoutMode == eLayoutMode.Stacked)
					valueRatio = (1f/_SliceCount)*_SliceIteration + (valueRatio*(1f/_SliceCount)  );

				
				Vector3 Point2D_Value = new Vector3(ratio, valueRatio, Camera.main.nearClipPlane*2  );

				ViewPortBuffer.Add(Point2D_Value);

				if(i>0)
				{
					Vector3 Point3D_ValuePrevious = Camera.main.ViewportToWorldPoint( ViewPortBuffer[i-1] );
					Vector3 Point3D_Value = Camera.main.ViewportToWorldPoint(Point2D_Value);

					Debug.DrawLine(Point3D_ValuePrevious, Point3D_Value, TheColor, 0f);	//duration à 0f(seconds) => 1 frame
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

	}
}







