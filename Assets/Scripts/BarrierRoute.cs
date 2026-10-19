using UnityEngine;
using System.Collections.Generic;

public class BarrierRoute : MonoBehaviour
{
    public List<int> blockedRouteIndex = new List<int>();
    public Collider barrierCollider;
    public Renderer barrierRender;
}
