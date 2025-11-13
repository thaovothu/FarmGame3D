using System.Collections.Generic;
using UnityEngine;
using FarmGame.Domain.Entities;

namespace FarmGame.UI
{
    public class Farm3DView : MonoBehaviour
    {
        public GameObject plotPrefab;
        public GameObject tomatoPrefab;
        public GameObject blueberryPrefab;
        public GameObject strawberryPrefab;
        public GameObject cowPrefab;

        [Header("Harvest Indicators")]
        public GameObject harvestReadyIndicatorPrefab; // prefab hiển thị số quả sẵn sàng thu hoạch (UI/3D)
        
        [Header("Growing Indicators Per Type")]
        public GameObject tomatoGrowingPrefab;
        public GameObject blueberryGrowingPrefab;
        public GameObject strawberryGrowingPrefab;
        public GameObject cowGrowingPrefab;

        public GameObject growingIndicatorPrefab;

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

        public GameObject HarvestReadyIndicatorPrefab => harvestReadyIndicatorPrefab;
        public GameObject GrowingIndicatorPrefab => growingIndicatorPrefab;

        public GameObject GetGrowingIndicatorPrefab(CropType type)
        {
            return type switch
            {
                CropType.Tomato => tomatoGrowingPrefab ?? growingIndicatorPrefab,
                CropType.Blueberry => blueberryGrowingPrefab ?? growingIndicatorPrefab,
                CropType.Strawberry => strawberryGrowingPrefab ?? growingIndicatorPrefab,
                _ => growingIndicatorPrefab
            };
        }

        // overload for animals (you can map by AnimalType)
        public GameObject GetGrowingIndicatorPrefabForAnimal(AnimalType aType)
        {
            // extend if more animals added
            return aType switch
            {
                AnimalType.DairyCow => cowGrowingPrefab ?? growingIndicatorPrefab,
                _ => growingIndicatorPrefab
            };
        }

        private void Clear()
        {
            foreach (var o in _instances) Destroy(o);
            _instances.Clear();
        }
    }
}