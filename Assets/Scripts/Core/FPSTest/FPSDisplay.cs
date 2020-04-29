using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	struct node
	{
		public string s;
		public float time;
		public node(string a,float b)
		{
			s = a;
			time = b;
		}
	}
	static List<node> message;
	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}
	public static  void PutMessage(string s,bool continuous)
	{
		if (message == null)
			message = new List<node>();
		node temp = new node();
		temp.s = s;
		if(continuous)
			temp.time = 1;
		else
			temp.time = 0.1f;
		message.Add(temp);
	}
	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;

		style.normal.textColor = Color.white;
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
		if (message == null)
			message = new List<node>();
		for(int i=0;i<message.Count ; i++)
		{
			rect.y += rect.height;
			GUI.Label(rect, message[i].s, style);
			message[i] =new node(message[i].s,message[i].time-Time.deltaTime);
			if (message[i].time < 0)
				message.RemoveAt(i);
		}
		//message.Clear();
	}
}