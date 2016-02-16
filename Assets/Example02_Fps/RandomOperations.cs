using UnityEngine;
using System.Collections;

using VisualVariableMonitoring;

public class RandomOperations : MonoBehaviour
	{

	public enum operation { Abs, Cosinus, SquareRoot };


	operation currentOperation;

	[DBG_Track("red")]
	public float fps;

	void Start()
		{
		currentOperation = operation.Abs;
		}

	void Update()
		{
		this.fps = Mathf.Round(Time.deltaTime *1000); //ms

		if ( Random.value < 0.005f )
			{
			switch ( Random.Range( 0, 3 ) )
				{
				case 0:
					currentOperation = operation.Abs;
					break;
				case 1:
					currentOperation = operation.Cosinus;
					break;
				case 2:
					currentOperation = operation.SquareRoot;
					break;
				default:
					break;
				}
			Debug.Log("Switching to compute = "+currentOperation.ToString());
			Camera.main.gameObject.GetComponent<CG_VisualVariableMonitoring>().RegisterEvent( currentOperation.ToString() );
			}


		for ( int i = 0 ; i < 10000 ; i++ )
			{
			switch ( currentOperation )
				{
				case operation.Abs:
					Mathf.Abs( Random.value );
					break;

				case operation.Cosinus:
					Mathf.Cos( Random.value );
					break;

				case operation.SquareRoot:
					Mathf.Sqrt( Random.value );
					break;

				default:
					break;
				}
			}




		}
	}
