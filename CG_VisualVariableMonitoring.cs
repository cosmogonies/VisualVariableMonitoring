using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;



//[System.AttributeUsage(System.AttributeTargets.All | System.AttributeTargets.Struct)]
//[System.AttributeUsage(System.AttributeTargets.All)]

public class DBG_Track : System.Attribute
{
	//private string name;
	public Color VariableColor;
	//public double version;
	
	//public DBG_Track(Color _Color)
	public DBG_Track()
	{
		//this.VariableColor = Color.white;

		//No Color determined, randomised:
		this.VariableColor = new Color( UnityEngine.Random.value, UnityEngine.Random.value,UnityEngine.Random.value);
		//TODO: having a predetermined color set (10?) for great-looking monitorying

	}

	/*
	public DBG_Track(Color _Color)
	{
		this.VariableColor = _Color;
	}
	*/


	public DBG_Track(string _ColorName)
	{

		//System.Reflection.FieldInfo[] FieldArray = Color.GetType().GetFields();

		//foreach( System.Reflection.FieldInfo current in FieldArray )

		//FieldInfo[] fi = typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Instance);
		//FieldInfo[] fi = typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.SetProperty);
		/*
		FieldInfo[] fi = typeof(Color).GetFields();
		foreach (FieldInfo info in fi)
		{
			Debug.Log(info.Name);
		}
		*/

		PropertyInfo[] pi = typeof(Color).GetProperties();
		foreach (PropertyInfo info in pi)
		{
			Debug.Log(info.Name);
		}

		//Debug.Log (Color.yellow.ToString());

		this.VariableColor = Color.yellow;
	}


	public DBG_Track(float _Red, float _Green, float _Blue)
	{
		//Unity uses ratio for color channels :
		//maybe user still use 255 range
		if (_Red > 1.0f)
			_Red /= 255;
		if (_Green > 1.0f)
			_Green /= 255;
		if (_Blue > 1.0f)
			_Blue /= 255;

		this.VariableColor = new Color (_Red, _Green,_Blue);
	}


}





public enum eLayoutMode {kStacked=1, kOverlapped=2};
public enum eDrawingStyle {kColumns=1, kCurve=2};




public class DBG_DataCollector
{
	internal System.Reflection.FieldInfo Field;
	internal MonoBehaviour Behaviour;

	//internal string VariableName ="Default";
	internal Color VariableColor = Color.white;

	//Queue<float> DataValue;
	public List<float> Data;



	public float MaximumValue= 0.0f;
	public float MinimumValue= 0.0f;

	public float Average= 0.0f;
	//float AverageExceptNull= 0.0f;

