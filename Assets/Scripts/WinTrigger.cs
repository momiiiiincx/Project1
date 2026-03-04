using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [Header("Settings")]
    public GameObject winPanel; // ลาก WinPanel จาก Hierarchy มาใส่ที่นี่
    public string playerTag = "Player"; // ตรวจสอบว่าตัว Player มี Tag ชื่อ Player หรือไม่

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ตรวจสอบว่าสิ่งที่เดินเข้ามาชนคือ Player หรือไม่
        if (collision.CompareTag(playerTag))
        {
            ShowWinPanel();
        }
    }

    void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true); // เปิดการแสดงผล Panel

            // ถ้าต้องการให้เกมหยุดเดินเมื่อชนะ ให้ปลดคอมเมนต์บรรทัดล่างนี้ครับ
            // Time.timeScale = 0f; 
        }
    }
}