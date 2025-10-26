using UnityEngine;
using System.Collections;

public class BlockRepositioner : MonoBehaviour
{
    public enum FixedAxis { X, Y, Z }

    [System.Serializable]
    public class BlockGroup
    {
        public string name;
        public Transform blockParent; 
        public BoxCollider spawnZone; 
        public FixedAxis fixedAxis;
    }

    public BlockGroup[] blockGroups;
    public float resetInterval = 5.0f;

    void Start()
    {
        StartCoroutine(ResetPositionRoutine());
    }

    IEnumerator ResetPositionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(resetInterval);
            RandomizeAllGroups();
        }
    }

    void RandomizeAllGroups()
    {
        foreach (BlockGroup group in blockGroups)
        {
            RandomizeGroup(group);
        }
    }

    void RandomizeGroup(BlockGroup group)
    {
        if (group.spawnZone == null || group.blockParent == null)
        {
            Debug.LogWarning("组 " + group.name + " 缺少 Spawn Zone 或 Block Parent!");
            return;
        }

        // --- 这是修复的核心 ---

        // 1. 获取 collider 的本地 center 和 size
        BoxCollider zoneCollider = group.spawnZone;
        Vector3 localCenter = zoneCollider.center;
        Vector3 localSize = zoneCollider.size;

        // 2. 获取平面的世界坐标位置（用于“压平”坐标）
        Vector3 zonePlanePosition = group.spawnZone.transform.position;

        // 3. 遍历所有方块
        foreach (Transform block in group.blockParent)
        {
            // 4. 在 collider 的 *本地空间* 中生成一个随机点
            float localX = Random.Range(localCenter.x - localSize.x / 2, localCenter.x + localSize.x / 2);
            float localY = Random.Range(localCenter.y - localSize.y / 2, localCenter.y + localSize.y / 2);
            float localZ = Random.Range(localCenter.z - localSize.z / 2, localCenter.z + localSize.z / 2);

            Vector3 localRandomPos = new Vector3(localX, localY, localZ);

            // 5. 将这个 *本地* 随机点转换为 *世界* 坐标
            //    这会正确地应用 zone 对象的 Position, Rotation, 和 Scale
            Vector3 worldRandomPos = zoneCollider.transform.TransformPoint(localRandomPos);

            // 6. 在 *世界空间* 中，将随机点“压平”到正确的平面上
            switch (group.fixedAxis)
            {
                case FixedAxis.X:
                    worldRandomPos.x = zonePlanePosition.x;
                    break;
                case FixedAxis.Y:
                    worldRandomPos.y = zonePlanePosition.y;
                    break;
                case FixedAxis.Z:
                    worldRandomPos.z = zonePlanePosition.z;
                    break;
            }

            // 7. 将方块移动到这个最终的、正确的坐标
            block.position = worldRandomPos;
        }
    }
}