	/*
	public DBG_DataCollector(string _Name, Color _Color)
	{
		this.VariableName = _Name;
		this.VariableColor = _Color;
		Data = new List<float>();

	}
	*/
	public DBG_DataCollector(System.Reflection.FieldInfo _Field, MonoBehaviour _Behaviour, Color _Color)
	{
		this.Field = _Field;
		this.Behaviour = _Behaviour;

		//this.VariableName = _Name;
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

	//public List<float> Data;
	public Dictionary<string,float> Data2;

	//[DBG_Track()]
	float MARGIN_WIDTH = 0.1f; // in screen ratio

	//eDrawingStyle DrawingStyle;
	eLayoutMode LayoutMode;

	//[DBG_Track()]
	public bool AbsoluteMode =true;

	public Dictionary<string, DBG_DataCollector> WatchDict;

	float GUI_Opacity = 1.0f;
	
	public CG_VisualVariableMonitoring()
	{
		this.WatchDict = new Dictionary<string, DBG_DataCollector>();

		//this.DrawingStyle = eDrawingStyle.kCurve;

		this.LayoutMode = eLayoutMode.kStacked;

		//this.WatchDict["AdjustmentAngle"] = new DBG_DataCollector("AdjustmentAngle", Color.yellow);
		//this.WatchDict["Velocity"] = new DBG_DataCollector("Velocity", Color.cyan);
		//this.WatchDict["PointOccluded"] = new DBG_DataCollector("PointOccluded", Color.green);
	}

	void Start()
	{
		Debug.Log ("** INTROSPECTING **");

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
							Debug.Log ("\tFound trackable variable @ class :" +currentBehaviour.name+" typeof "+currentBehaviour.GetType().Name +" FieldName = "+ currentField.Name);

							this.WatchDict[currentField.Name] = new DBG_DataCollector(currentField, currentBehaviour,  ((DBG_Track) CustomAttributeArray[k]).VariableColor);

							/*
							if(  ((DBG_Track) CustomAttributeArray[k]).VariableColor == Color.red )
							{
								this.WatchDict[currentField.Name] = new DBG_DataCollector(currentField, currentBehaviour,  (DBG_Track) CustomAttributeArray[k]).VariableColor);
							}
							else
								this.WatchDict[currentField.Name] = new DBG_DataCollector(currentField, currentBehaviour,  Color.green);
*/

							//float currentValue = (float) currentField.GetValue(currentBehaviour); //TODO: be sure the cast is possible
							//Debug.Log (currentValue);
						}
					}
				}
				
			}




		}

		//Debug.Log ( Assembly.GetAssembly().GetTypes().Length );

		//Instrospecting current class !



		//Debug.Log ("Introspecting current class :" +this.name+" of type "+this.GetType().Name);


		/*
		System.Reflection.MemberInfo info = this.GetType();
		object[] AttributeArray = info.GetCustomAttributes(true);
		Debug.Log ( AttributeArray.Length+" attributes found" );
		for (int i = 0; i < AttributeArray.Length; i ++)
		{
			Debug.Log(AttributeArray[i]);
		}
		*/



		/*
		System.Reflection.MemberInfo[] MemberArray = this.GetType().GetMembers();
		for (int k = 0; k < MemberArray.Length; k ++)
		{
			System.Reflection.MemberInfo currentMember = MemberArray[k];
			Debug.Log ("Scanning Member ="+currentMember.Name);
			
			object[] AttributeArray3 = currentMember.GetCustomAttributes(true);
			for (int i = 0; i < AttributeArray3.Length; i ++)
			{
				Debug.Log("==> fouuund "+AttributeArray3[i]);
			}
		}
*/



		/*
		System.Reflection.FieldInfo[] FieldArray = this.GetType().GetFields();
		for (int i = 0; i < FieldArray.Length; i ++) 
		{
			System.Reflection.FieldInfo currentField = FieldArray[i];

			if( currentField.GetCustomAttributes(true).Length>0 )
			{

				Debug.Log("Found attributes on attribute "+currentField.Name);

				if( currentField.GetCustomAttributes(true)[0].GetType() == typeof(DBG_Track) )
				{
					Debug.Log ("Found trackable variable = "+ currentField.Name);

					float currentValue = (float) currentField.GetValue(this); //TODO: be sure the cast is possible

					Debug.Log (currentValue);
				}

			}

		}
*/


		//Type type = this.GetType();
		//Debug.Log( "this.GetType().Name =" + type.Name );
		
		
		//System.Reflection.FieldInfo info = type.GetField("TestVariable");	
		//Debug.Log( info );
		
		//Debug.Log( info.GetValue(this) );





		/*
		System.Reflection.PropertyInfo[] PropertyArray = this.GetType().GetProperties();
		for (int j = 0; j < PropertyArray.Length; j ++)
		{
			System.Reflection.PropertyInfo currentProperty = PropertyArray[j];
			Debug.Log ("Scanning Property ="+currentProperty.Name);

			object[] AttributeArray2 = currentProperty.GetCustomAttributes(true);
			for (int i = 0; i < AttributeArray2.Length; i ++)
			{
				Debug.Log("==> found "+AttributeArray2[i]);
			}
		}
		*/
	}

	// DID NOT work... because Mono does not have LINQ *sigh*
	/*
	static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly) 
	{
		foreach(Type type in assembly.GetTypes()) 
		{
			if (type.GetCustomAttributes(typeof(HelpAttribute), true).Length > 0) 
			{
				yield return type;
			}
		}
	}
*/

	void OnGUI()
	{
		float PanelWidth = MARGIN_WIDTH * Screen.width;
		float FontHeight =18.0f;

		float SlicedPanelHeight =  (1f/this.WatchDict.Keys.Count) * Screen.height;

		float XPos = Screen.width - (PanelWidth);


		int j=1;
		foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict)
		{
			int i=1;
			float YPos = Screen.height - SlicedPanelHeight*j;

			DBG_DataCollector current = kvp.Value;

			GUIStyle TextStyle = new GUIStyle();
			TextStyle.normal.textColor = current.VariableColor;

			GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MARGIN_WIDTH * Screen.width),FontHeight), "["+current.Field.Name+"]",TextStyle);
			i++;
			GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MARGIN_WIDTH * Screen.width),FontHeight), "[Min="+current.MinimumValue.ToString()+"]",TextStyle);
			i++;
			GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MARGIN_WIDTH * Screen.width),FontHeight), "[Max="+current.MaximumValue.ToString()+"]",TextStyle);
			i++;
			GUI.Label(new Rect(XPos, YPos+FontHeight*i, (MARGIN_WIDTH * Screen.width),FontHeight), "[Avrg="+current.Average.ToString()+"]",TextStyle);
			i++;
			i++;

			j++;
		}

		//Footer

		this.GUI_Opacity = GUI.HorizontalSlider (new Rect (XPos, Screen.height - FontHeight * 2, PanelWidth, FontHeight), this.GUI_Opacity, 0.0f, 1.0f);
		if( GUI.Button( new Rect(XPos, Screen.height - FontHeight, PanelWidth*0.5f,FontHeight), "Layout" ) )
		{
			if(this.LayoutMode == eLayoutMode.kStacked)
				this.LayoutMode = eLayoutMode.kOverlapped;
			else
				this.LayoutMode = eLayoutMode.kStacked;
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
		//AdjustementAngle
		/*
		CameraControl comp = Camera.main.camera.GetComponent<CameraControl>() as CameraControl;
		float value = Mathf.Abs( comp.gonCameraMotion.PreviousAngle );		
		if(AbsoluteMode) 
			if(value<0)
				value = Mathf.Abs(value);
		this.WatchDict["AdjustmentAngle"].addValue(value);


		//	velocity
		value = comp.velocity ;		
		this.WatchDict["Velocity"].addValue(value);
		*/


		//	PointOccluded
		//value = (float)comp.PointOccluded ;		
		//this.WatchDict["PointOccluded"].addValue(value);


		/*
		//access to CLS_CameraMotion.previousAngle :
		//From http://msdn.microsoft.com/en-us/library/z919e8tw%28v=vs.110%29.aspx
		System.Type t = typeof(DBG_Track);
		// Using reflection.
		System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);  // Reflection. 

		// Displaying output. 
		foreach (System.Attribute attr in attrs)
		{

			Debug.Log(attr.GetType().ToString());
			if (attr is DBG_Track)
			{
				Debug.LogError("FOUUUND");

				//Author a = (Author)attr;
				//System.Console.WriteLine("   {0}, version {1:f}", a.GetName(), a.version);
				DBG_Track a = (DBG_Track) attr;
				Debug.Log ("found =");
				Debug.Log (a.ToString());
			}
		}
		*/


		foreach (KeyValuePair<string, DBG_DataCollector> kvp in this.WatchDict) 
		{
			//float currentValue = (float)kvp.Value.Field.GetValue (kvp.Value.Behaviour); //TODO: be sure the cast is possible

			float currentValue = (float)kvp.Value.Field.GetValue (kvp.Value.Behaviour); //TODO: be sure the cast is possible
			currentValue = Mathf.Abs (currentValue);

			kvp.Value.addValue(currentValue);
			//Debug.Log (currentValue);

			//Debug.Log (kvp.Value.Behaviour.);
		}

		//if(this.DrawingStyle == eDrawingStyle.kColumns)
		//	DrawColumns();

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

			ratio =  Mathf.Lerp(MARGIN_WIDTH,1.0f-MARGIN_WIDTH, (float) i / (this.DataValue.Count -1) ) ;

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

			//ratio =  Mathf.Lerp(MARGIN_WIDTH,1.0f-MARGIN_WIDTH, (float) i / (_DataCollector.Data.Count -1) ) ;
			ratio =  Mathf.Lerp(0.0f,1.0f-MARGIN_WIDTH, (float) i / (_DataCollector.Data.Count -1) ) ;
			
			value = _DataCollector.Data[i];

			float valueRatio = 0.0f;
			if(_DataCollector.MaximumValue!=0.0f)
				valueRatio = value/_DataCollector.MaximumValue;

			if( this.LayoutMode == eLayoutMode.kStacked)
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
	
}
