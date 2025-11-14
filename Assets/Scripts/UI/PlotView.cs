using UnityEngine;
using FarmGame.Domain.Entities;

namespace FarmGame.UI
{
    [RequireComponent(typeof(Collider))]
    public class PlotView : MonoBehaviour
    {
        private int _plotIndex;
        private Farm3DView _farmView;
        private GameController _controller;
        private GameObject _contentInstance;
        private GameObject _indicatorInstance; // visual duy nhất hiển thị theo stage
        private float _lastIndicatorUpdateTime;


        public void Initialize(int plotIndex, Farm3DView farmView, GameController controller)
        {
            _plotIndex = plotIndex;
            _farmView = farmView;
            _controller = controller;
            _lastIndicatorUpdateTime = 0f;
            UpdateView();
        }

        void Update()
        {
            // Update indicator every 1 second to show growth stage changes
            if (Time.time - _lastIndicatorUpdateTime >= 1f)
            {
                _lastIndicatorUpdateTime = Time.time;
                RefreshIndicator();
            }
        }

        private void RefreshIndicator()
        {
            if (_controller == null || _controller.Farm == null) return;
            if (_plotIndex < 0 || _plotIndex >= _controller.Farm.Plots.Count) return;

            var plot = _controller.Farm.Plots[_plotIndex];

            if (plot.Plant != null)
            {
                UpdateHarvestIndicator(plot.Plant, null);
            }
            else if (plot.Animal != null)
            {
                UpdateHarvestIndicator(null, plot.Animal);
            }
        }

        public void UpdateView()
        {
            // destroy previous content instance
            if (_contentInstance != null) Destroy(_contentInstance);

            if (_controller == null || _controller.Farm == null) return;
            if (_plotIndex < 0 || _plotIndex >= _controller.Farm.Plots.Count) return;

            var plot = _controller.Farm.Plots[_plotIndex];

            if (plot.Plant != null)
            {
                // Chỉ update indicator, không spawn visual nữa
                UpdateHarvestIndicator(plot.Plant, null);
            }
            else if (plot.Animal != null)
            {
                // Chỉ update indicator, không spawn visual nữa
                UpdateHarvestIndicator(null, plot.Animal);
            }
            else
            {
                // nothing on plot
                if (_indicatorInstance != null)
                {
                    Destroy(_indicatorInstance);
                    _indicatorInstance = null;
                }
            }
        }

        void OnMouseDown()
        {
            // Chặn click vào plot khi đang hover UI (tránh click xuyên qua UI panel)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log($"PlotView: Click blocked - pointer over UI");
                return;
            }

            Debug.Log($"PlotView: OnMouseDown called for plot {_plotIndex}");
            
            if (_controller == null)
            {
                Debug.LogError("PlotView: _controller is null!");
                return;
            }
            
            // Gọi GameController xử lý logic click
            _controller.OnPlotClicked(_plotIndex);
            
            // Cập nhật visual
            UpdateView();
        }

