using UnityEngine;
using UnityEngine.EventSystems;

namespace FarmGame.UI
{
    [RequireComponent(typeof(Collider))]
    public class BackgroundView : MonoBehaviour
    {
        private GameController _controller;
        
        void Start()
        {
            _controller = FindObjectOfType<GameController>();
        }
        
        void OnMouseDown()
        {
            // Chặn click khi đang hover UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            // Lấy vị trí click từ Raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 clickPosition = Vector3.zero;
            
            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                Debug.Log($"Background clicked at position: {clickPosition}");
            }
            
            var uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowSeedSelection(-1, clickPosition);
        }
    }
}