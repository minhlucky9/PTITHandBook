
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR

public class NodeEditor : EditorWindow
{
	private Vector2 scrollPosition; // Stores the scroll offset
	private float zoomScale = 1.0f; // Current zoom level
	private Rect zoomArea; // The area where zooming happens
	private readonly float zoomMin = 0.5f; // Minimum zoom level
	private readonly float zoomMax = 2.0f; // Maximum zoom level
	private Vector2 zoomOrigin = Vector2.zero; // Center point for zoom

	public bool isConnect = false;
	public int selectedNode = -1;
	public List<Rect> nodes = new List<Rect>();
	public string[] names;

	public Rect selectedRect = new Rect(10, 10, 200, 100);

	public static void ShowEditor()
	{
		NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
		editor.Init();
		Debug.Log("base");
	}

	public virtual void Init()
	{
		zoomScale = 1.0f;
		nodes.Clear();
	}

	private void HandleZoom(Event e)
	{
		if (e.type == EventType.ScrollWheel && zoomArea.Contains(e.mousePosition))
		{
			float zoomDelta = -e.delta.y * 0.01f; // Adjust zoom speed
			float oldZoom = zoomScale;

			// Calculate new zoom level
			zoomScale = Mathf.Clamp(zoomScale + zoomDelta, zoomMin, zoomMax);

			// Adjust scroll position to keep zoom centered
			Vector2 mouseOffset = e.mousePosition - zoomOrigin;
			Vector2 zoomOffset = mouseOffset - (mouseOffset * (zoomScale / oldZoom));
			scrollPosition -= zoomOffset / zoomScale;

			e.Use(); // Mark the event as handled
		}
	}

	public virtual void OnGUI()
	{
		zoomArea = new Rect(0, 50, position.width, position.height - 50);

		// Handle Zoom
		HandleZoom(Event.current);

		// Apply zoom
		GUI.BeginGroup(zoomArea);
		Matrix4x4 oldMatrix = GUI.matrix;
		GUIUtility.ScaleAroundPivot(new Vector2(zoomScale, zoomScale), zoomOrigin);

		BeginWindows();
		
		DrawNodes();
		EndWindows();

		DrawClickMenu();

		if (isConnect)
		{
            Handles.color = Color.black;
			Vector2 mousePos = Event.current.mousePosition;

			Vector3 startPos = new Vector3(selectedRect.x + selectedRect.width / 2, selectedRect.y + selectedRect.height / 2, 0);
			if(mousePos.x < selectedRect.x)
            {
				startPos.x = selectedRect.x;
			}
			if (mousePos.x > selectedRect.x + selectedRect.width)
			{
				startPos.x = selectedRect.x + selectedRect.width;
			}
			if (mousePos.y < selectedRect.y)
			{
				startPos.y = selectedRect.y;
			}
			if (mousePos.y > selectedRect.y + selectedRect.height)
			{
				startPos.y = selectedRect.y + selectedRect.height;
			}

			Handles.DrawAAPolyLine(5.0f, startPos, mousePos);
            Repaint();
        }

		// Restore matrix
		GUI.matrix = oldMatrix;
		GUI.EndGroup();
	}

	public virtual void DrawNodes() {
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i] = GUILayout.Window(i, nodes[i], DrawNodeWindow, names[i]);
		}

	}

	void DrawNodeWindow(int id)
	{
		Event currentEvent = Event.current;
		if (currentEvent.type == EventType.MouseDown) // Left-click
		{
			Debug.Log($"Window {id} clicked!");
			isConnect = !isConnect;
			OnMouseDownNode(id);
		}

		DrawNodeContent(id);
		GUI.DragWindow();
		OnDragNode(id);
	}

	public virtual void DrawNodeContent(int i)
    {
		EditorGUILayout.LabelField("Name", "name");
	}

	void DrawClickMenu()
	{
		// Check for mouse click events
		Event currentEvent = Event.current;

		if (currentEvent.type == EventType.ContextClick) // Detect right-click
		{
			// Get the mouse position
			Vector2 mousePosition = currentEvent.mousePosition;

			// Create and display the GenericMenu
			GenericMenu menu = new GenericMenu();
			CreateMenu(ref menu, mousePosition);
			// Display the menu at the mouse position
			menu.DropDown(new Rect(mousePosition, Vector2.zero));

			// Mark the event as used so it doesn't propagate further
			currentEvent.Use();
		}

		if (currentEvent.type == EventType.MouseDown)
		{
			isConnect = false;
			OnMouseDownMenu();
		}
	}

	public virtual void CreateMenu(ref GenericMenu menu, Vector2 mousePosition)
    {
		menu.AddItem(new GUIContent("Dialog"), false, () => Debug.Log("Option 1 Selected"));
		menu.AddItem(new GUIContent("Anwser"), false, () => Debug.Log("Option 2 Selected"));
	}

	public void DrawNodeCurve(Rect start, Rect end)
	{
		Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
		Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
		Vector3 startTan = startPos + Vector3.right * 50;
		Vector3 endTan = endPos + Vector3.left * 50;
		Color shadowCol = new Color(0, 0, 0, 0.06f);
		for (int i = 0; i < 3; i++) // Draw a shadow
			Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
		Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
	}

	#region Event Listener
	public virtual void OnDragNode(int i)
	{

	}

	public virtual void OnMouseDownMenu()
    {
		selectedNode = -1;
	}

	public virtual void OnMouseDownNode(int id)
	{
		selectedNode = id;
		selectedRect = nodes[id];
	}
	#endregion
}


#endif