using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(WaypointNetworkScript))]
public class WaypointNetworkEditorScript : Editor
{
    #region System
    public override void OnInspectorGUI()
    {
        WaypointNetworkScript network = (WaypointNetworkScript)target;

        network.SetDisplayMode((PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode", network.GetDisplayMode())); 

       if (network.GetDisplayMode() == PathDisplayMode.Paths)
        {
            network.SetDrawMode((PathDrawMode)EditorGUILayout.EnumPopup("Draw Mode", network.GetDrawMode()));
            network.SetUIStart(EditorGUILayout.IntSlider("Waypoint Start", network.GetUIStart(), 0, network.GetWaypointsSize()-1)); 
            network.SetUIEnd(EditorGUILayout.IntSlider("Waypoint End", network.GetUIEnd(), 0, network.GetWaypointsSize()-1));
        }

       DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        WaypointNetworkScript network = (WaypointNetworkScript)target;

        for (int i = 0; i < network.GetWaypointsSize(); i++)
        {
            if (network.GetTransformAtIndex(i) != null)
                Handles.Label(network.GetTransformAtIndex(i).position, (i+1).ToString() + " : " + network.GetTransformAtIndex(i).name);
        }

       if (network.GetDisplayMode() == PathDisplayMode.Connections)
        {
            Vector3[] linePoints = new Vector3[network.GetWaypointsSize() + 1];

            for (int i = 0; i <= network.GetWaypointsSize(); i++)
            {
                int index = i != network.GetWaypointsSize() ? i : 0;
                if (network.GetTransformAtIndex(index) != null)
                    linePoints[i] = network.GetTransformAtIndex(index).position;
                else
                    linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
                  
            Handles.color = Color.cyan;           
            Handles.DrawPolyLine(linePoints);
        }
        else if(network.GetDisplayMode() == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();

            if (network.GetDrawMode() == PathDrawMode.FromTo)
            {              
                Vector3 from = network.GetTransformAtIndex(network.GetUIStart()).position;
                Vector3 to = network.GetTransformAtIndex(network.GetUIEnd()).position;

                NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
                Handles.color = Color.yellow;                
                Handles.DrawPolyLine(path.corners);
            }
            else if(network.GetDrawMode() == PathDrawMode.All)
            {
                int startIndex = network.GetUIStart();
                int lastIndex = network.GetUIEnd();

                Vector3[] from;
                Vector3[] to;

                if (startIndex <= lastIndex)
                {
                    from = new Vector3[lastIndex - startIndex];
                    to = new Vector3[lastIndex - startIndex];
                }
                else
                {
                    from = new Vector3[startIndex - lastIndex];
                    to = new Vector3[startIndex - lastIndex];
                }
                
                
                int j = 0;

                if(startIndex <= lastIndex)
                {
                    for (int i = startIndex; i < lastIndex; i++)
                    {
                        from[j] = network.GetTransformAtIndex(i).position;
                        to[j] = network.GetTransformAtIndex(i + 1).position;
                        j++;
                    }

                    for (int i = 0; i < from.Length; i++)
                    {

                        NavMesh.CalculatePath(from[i], to[i], NavMesh.AllAreas, path);
                        Handles.color = Color.yellow;
                        Handles.DrawPolyLine(path.corners);
                    }
                }
                else
                {
                    for (int i = lastIndex; i < startIndex; i++)
                    {
                        from[j] = network.GetTransformAtIndex(i).position;
                        to[j] = network.GetTransformAtIndex(i + 1).position;
                        j++;
                    }

                    for (int i = 0; i < from.Length; i++)
                    {

                        NavMesh.CalculatePath(from[i], to[i], NavMesh.AllAreas, path);
                        Handles.color = Color.yellow;
                        Handles.DrawPolyLine(path.corners);
                    }
                }

            }
        }
    }
    #endregion
}
