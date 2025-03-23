using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class SnapController : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve attractForceCurve;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip snapSound;
    [SerializeField]
    private CinemachineImpulseSource snapImpulse;
    [SerializeField]
    private float snapImpulseForce;
    [SerializeField]
    private ParticleSystem snapParticle;
    private ParticleSystem.MainModule snapParticleMain;

    private bool m_isAttracting = false;

    private List<SnapPart> m_originalSnapParts = new List<SnapPart>();
    private List<SnapPart> m_snapParts = new List<SnapPart>();
    private List<SnapPointInfo> m_snapPoints = new List<SnapPointInfo>();


    void Awake()
    {
        SnapPart[] parts = GetComponentsInChildren<SnapPart>();
        for (int i = 0; i < parts.Length; i++)
        {
            AddSnapPartAndSnapPointInfos(parts[i]);
        }

        snapParticleMain = snapParticle.main;
    }

#region  Adding and removing Snap Parts, snap point setting
    void AddSnapPartAndSnapPointInfos(SnapPart snapPart)
    {
        snapPart.SnapController = this;
        m_originalSnapParts.Add(snapPart);

        for (int i = 0; i < snapPart.SnapSettings.Length; i++)
        {
            SnapPointInfo info = new SnapPointInfo();
            info.Setting = snapPart.SnapSettings[i];

            info.IsOriginal = true;
            info.ParentPart = snapPart;
            info.SnappedPart = null;
            info.SnappedPartIndex = -1;
            info.State = SnapPointState.Free;
            m_snapPoints.Add(info);
        }
    }
#endregion


#region Snap point manipulation
    bool ScanAvalibleSnapPartToAttract()
    {
        bool canAttract = false;
        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_snapPoints[i].State == SnapPointState.Snapped)
            {
                continue;
            }

            m_snapPoints[i].State = SnapPointState.Free;
            m_snapPoints[i].SnappedPart = null;

            Vector3 origin = transform.TransformPoint(m_snapPoints[i].Position);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, transform.TransformDirection(m_snapPoints[i].LookDirection), m_snapPoints[i].AttractDistance);
            for (int j = 0; j < hits.Length; j++)
            {
                SnapPart snapPart = hits[j].collider.GetComponent<SnapPart>();
                if (snapPart == null || snapPart.SnapController == this)
                {
                    continue;
                }

                bool isSnapped = false;
                for (int k = 0; k < snapPart.SnapSettings.Length; k++)
                {
                    if (!snapPart.SnapSettings[k].IsSlot || snapPart.SnapSettings[k].Indetifier != m_snapPoints[i].Setting.Indetifier)
                    {
                        continue;
                    }

                    m_snapPoints[i].State = SnapPointState.Attracting;
                    m_snapPoints[i].SnappedPart = snapPart;
                    m_snapPoints[i].SnappedPartIndex = k;
                    isSnapped = true;
                    canAttract = true;
                    break;
                }

                if (isSnapped)
                {
                    break;
                }
            }
        }
        return canAttract;
    }


    void SnapInfoSnapWithPart(int snapPointIndex, SnapPart snapPart)
    {
        m_snapPoints[snapPointIndex].State = SnapPointState.Snapped;

        snapPart.SnapToController(this, m_snapPoints[snapPointIndex]);
        if (m_snapParts.Count >= 1)
        {
            snapPart.transform.SetParent(m_snapParts[0].transform);
        }
        m_snapParts.Add(snapPart);
        
        // Add snap point infos
        int otherSnapPointIndex = m_snapPoints[snapPointIndex].SnappedPartIndex;
        for (int i = 0; i < snapPart.SnapSettings.Length; i++)
        {
            SnapPointInfo info = new SnapPointInfo();
            info.Setting = snapPart.SnapSettings[i];
            info.Setting.Position = transform.InverseTransformPoint(snapPart.transform.TransformPoint(info.Position));
            info.Setting.LookDirection = transform.InverseTransformDirection(snapPart.transform.TransformDirection(info.LookDirection));

            info.ParentPart = snapPart;
            info.SnappedPart = null;
            info.SnappedPartIndex = -1;
            info.State = SnapPointState.Free;

            if (i == otherSnapPointIndex)
            {
                info.SnappedPart = m_snapPoints[snapPointIndex].ParentPart;
                info.SnappedPartIndex = snapPointIndex;
                info.State = SnapPointState.Snapped;
            }

            m_snapPoints.Add(info);
        }

        PlaySnapEffects(snapPointIndex, snapPart);
    }

    void PlaySnapEffects(int i, SnapPart snapPart)
    {
        audioSource.PlayOneShot(snapSound);

        Vector3 snapedPoint = transform.TransformPoint(m_snapPoints[i].Position);
        Vector3 delta = (snapedPoint - snapPart.transform.position).normalized;
        snapImpulse.GenerateImpulse(new Vector3(delta.x * snapImpulseForce + Random.Range(-snapImpulseForce, snapImpulseForce),
                                                delta.y * snapImpulseForce + Random.Range(-snapImpulseForce, snapImpulseForce),
                                                0));

        // var shap = snapPart.GetComponent<GeometryBaseShape>();
        snapParticleMain.startColor = Color.white;
        snapParticle.transform.position = snapedPoint;
        snapParticle.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        snapParticle.Play();
    }

    public void ReleaseOtherParts()
    {
        SnapPart mainSnapPart = null;
        for (int i = 0; i < m_snapPoints.Count; i++)
        {
            SnapPointInfo snapPoint = m_snapPoints[i];

            if (snapPoint.IsOriginal)
            {
                if (snapPoint.State == SnapPointState.Snapped)
                {
                    snapPoint.State = SnapPointState.Free;
                    snapPoint.SnappedPart.ReleaseFromController();
                    mainSnapPart = snapPoint.SnappedPart;
                    snapPoint.SnappedPart = null;
                    snapPoint.ParentPart = null;
                }
            }
            else
            {
                m_snapPoints.RemoveAt(i);
                i--;
            }
        }

        if (mainSnapPart)
        {
            mainSnapPart.DetectChildrenSanpParts();
        }

        m_snapParts.Clear();
    }
