using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathDisplayMode { None, Connections, Paths };
public enum PathDrawMode { FromTo, All };

public class WaypointNetworkScript : MonoBehaviour
{
    #region Publics Members
   
    #endregion

    #region Serialize Field
    [SerializeField]
    List<Transform> waypoints = new List<Transform>();
    #endregion

    #region Private & Protected Members
    PathDisplayMode _displayMode = PathDisplayMode.Connections;
    PathDrawMode _drawMode = PathDrawMode.FromTo;
    int _UIStart, _UIEnd;
    #endregion

    #region Publics Methods
    public Transform GetTransformAtIndex(int index)
    {
        return waypoints[index];
    }
    public int GetWaypointsSize()
    {
        return waypoints.Count;
    }

    public string GetNameByIndex(int index)
    {
        return waypoints[index].name;
    }
    #endregion

    #region Getter & Setter
    public List<Transform> GetWaypoints()
    {
        return waypoints;
    }
    public void SetWaypoints(List<Transform> transforms)
    {
        waypoints = transforms;
    }
    public PathDisplayMode GetDisplayMode()
    {
        return _displayMode;
    }
    public void SetDisplayMode(PathDisplayMode displayMode)
    {
        _displayMode = displayMode;
    }
    public PathDrawMode GetDrawMode()
    {
        return _drawMode;
    }
    public void SetDrawMode(PathDrawMode drawMode)
    {
        _drawMode = drawMode;
    }
    public int GetUIStart()
    {
        return _UIStart;
    }
    public void SetUIStart(int value)
    {
        _UIStart = value;
    }
    public int GetUIEnd()
    {
        return _UIEnd;
    }
    public void SetUIEnd(int value)
    {
        _UIEnd = value;
    }
    #endregion

}
