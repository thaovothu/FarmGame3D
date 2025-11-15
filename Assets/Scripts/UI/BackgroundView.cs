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
                
            Debug.Log("Background clicked - Show buy plot option");
            
            var uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowSeedSelection(-1);
        }
    }
}