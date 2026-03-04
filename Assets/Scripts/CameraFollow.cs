using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // ลาก Player มาใส่ในช่องนี้
    public float smoothSpeed = 0.125f; // ความเร็วในการลัดเลาะ (ยิ่งน้อยยิ่งนุ่ม)
    public Vector3 offset; // ระยะห่างระหว่างกล้องกับ Player (เช่น z = -10)

    void LateUpdate() // ใช้ LateUpdate เพื่อให้กล้องขยับหลังจาก Player ขยับเสร็จแล้ว
    {
        if (target == null) return;

        // ตำแหน่งที่กล้องควรจะไป
        Vector3 desiredPosition = target.position + offset;

        // ทำให้การเคลื่อนที่นุ่มนวลด้วย Lerp
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}