#endregion


#region Update
    void FixedUpdate()
    {
        if (m_isAttracting)
        {
            FixedUpdateAttracting();
        }
    }

    void FixedUpdateAttracting()
    {
        Debug.Assert(m_isAttracting);
        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            SnapPointInfo snapPoint = m_snapPoints[i];
            if (snapPoint.State != SnapPointState.Attracting)
            {
                continue;
            }

            if (snapPoint.SnappedPart == null)
            {
                snapPoint.State = SnapPointState.Free;
                continue;
            }

            Vector2 point = transform.TransformPoint(snapPoint.Position);
            RaycastHit2D[] hits = Physics2D.LinecastAll(point, point + (Vector2)transform.TransformDirection(snapPoint.LookDirection * snapPoint.AttractDistance));

            bool snapPartIsInSight = false;
            // Vector2 otherObjectHitPoint = Vector3.zero;
            // Vector2 snapPartHitPoint = Vector3.zero;
            for (int j = 0; j < hits.Length; j++)
            {
                Transform root = hits[j].collider.transform.root;
                if (root == snapPoint.SnappedPart.transform.root)
                {
                    snapPartIsInSight = true;
                    // snapPartHitPoint = hits[j].point;
                    break;
                }
                // else if (root != transform)
                // {
                //     otherObjectHitPoint = hits[j].point;
                // }
            }

            // if (otherObjectHitPoint != Vector2.zero)
            // {
            //     Vector2 delta1 = otherObjectHitPoint - point;
            //     Vector2 delta2 = snapPartHitPoint - point;

            //     if (delta1.sqrMagnitude < delta2.sqrMagnitude)
            //     {
            //         snapPartIsInSight = false;
            //     }
            // }

            if (snapPartIsInSight)
            {
                // Michael TODO: improve this function call
                snapPoint.SnappedPart.PullTowards(transform.TransformPoint(snapPoint.Position), snapPoint.SnappedPartIndex,
                                                snapPoint.AttractStrength, snapPoint.AttractDistance, attractForceCurve);
            }
            else
            {
                snapPoint.SnappedPart.StopAttracting();
            }
        }
    }
#endregion

#region Inputs
    public void StartAttractingOtherParts()
    {
        m_isAttracting = true;
        ScanAvalibleSnapPartToAttract();
    }

    public void StopAttracting()
    {
        for (int i = 0; i < m_snapPoints.Count; i++)
        {
            if (m_snapPoints[i].State == SnapPointState.Attracting)
            {
                m_snapPoints[i].State = SnapPointState.Free;
                m_snapPoints[i].SnappedPart.StopAttracting();
                m_snapPoints[i].SnappedPart = null;
            }
        }
        m_isAttracting = false;
    }
#endregion

    public void OnPartCollisionEnter(SnapPart snapPart, Collision2D collision2D)
    {
        OnCollisionEnter2D(collision2D);
    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        SnapPart snapPart = collision2D.collider.GetComponent<SnapPart>();
        if (snapPart == null)
        {
            return;
        }

        int count = m_snapPoints.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_snapPoints[i].State != SnapPointState.Attracting)
            {
                continue;
            }

            if (m_snapPoints[i].SnappedPart == snapPart)
            {
                SnapInfoSnapWithPart(i, snapPart);
                break;
            }
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < m_snapPoints.Count; i++)
        {
            Vector3 point = transform.TransformPoint(m_snapPoints[i].Position);
            Gizmos.DrawSphere(point, 0.1f);
            Gizmos.DrawLine(point, point + transform.TransformDirection(m_snapPoints[i].LookDirection));
        }
    }
}

public enum SnapPointState
{
    Free,
    Attracting,
    Snapped,
}

#if UNITY_EDITOR
[System.Serializable]
#endif
public class SnapPointInfo
{
    public SnapePointSetting Setting;

    public bool IsOriginal = false;
    public Vector3 Position => Setting.Position;
    public Vector3 LookDirection => Setting.LookDirection;
    public float AttractDistance => Setting.AttractDistance;
    public float AttractStrength => Setting.AttractStrength;

    public SnapPart ParentPart;
    public SnapPart SnappedPart;
    public int SnappedPartIndex;
    public SnapPointState State;
}