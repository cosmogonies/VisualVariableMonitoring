using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

//========================
//	Author: Cyrille PAULHIAC
//	Email: contact@cosmogonies.net
//	WebSite: www.cosmogonies.net
//========================

namespace VisualVariableMonitoring
	{


	//TODO: if the time frame becomnes too wide, the data is too tight. Idea is to only display averave of bunch of 10 values, then 100, then 1000

	public class DBG_DataCollector
		{   //Custom class to hold collected data and perform analysis.

		internal System.Reflection.FieldInfo Field;     //The source field
		internal MonoBehaviour Behaviour;               //The source scripted Component
		internal Color VariableColor = Color.white;     //Default Color.

		public List<float> Data;                        //The Data !

		public float MaximumValue = float.MinValue;
		public float MinimumValue = float.MaxValue;

		public float Average = 0.0f;

		public GUIStyle guiStyle;

		public DBG_DataCollector( System.Reflection.FieldInfo _Field, MonoBehaviour _Behaviour, Color _Color )
			{   //Constructor
			this.Field = _Field;
			this.Behaviour = _Behaviour;
			this.VariableColor = _Color;
			Data = new List<float>();

			GUIStyle TextStyle = new GUIStyle();
			TextStyle.normal.textColor = this.VariableColor;
			guiStyle = TextStyle;

			}

		public void addValue( float _NewValue )
			{   //Update with the new Value at current frame.
			this.Data.Add( _NewValue );

			//Update Min/Max
			if ( _NewValue > this.MaximumValue )
				this.MaximumValue = _NewValue;
			if ( _NewValue < this.MinimumValue )
				this.MinimumValue = _NewValue;

			//Update Average computation
			int size = this.Data.Count;
			/*
			float sum =0.0f;
			int i = 0;
			for ( i = 0 ; i < size ; i++ )
				{	//TODO find the for;ula tu update average WITHOUT compute all datas again...
				sum += this.Data[i];
				}
			this.Average = sum / this.Data.Count;
			*/
			//Well, iterating through all the numbers is too much expensive so we use that:
			//http://stackoverflow.com/questions/22999487/update-the-average-of-a-continuous-sequence-of-numbers-in-constant-time
			this.Average = (size * this.Average + _NewValue) / (size + 1);
			}



		public float getCurrentValue()
			{   //returns the current Value (last one registered).
			if ( this.Data.Count == 0 )//First frame or we just done a clear
				return 0.0f;
			return this.Data[this.Data.Count - 1];
			}

		public void clearData()
			{   //wipe all datas, for a total new context.
			this.Data.Clear();
			this.MaximumValue = float.MinValue;
			this.MinimumValue = float.MaxValue;
			this.Average = 0.0f;
			}

		}

	}
