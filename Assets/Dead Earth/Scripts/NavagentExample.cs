using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class NavagentExample : MonoBehaviour
{
    public AIWaypointNetwork WayponitNetwork = null;
    public int CurrentIndex = 0;
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();

    private NavMeshAgent _navAgent = null;

    // Use this for initialization
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();

        if (WayponitNetwork==null) return;
        SetNextDestination(false);
    }

    private void SetNextDestination(bool increment)
    {
        if (!WayponitNetwork) return;
        int incStep = increment ? 1 : 0;
        int nextWaypoint = (CurrentIndex + incStep >= WayponitNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        Transform nextWaypointTransform = WayponitNetwork.Waypoints[nextWaypoint];

        if (nextWaypointTransform != null)
        {
            CurrentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }

        CurrentIndex = nextWaypoint;
    }

    // Update is called once per frame
    void Update()
    {
        HasPath = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        PathStatus = _navAgent.pathStatus;

        if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
        }

        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);
        else if (_navAgent.isPathStale)
            SetNextDestination(false);
    }

    private IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0.0f;
        while (time<=duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}
