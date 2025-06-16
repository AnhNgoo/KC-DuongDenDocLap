using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Thêm dòng này để sử dụng .Any() và .All() nếu cần

public class Shoot : MonoBehaviour
{
    [System.Serializable]
    public class UnitBulletPrefab
    {
        public Unit.UnitType unitType;
        public GameObject bulletPrefab;
        [Tooltip("Các điểm xuất phát của viên đạn cho loại đơn vị này. Trực thăng có thể có nhiều điểm.")]
        public List<Transform> shootPoints; // THAY ĐỔI MỚI: Danh sách các điểm bắn
        [Tooltip("Tốc độ bắn của đơn vị loại này (số viên đạn mỗi giây).")]
        public float fireRate = 1.0f;
        [Tooltip("Sát thương mỗi viên đạn gây ra.")]
        public float bulletDamage = 10f;
        [Tooltip("Tốc độ bay của viên đạn cho loại đơn vị này.")]
        public float bulletSpeed = 20f;
    }

    [Header("Bullet Prefabs & Shoot Points")]
    public List<UnitBulletPrefab> unitBulletPrefabs;

    private Unit parentUnit;
    private Dictionary<Unit.UnitType, UnitBulletPrefab> bulletSettingsDict = new Dictionary<Unit.UnitType, UnitBulletPrefab>();

    private void Awake()
    {
        parentUnit = GetComponentInParent<Unit>();
        if (parentUnit == null)
        {
            Debug.LogError("Script Shoot yêu cầu phải là con của GameObject có script Unit.", this);
            enabled = false;
            return;
        }

        foreach (var entry in unitBulletPrefabs)
        {
            // THAY ĐỔI MỚI: Kiểm tra danh sách shootPoints
            if (entry.bulletPrefab != null && entry.shootPoints != null && entry.shootPoints.Count > 0 && entry.shootPoints.All(sp => sp != null))
            {
                bulletSettingsDict[entry.unitType] = entry;
            }
            else
            {
                Debug.LogWarning($"Shoot: Bullet Prefab hoặc Shoot Points cho {entry.unitType} chưa được gán hoặc bị thiếu. Đơn vị loại này sẽ không bắn được hoặc bắn sai vị trí.", this);
            }
        }
    }

    /// <summary>
    /// Tạo và bắn một hoặc nhiều viên đạn theo hướng mục tiêu được chỉ định từ các điểm bắn.
    /// Phương thức này sẽ được gọi từ script Unit.
    /// </summary>
    /// <param name="targetPosition">Vị trí mà Unit đang nhắm tới.</param>
    /// <returns>Thời gian cooldown cần thiết sau khi bắn.</returns>
    public float FireBullet(Vector3 targetPosition)
    {
        Unit.UnitType currentUnitType = parentUnit.unitType;

        if (!bulletSettingsDict.TryGetValue(currentUnitType, out UnitBulletPrefab currentBulletSettings))
        {
            Debug.LogWarning($"Shoot: Không tìm thấy cài đặt Bullet Prefab hoặc Shoot Points cho UnitType '{currentUnitType}'.", this);
            return 0f;
        }

        GameObject bulletPrefab = currentBulletSettings.bulletPrefab;
        List<Transform> shootPointTransforms = currentBulletSettings.shootPoints; // THAY ĐỔI MỚI: Lấy danh sách

        float bulletDamage = currentBulletSettings.bulletDamage;
        float currentBulletSpeed = currentBulletSettings.bulletSpeed;

        if (bulletPrefab == null)
        {
            Debug.LogWarning($"Shoot: Bullet Prefab cho UnitType '{currentUnitType}' là null trong cài đặt.", this);
            return 0f;
        }
        if (shootPointTransforms == null || shootPointTransforms.Count == 0 || shootPointTransforms.Any(sp => sp == null))
        {
            Debug.LogWarning($"Shoot: Shoot Points cho UnitType '{currentUnitType}' chưa được gán hoặc bị thiếu. Đơn vị sẽ không bắn được.", this);
            return 0f;
        }

        // THAY ĐỔI MỚI: Lặp qua TẤT CẢ các điểm bắn
        foreach (Transform shootPointTransform in shootPointTransforms)
        {
            Vector3 spawnPosition = shootPointTransform.position;
            Vector3 shootDirection = (targetPosition - spawnPosition).normalized;
            shootDirection.z = 0; // Đảm bảo đạn bay trên mặt phẳng 2D
            if (shootDirection == Vector3.zero) shootDirection = shootPointTransform.up; // Hướng mặc định nếu target trùng với spawn

            GameObject bulletGO = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            Bullet bullet = bulletGO.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(shootDirection, currentBulletSpeed, bulletDamage);
            }
            else
            {
                Debug.LogError($"Shoot: Prefab '{bulletPrefab.name}' không có script Bullet!", bulletPrefab);
                Destroy(bulletGO);
            }

   
        }

        return currentBulletSettings.fireRate;
    }
}