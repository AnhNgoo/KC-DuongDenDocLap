using UnityEngine;

namespace Pathfinding
{
    [ExecuteInEditMode]
    /// <summary>
    /// Helper class to keep track of references to GameObjects.
    /// Does nothing more than to hold a GUID value.
    /// </summary>
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_unity_reference_helper.php")]
    public class UnityReferenceHelper : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private string guid;

        public string GetGUID()
        {
            return guid;
        }

        public void Awake()
        {
            Reset();
        }

        public void Reset()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Pathfinding.Util.Guid.NewGuid().ToString();
                Debug.Log("Created new GUID - " + guid, this);
                return;
            }

            // Nếu object này không phải prefab (prefab.scene.name == null)
            if (!string.IsNullOrEmpty(gameObject.scene.name))
            {
                // Dùng API mới: FindObjectsByType (Unity 2023+)
                var allHelpers = UnityEngine.Object.FindObjectsByType<UnityReferenceHelper>(FindObjectsSortMode.None);

                foreach (var urh in allHelpers)
                {
                    if (urh != this && guid == urh.guid)
                    {
                        guid = Pathfinding.Util.Guid.NewGuid().ToString();
                        Debug.Log("GUID duplicated, created new GUID - " + guid, this);
                        break;
                    }
                }
            }
        }

    }
}
