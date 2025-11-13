using UnityEngine;

/// <summary>
/// Script test siêu đơn giản - chỉ log khi Start và khi click chuột
/// Gắn vào Canvas hoặc bất kỳ GameObject nào
/// </summary>
public class SimpleTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("======================");
        Debug.Log("=== SIMPLE TEST STARTED ===");
        Debug.Log("GameObject: " + gameObject.name);
        Debug.Log("======================");
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[SimpleTest] Mouse clicked at: {Input.mousePosition}");
        }
    }
}
