using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Plant : MonoBehaviour
{
    [SerializeField]
    private Transform sun;
    [SerializeField]
    private LineRenderer stem;
    [SerializeField, Range(0, 10)]
    private float timeScale = 1.0f;
    private float DeltaTime => Time.deltaTime * timeScale;

    [SerializeField]
    private float growthRate = 0.1f;
    [SerializeField]
    private float stemPeiceMaxLength = 0.5f;
    [SerializeField]
    private float stemDirectionRandomness = 0.1f;

    [SerializeField]
    private float maxLength = 10.0f;
    private float _length = 0;


    [Header("Branches")]
    [SerializeField]
    private LineRenderer branchStemPrefab;
    [SerializeField]
    private float spawnBranchAfterLength = 5.0f;
    [SerializeField]
    private float branchSpawnRate = 0.1f;
    private float _branchSpawnRecord = 0;
    [SerializeField]
    private bool spawnDoubleBranches = false;
    private bool _branchSideIsLeft = true;
    [SerializeField]
    private RangeStruct branchSpawnAngles;
    [SerializeField]
    private float branchMaxLength = 2.0f;


    private List<Branch> _branches = new List<Branch>();



    private bool _canGrow = true;

    private float _currentPeiceLength = 0.0f;
    private Vector2 _currentLightDirection => (sun.position - stem.transform.TransformPoint(stem.GetPosition(stem.positionCount - 1))).normalized;
    private Vector2 _currentGrowthDirection = Vector2.up;

    void Awake()
    {
        stem.positionCount = 2;
        stem.SetPosition(0, Vector3.zero);
        stem.SetPosition(1, Vector3.zero);
    }

    void Update()
    {
        if (!_canGrow)
            return;

        GrowMainStem(out float growAmount);

        for (int i = 0; i < _branches.Count; i++)
        {
            _branches[i] = GrowBranch(_branches[i], growAmount);
        }

        UpdateSpawnNewBranch(growAmount);
    }

    void GrowMainStem(out float growAmount)
    {
        growAmount = growthRate * DeltaTime;
        _currentPeiceLength += growAmount;
        _length += growAmount;

        if (_length > maxLength)
        {
            _canGrow = false;
            return;
        }

        if (_currentPeiceLength > stemPeiceMaxLength)
        {
            _currentPeiceLength = 0.0f;
            stem.positionCount++;
            stem.SetPosition(stem.positionCount - 1, stem.GetPosition(stem.positionCount - 2));

            _currentGrowthDirection = Quaternion.Euler(0, 0, Random.Range(-stemDirectionRandomness, stemDirectionRandomness)) * _currentLightDirection;
        }

        stem.SetPosition(stem.positionCount - 1, stem.GetPosition(stem.positionCount - 2) + (Vector3)_currentGrowthDirection * _currentPeiceLength);
    }

    void UpdateSpawnNewBranch(float growAmount)
    {
        if (_length <= spawnBranchAfterLength)
            return;

        _branchSpawnRecord += growAmount;

        if (_branchSpawnRecord < branchSpawnRate)
            return;
        _branchSpawnRecord = 0;

        SpawnBranch();
        if (spawnDoubleBranches) SpawnBranch();
    }

    private void SpawnBranch()
    {

        LineRenderer branch = Instantiate(branchStemPrefab, transform);
        branch.gameObject.SetActive(true);

        Transform t = branch.transform;
        t.position = stem.transform.TransformPoint(stem.GetPosition(stem.positionCount - 1));

        float currentDirectionEuler = Mathf.Atan2(_currentGrowthDirection.y, _currentGrowthDirection.x) * Mathf.Rad2Deg - 90;
        if (_branchSideIsLeft) currentDirectionEuler += branchSpawnAngles.PickRandomNumber();
        else currentDirectionEuler -= branchSpawnAngles.PickRandomNumber();
        t.rotation = Quaternion.Euler(0, 0, currentDirectionEuler);

        branch.positionCount = 2;
        branch.SetPosition(0, Vector3.zero);
        branch.SetPosition(1, Vector3.zero);

        _branchSideIsLeft = !_branchSideIsLeft;

        _branches.Add(new Branch
        {
            Stem = branch,
            Direction = t.rotation * Vector3.up,
            Length = 0
        });
    }


    Branch GrowBranch(Branch branch, float growAmount)
    {
        if (branch.Length > branchMaxLength)
            return branch;
        branch.Length += growAmount;
        branch.CurrentPeiceLength += growAmount;


        if (branch.CurrentPeiceLength > stemPeiceMaxLength)
        {
            branch.CurrentPeiceLength = 0.0f;
            branch.Stem.positionCount++;
            // branch.Stem.SetPosition(branch.Stem.positionCount - 1, branch.Stem.GetPosition(branch.Stem.positionCount - 2));

            branch.Direction = Quaternion.Euler(0, 0, Random.Range(-stemDirectionRandomness, stemDirectionRandomness)) * branch.Direction;
        }

        branch.Stem.SetPosition(branch.Stem.positionCount - 1, branch.Stem.GetPosition(branch.Stem.positionCount - 2) + (Vector3)branch.Direction * branch.CurrentPeiceLength);
        return branch;
    }

    public struct Branch
    {
        public LineRenderer Stem;
        public Vector2 Direction;
        public float Length;
        public float CurrentPeiceLength;
    }
}


[System.Serializable]
public struct RangeStruct
{
    public float Min;
    public float Max;

    public float Lerp(float t)
    {
        return Mathf.Lerp(Min, Max, t);
    }

    public float InverseLerp(float number)
    {
        return Mathf.InverseLerp(Min, Max, number);
    }

    public float Clamp(float number)
    {
        return Mathf.Clamp(number, Min, Max);
    }

    public float PickRandomNumber()
    {
        return Random.Range(Min, Max);
    }

    // A add operator between RangeStruct and float
    public static RangeStruct operator +(RangeStruct range, float number)
    {
        return new RangeStruct { Min = range.Min + number, Max = range.Max + number };
    }

    public override string ToString()
    {
        return $"RangeStruct({Min}~{Max})";
    }
}