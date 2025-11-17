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
        private readonly Dictionary<int, Vector3> _plotPositions = new Dictionary<int, Vector3>();
        private readonly HashSet<int> _placedPlots = new HashSet<int>(); // Track plots that have been placed
        private int _initialPlotCount = 0; // Number of plots at game start

        void Start()
        {
            // cố gắng lấy GameController; nếu chưa có thì đợi (không crash)
            _controller = FindObjectOfType<GameController>();
            if (_controller == null)
            {
                Debug.LogWarning("Farm3DView: GameController not found in scene. RenderPlots will be skipped until a controller is available.");
                return;
            }

            // Lưu số lượng plot ban đầu từ config (chỉ lấy 1 lần duy nhất)
            if (_initialPlotCount == 0 && _controller.Config != null)
            {
                _initialPlotCount = _controller.Config.InitialPlots;
                Debug.Log($"Farm3DView: Initial plot count set to {_initialPlotCount}");
                
                // Đánh dấu tất cả plot ban đầu là đã được đặt (chỉ làm 1 lần)
                for (int i = 0; i < _initialPlotCount; i++)
                {
                    _placedPlots.Add(i);
                }
                Debug.Log($"Farm3DView: Marked {_initialPlotCount} initial plots as placed");
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

                Debug.Log($"Farm3DView: Rendering {_controller.Farm.Plots.Count} plots (Initial: {_initialPlotCount})");

                for (int i = 0; i < _controller.Farm.Plots.Count; i++)
                {
                    var go = Instantiate(plotPrefab, plotsParent ? plotsParent : transform);
                    
                    // Sử dụng vị trí đã lưu nếu có, không thì dùng vị trí mặc định
                    if (_plotPositions.ContainsKey(i))
                    {
                        go.transform.position = _plotPositions[i];
                    }
                    else
                    {
                        // Plots ban đầu hoặc plot chưa được đặt vị trí custom hiện ở vị trí mặc định
                        go.transform.localPosition = new Vector3(i * spacing, 0f, 0f);
                    }
                    
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

        /// <summary>
        /// Set position for a specific plot
        /// </summary>
        public void SetPlotPosition(int plotIndex, Vector3 worldPosition)
        {
            _plotPositions[plotIndex] = worldPosition;
            _placedPlots.Add(plotIndex);
        }

        /// <summary>
        /// Get position for a specific plot (if set)
        /// </summary>
        public Vector3? GetPlotPosition(int plotIndex)
        {
            if (_plotPositions.ContainsKey(plotIndex))
                return _plotPositions[plotIndex];
            return null;
        }

        /// <summary>
        /// Check if a plot has been placed
        /// </summary>
        public bool IsPlotPlaced(int plotIndex)
        {
            return _placedPlots.Contains(plotIndex);
        }

        /// <summary>
        /// Get number of unplaced plots (chỉ tính plot mới mua)
        /// </summary>
        public int GetUnplacedPlotCount()
        {
            int totalPlots = _controller?.Farm?.Plots.Count ?? 0;
            // Chỉ đếm plot mới mua (index >= _initialPlotCount) mà chưa được đặt
            int newlyPurchasedPlots = Mathf.Max(0, totalPlots - _initialPlotCount);
            int newlyPurchasedAndPlaced = 0;
            for (int i = _initialPlotCount; i < totalPlots; i++)
            {
                if (_placedPlots.Contains(i))
                    newlyPurchasedAndPlaced++;
            }
            return newlyPurchasedPlots - newlyPurchasedAndPlaced;
        }

        /// <summary>
        /// Get initial plot count
        /// </summary>
        public int GetInitialPlotCount()
        {
            return _initialPlotCount;
        }

        /// <summary>
        /// Lưu vị trí của các plot để save game
        /// </summary>
        public Infrastructure.PlotPositionData[] GetPlotPositionsForSave()
        {
            var positions = new List<Infrastructure.PlotPositionData>();
            foreach (var kvp in _plotPositions)
            {
                positions.Add(new Infrastructure.PlotPositionData
                {
                    plotIndex = kvp.Key,
                    x = kvp.Value.x,
                    y = kvp.Value.y,
                    z = kvp.Value.z
                });
            }
            return positions.ToArray();
        }

        /// <summary>
        /// Load vị trí của các plot từ save game
        /// </summary>
        public void LoadPlotPositions(Infrastructure.PlotPositionData[] positions)
        {
            if (positions == null) return;
            
            _plotPositions.Clear();
            _placedPlots.Clear();
            
            // Load plot positions từ save file
            foreach (var pos in positions)
            {
                var vector = new Vector3(pos.x, pos.y, pos.z);
                _plotPositions[pos.plotIndex] = vector;
                _placedPlots.Add(pos.plotIndex);
            }
            
            // Đánh dấu các plot ban đầu là đã được đặt
            for (int i = 0; i < _initialPlotCount; i++)
            {
                _placedPlots.Add(i);
            }
            
            Debug.Log($"Loaded {positions.Length} plot positions from save file");
        }

        private void Clear()
        {
            foreach (var o in _instances) Destroy(o);
            _instances.Clear();
        }
    }
}