        private void UpdateHarvestIndicator(Plant plant, Animal animal)
        {
            // destroy old indicator
            if (_indicatorInstance != null)
            {
                Destroy(_indicatorInstance);
                _indicatorInstance = null;
            }

            if (_controller == null || _controller.Config == null) return;

            int readyCount = 0;
            bool isReady = false;

            GameObject indicatorPrefab = null;

            if (plant != null)
            {
                var equipmentBonus = _controller.Farm.Inventory.GetEquipmentBonus();
                readyCount = plant.GetReadyHarvestCount(System.DateTime.Now, equipmentBonus);
                isReady = readyCount > 0;

                if (isReady)
                {
                    // Stage 3: Sẵn sàng thu hoạch (100%+) → dùng crop prefab (tomato, blueberry, etc.)
                    Debug.Log($"[{plant.CropType}] Stage 3: Ready (100%+ in SpoilageTime)");
                    indicatorPrefab = _farmView.GetCropPrefab(plant.CropType);
                }
                else
                {
                    // Calculate growth stage based on time
                    var cropConfig = _controller.Config.GetCropConfig(plant.CropType.ToString());
                    if (cropConfig != null)
                    {
                        float adjustedGrowthTime = cropConfig.GrowthTimeMinutes / (1f + equipmentBonus);
                        float totalGrowthTime = adjustedGrowthTime * cropConfig.LifespanYields;
                        float halfGrowthTime = totalGrowthTime / 2f;
                        
                        var timeSince = (float)(System.DateTime.Now - plant.LastHarvestTime).TotalMinutes;
                        
                        Debug.Log($"[{plant.CropType}] timeSince={timeSince:F2}min, half={halfGrowthTime:F2}min, total={totalGrowthTime:F2}min");
                        
                        if (timeSince < halfGrowthTime)
                        {
                            // Stage 1: Young/Seedling (0% - 50%)
                            Debug.Log($"[{plant.CropType}] Stage 1: Young (0-50%)");
                            indicatorPrefab = _farmView.GetYoungIndicatorPrefab(plant.CropType);
                        }
                        else
                        {
                            // Stage 2: Growing (50% - 100%)
                            Debug.Log($"[{plant.CropType}] Stage 2: Growing (50-100%)");
                            indicatorPrefab = _farmView.GetGrowingIndicatorPrefab(plant.CropType);
                        }
                    }
                    else
                    {
                        // Fallback if config not found
                        indicatorPrefab = _farmView.GetGrowingIndicatorPrefab(plant.CropType);
                    }
                }
            }
            else if (animal != null)
            {
                var equipmentBonus = _controller.Farm.Inventory.GetEquipmentBonus();
                readyCount = animal.GetReadyProductionCount(System.DateTime.Now, equipmentBonus);
                isReady = readyCount > 0;

                if (isReady)
                {
                    // Stage 3: Sẵn sàng thu hoạch (100%+) → dùng cow prefab
                    Debug.Log($"[{animal.AnimalType}] Stage 3: Ready (100%+ in SpoilageTime)");
                    indicatorPrefab = _farmView.CowPrefab;
                }
                else
                {
                    // Calculate growth stage based on time
                    var animalConfig = _controller.Config.GetAnimalConfig(animal.AnimalType.ToString());
                    if (animalConfig != null)
                    {
                        float adjustedProductionTime = animalConfig.ProductionTimeMinutes / (1f + equipmentBonus);
                        float totalProductionTime = adjustedProductionTime * animalConfig.LifespanProductions;
                        float halfProductionTime = totalProductionTime / 2f;
                        
                        var timeSince = (float)(System.DateTime.Now - animal.LastProductionTime).TotalMinutes;
                        
                        Debug.Log($"[{animal.AnimalType}] timeSince={timeSince:F2}min, half={halfProductionTime:F2}min, total={totalProductionTime:F2}min");
                        
                        if (timeSince < halfProductionTime)
                        {
                            // Stage 1: Young (0% - 50%)
                            Debug.Log($"[{animal.AnimalType}] Stage 1: Young (0-50%)");
                            indicatorPrefab = _farmView.GetYoungIndicatorPrefabForAnimal(animal.AnimalType);
                        }
                        else
                        {
                            // Stage 2: Growing (50% - 100%)
                            Debug.Log($"[{animal.AnimalType}] Stage 2: Growing (50-100%)");
                            indicatorPrefab = _farmView.GetGrowingIndicatorPrefabForAnimal(animal.AnimalType);
                        }
                    }
                    else
                    {
                        // Fallback if config not found
                        indicatorPrefab = _farmView.GetGrowingIndicatorPrefabForAnimal(animal.AnimalType);
                    }
                }
            }

            if (indicatorPrefab == null) return;

            // spawn indicator - tính toán scale để bù trừ parent scale
            var inst = Instantiate(indicatorPrefab);
            
            // Lưu lại scale gốc từ prefab
            Vector3 desiredWorldScale = indicatorPrefab.transform.localScale;
            
            // Set parent trước
            inst.transform.SetParent(transform, false);
            
            // Tính toán local scale cần thiết để đạt được world scale mong muốn
            // localScale = desiredWorldScale / parentLossyScale
            Vector3 parentScale = transform.lossyScale;
            Vector3 compensatedScale = new Vector3(
                desiredWorldScale.x / parentScale.x,
                desiredWorldScale.y / parentScale.y,
                desiredWorldScale.z / parentScale.z
            );
            
            inst.transform.localScale = compensatedScale;
            inst.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            inst.transform.localRotation = Quaternion.identity;

            // nếu indicator có Text component, update số lượng (chỉ khi ready)
            if (isReady)
            {
                // Try UnityEngine.UI.Text
                var uiText = inst.GetComponentInChildren<UnityEngine.UI.Text>(true);
                if (uiText != null)
                {
                    uiText.text = readyCount.ToString();
                }
                else
                {
                    // Try legacy TextMesh
                    var tm = inst.GetComponentInChildren<TextMesh>(true);
                    if (tm != null)
                    {
                        tm.text = readyCount.ToString();
                    }
                    else
                    {
                        // Try TextMeshPro (via reflection to avoid compile dependency)
                        var tmpType = System.Type.GetType("TMPro.TextMeshPro, Unity.TextMeshPro") 
                                      ?? System.Type.GetType("TMPro.TextMeshPro");
                        var tmpUGUIType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") 
                                      ?? System.Type.GetType("TMPro.TextMeshProUGUI");
                        Component tmpComp = null;
                        if (tmpType != null)
                            tmpComp = inst.GetComponentInChildren(tmpType, true);
                        if (tmpComp == null && tmpUGUIType != null)
                            tmpComp = inst.GetComponentInChildren(tmpUGUIType, true);

                        if (tmpComp != null)
                        {
                            var textProp = tmpComp.GetType().GetProperty("text");
                            if (textProp != null && textProp.CanWrite)
                            {
                                textProp.SetValue(tmpComp, readyCount.ToString(), null);
                            }
                        }
                    }
                }
            }

            _indicatorInstance = inst;
        }
    }
}
