// UnitStats.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "RTS/Unit Stats")]
public class UnitStats : ScriptableObject
{
    // Thêm thuộc tính UnitType vào đây để xác định loại đơn vị mà stats này áp dụng
    public Unit.UnitType unitType; // <-- Thêm dòng này

    [Header("Movement Stats")]
    [Tooltip("Tốc độ di chuyển của đơn vị.")]
    public float moveSpeed = 5f;
    [Tooltip("Khoảng cách dừng tối thiểu mà đơn vị này sẽ giữ từ mục tiêu.")]
    public float minDistance = 0.5f;

    [Header("Combat Stats")]
    [Tooltip("Tốc độ bắn của đơn vị (số lần bắn mỗi giây).")]
    public float fireRate = 1f;
    [Tooltip("Sát thương mỗi phát bắn.")]
    public float damage = 10f;
    [Tooltip("Tầm bắn tối đa của đơn vị.")]
    public float attackRange = 10f;
}