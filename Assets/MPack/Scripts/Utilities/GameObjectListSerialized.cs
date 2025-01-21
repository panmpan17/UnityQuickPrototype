using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/GameObject List Serialized")]
    public class GameObjectListSerialized : ScriptableObject
    {
    #if UNITY_EDITOR
        [TextArea]
        public string Note;
    #endif

        public List<GameObject> List = new List<GameObject>();

        public ValueWithEnable<int> CountLimit;

        public bool ReachedLimit => CountLimit.Enable && AliveCount >= CountLimit.Value;

        public int AliveCount {
            get {
                int count = 0;
                for (int i = List.Count - 1; i >= 0; i--)
                {
                    if (List[i] == null) List.RemoveAt(i);
                    else count++;
                }
                return count;
            }
        }

        public void Add(GameObject gameObject)
        {
            for (int i = List.Count - 1; i >= 0; i--)
            {
                if (List[i] == null) List.RemoveAt(i);
            }
            List.Add(gameObject);
        }

        public void DestroyAll()
        {
            while (List.Count > 0)
            {
                if (List[0])
                    Destroy(List[0]);
                List.RemoveAt(0);
            }
        }

        public GameObject RandomItem()
        {
            if (List.Count == 0)
                return null;

            return List[Random.Range(0, List.Count)];
        }
    }
}
