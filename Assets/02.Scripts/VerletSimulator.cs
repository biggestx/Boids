using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator : MonoBehaviour, IDisposable
{

    [SerializeField]
    private List<VerletNode> Nodes = null;

    [SerializeField]
    private Transform PinPoint = null;

    [SerializeField]
    private Vector3 StartPosition = Vector3.zero;

    [SerializeField]
    private float Distance = 0.5f;

    [SerializeField]
    private Vector3 Gravity = new Vector3(0, -0.5f, 0);

    [SerializeField]
    private int IterationCount = 80;

    [SerializeField]
    private float Movement = 0.5f;

    [SerializeField]
    private float DistanceLimitation = 1f;

    [SerializeField]
    private Transform ConstraintTarget = null;

    void Start()
    {
        StartPosition = PinPoint.position;
        foreach (var node in Nodes)
        {
            node.transform.SetParent(null);
            node.transform.position = StartPosition;
            node.PrevPos = StartPosition;

            StartPosition.y -= Distance;
        }

        PrevObjPos = this.transform.position;
    }


    // TODO : 누수 없게 체크 필요
    public void Dispose()
    {
        foreach (var node in Nodes)
        {
            Destroy(node.gameObject);
        }
        Nodes.Clear();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    Vector3 PrevObjPos = Vector3.zero;
    private void ValidateDistance()
    {
        var curPos = transform.position;

        var dist = transform.position - PrevObjPos;
        if (dist.magnitude > DistanceLimitation)
        {
            foreach (var node in Nodes)
            {
                node.transform.position += dist;
                node.PrevPos += dist;
            }
        }

        PrevObjPos = this.transform.position;
    }


    private void Simulate()
    {
        for(int i=0;i < Nodes.Count; ++i)
        {
            var node = Nodes[i];

            Vector3 velocity = node.transform.position - node.PrevPos;
            node.PrevPos = node.transform.position;

            Vector3 newPos = node.transform.position + velocity;
            newPos += Gravity * Time.deltaTime;

            Vector3 dir = node.transform.position - newPos;

            if (ConstraintTarget != null)
            {
                if (newPos.y < ConstraintTarget.position.y)
                    newPos.y = ConstraintTarget.position.y;
            }

            node.transform.position = newPos;
        }
    }

    private void ApplyConstraint()
    {

        Nodes[0].transform.position = PinPoint.position;

        var nodeCount = Nodes.Count;
        for (int i = 0; i < nodeCount - 1; ++i)
        {
            var node1 = Nodes[i];
            var node2 = Nodes[i + 1];

            float curDistance = (node1.transform.position - node2.transform.position).magnitude;
            float diff = Mathf.Abs(curDistance - Distance);

            var dir = Vector3.zero;

            if (curDistance > Distance)
                dir = (node1.transform.position - node2.transform.position).normalized;
            else if (curDistance < Distance)
                dir = (node2.transform.position - node1.transform.position).normalized;


            Vector3 movement = dir * diff;

            node1.transform.position -= (movement * Movement);
            node2.transform.position += (movement * Movement);
        }

    }


    void Update()
    {
        ValidateDistance();
        Simulate();

        for (int i = 0; i < IterationCount; ++i)
        {
            ApplyConstraint();

            if (i % 2 == 1)
            {
                for (int j = 0; j < Nodes.Count; ++j)
                {
                    var node = Nodes[j];

                    var newPos = node.transform.position;

                    if (ConstraintTarget != null)
                    {
                        if (newPos.y < ConstraintTarget.position.y)
                            newPos.y = ConstraintTarget.position.y;
                    }

                    node.transform.position = newPos;
                }
            }
        }

    }
}
