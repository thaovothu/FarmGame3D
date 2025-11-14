using System.Collections.Generic;
using UnityEngine;
using FarmGame.Domain.Entities;

namespace FarmGame.UI
{
    public class Farm3DView : MonoBehaviour
    {
        public GameObject plotPrefab;
        
        [Header("Stage 1: Young/Seedling Indicators (0% - 50%)")]
        public GameObject tomatoYoungPrefab;
        public GameObject blueberryYoungPrefab;
        public GameObject strawberryYoungPrefab;
        public GameObject cowYoungPrefab;
        
        [Header("Stage 2: Growing Indicators (50% - 100%)")]
        public GameObject tomatoGrowingPrefab;
        public GameObject blueberryGrowingPrefab;
        public GameObject strawberryGrowingPrefab;
        public GameObject cowGrowingPrefab;
        
        [Header("Stage 3: Ready to Harvest (100% + SpoilageTime)")]
        public GameObject tomatoPrefab; // Hiển thị sau 100%, trong thời gian SpoilageTimeMinutes
        public GameObject blueberryPrefab;
        public GameObject strawberryPrefab;
        public GameObject cowPrefab;

        public GameObject growingIndicatorPrefab; // fallback

        public Transform plotsParent;
        public float spacing = 3f;

        private GameController _controller;
        private readonly List<GameObject> _instances = new List<GameObject>();

        void Start()
        {
            // cố gắng lấy GameController; nếu chưa có thì đợi (không crash)
            _controller = FindObjectOfType<GameController>();
            if (_controller == null)
            {
                Debug.LogWarning("Farm3DView: GameController not found in scene. RenderPlots will be skipped until a controller is available.");
                return;
            }

            RenderPlots();
        }

        public void RenderPlots()
        {
            try
            {
                // đảm bảo controller/farm có sẵn
                if (_controller == null)
                    _controller = FindObjectOfType<GameController>();

                if (_controller == null)
                {
                    Debug.LogWarning("Farm3DView.RenderPlots: GameController is null. Call RenderPlots after GameController initialized.");
                    return;
                }

                if (_controller.Farm == null)
                {
                    Debug.LogWarning("Farm3DView.RenderPlots: Farm is null on GameController.");
                    return;
                }

                if (plotPrefab == null)
                {
                    Debug.LogWarning("Farm3DView.RenderPlots: plotPrefab is not assigned in inspector.");
                    return;
                }

                Clear();

                Debug.Log($"Farm3DView: Rendering {_controller.Farm.Plots.Count} plots");

                for (int i = 0; i < _controller.Farm.Plots.Count; i++)
                {
                    var go = Instantiate(plotPrefab, plotsParent ? plotsParent : transform);
                    go.transform.localPosition = new Vector3(i * spacing, 0f, 0f);
                    go.name = $"Plot_{i}";

                    // ensure a Collider exists so PlotView (which requires a Collider) can be added
                    if (go.GetComponent<Collider>() == null)
                    {
                        Debug.Log($"Plot_{i}: Adding BoxCollider");
                        var box = go.AddComponent<BoxCollider>();
                        var mr = go.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            var bounds = mr.bounds;
                            box.center = go.transform.InverseTransformPoint(bounds.center);
                            box.size = bounds.size;
                            Debug.Log($"Plot_{i}: BoxCollider size set to {bounds.size}");
                        }
                        else
                        {
                            box.size = new Vector3(1f, 0.2f, 1f);
                            Debug.Log($"Plot_{i}: BoxCollider size set to default (1, 0.2, 1)");
                        }
                    }
                    else
                    {
                        Debug.Log($"Plot_{i}: Already has Collider");
                    }

                    var pv = go.GetComponent<PlotView>();
                    if (pv == null)
                    {
                        pv = go.AddComponent<PlotView>();
                        Debug.Log($"Plot_{i}: PlotView component added");
                    }
                    pv.Initialize(i, this, _controller);
                    _instances.Add(go);
                }
                
                Debug.Log($"Farm3DView: Finished rendering {_instances.Count} plot instances");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Farm3DView.RenderPlots error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public GameObject GetCropPrefab(CropType type)
        {
            return type switch
            {
                CropType.Tomato => tomatoPrefab,
                CropType.Blueberry => blueberryPrefab,
                CropType.Strawberry => strawberryPrefab,
                _ => null
            };
        }

        public GameObject CowPrefab => cowPrefab;
        public GameObject GrowingIndicatorPrefab => growingIndicatorPrefab;

        /// <summary>
        /// Get indicator prefab for young stage (0-50%)
        /// </summary>
        public GameObject GetYoungIndicatorPrefab(CropType type)
        {
            GameObject prefab = type switch
            {
                CropType.Tomato => tomatoYoungPrefab,
                CropType.Blueberry => blueberryYoungPrefab,
                CropType.Strawberry => strawberryYoungPrefab,
                _ => null
            };
            return prefab != null ? prefab : growingIndicatorPrefab;
        }

        /// <summary>
        /// Get indicator prefab for growing stage (50-100%)
        /// </summary>
        public GameObject GetGrowingIndicatorPrefab(CropType type)
        {
            GameObject prefab = type switch
            {
                CropType.Tomato => tomatoGrowingPrefab,
                CropType.Blueberry => blueberryGrowingPrefab,
                CropType.Strawberry => strawberryGrowingPrefab,
                _ => null
            };
            return prefab != null ? prefab : growingIndicatorPrefab;
        }

        /// <summary>
        /// Get indicator prefab for young animal stage (0-50%)
        /// </summary>
        public GameObject GetYoungIndicatorPrefabForAnimal(AnimalType aType)
        {
            GameObject prefab = aType switch
            {
                AnimalType.DairyCow => cowYoungPrefab,
                _ => null
            };
            return prefab != null ? prefab : growingIndicatorPrefab;
        }

        /// <summary>
        /// Get indicator prefab for growing animal stage (50-100%)
        /// </summary>
        public GameObject GetGrowingIndicatorPrefabForAnimal(AnimalType aType)
        {
            GameObject prefab = aType switch
            {
                AnimalType.DairyCow => cowGrowingPrefab,
                _ => null
            };
            return prefab != null ? prefab : growingIndicatorPrefab;
        }

        private void Clear()
        {
            foreach (var o in _instances) Destroy(o);
            _instances.Clear();
        }
    }
}