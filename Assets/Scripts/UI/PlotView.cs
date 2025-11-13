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
        private GameObject _visualInstance;
        private GameObject _indicatorInstance; // indicator hiển thị trạng thái thu hoạch


        public void Initialize(int plotIndex, Farm3DView farmView, GameController controller)
        {
            _plotIndex = plotIndex;
            _farmView = farmView;
            _controller = controller;
            UpdateView();
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
                var prefab = _farmView.GetCropPrefab(plot.Plant.CropType);
                if (prefab != null)
                {
                    SpawnVisual(prefab);
                    UpdateHarvestIndicator(plot.Plant, null);
                }
            }
            else if (plot.Animal != null)
            {
                var pf = _farmView.CowPrefab;
                if (pf != null)
                {
                    SpawnVisual(pf);
                    UpdateHarvestIndicator(null, plot.Animal);
                }
            }
            else
            {
                // nothing on plot
                if (_visualInstance != null)
                {
                    Destroy(_visualInstance);
                    _visualInstance = null;
                }
                if (_indicatorInstance != null)
                {
                    Destroy(_indicatorInstance);
                    _indicatorInstance = null;
                }
            }
        }

        void OnMouseDown()
        {
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

         public void SpawnVisual(GameObject prefab)
        {
            // remove old visual
            if (_visualInstance != null) Destroy(_visualInstance);
            if (prefab == null) return;

            // instantiate prefab and preserve its original local scale
            var inst = Instantiate(prefab);
            inst.transform.localScale = prefab.transform.localScale;

            // parent to this transform WITHOUT keeping world position so we can set local position precisely
            inst.transform.SetParent(transform, false);

            // set the required local position on the plot
            inst.transform.localPosition = new Vector3(0f, 1f, 0f);

            // reset rotation if needed
            inst.transform.localRotation = Quaternion.identity;

            _visualInstance = inst;
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
                    // sẵn sàng thu hoạch → dùng harvestReadyIndicatorPrefab
                    indicatorPrefab = _farmView.HarvestReadyIndicatorPrefab;
                }
                else
                {
                    // đang lớn → dùng growing indicator theo loại cây
                    indicatorPrefab = _farmView.GetGrowingIndicatorPrefab(plant.CropType);
                }
            }
            else if (animal != null)
            {
                var equipmentBonus = _controller.Farm.Inventory.GetEquipmentBonus();
                readyCount = animal.GetReadyProductionCount(System.DateTime.Now, equipmentBonus);
                isReady = readyCount > 0;

                if (isReady)
                {
                    // sẵn sàng thu hoạch → dùng harvestReadyIndicatorPrefab
                    indicatorPrefab = _farmView.HarvestReadyIndicatorPrefab;
                }
                else
                {
                    // đang lớn → dùng growing indicator theo loại động vật
                    indicatorPrefab = _farmView.GetGrowingIndicatorPrefabForAnimal(animal.AnimalType);
                }
            }

            if (indicatorPrefab == null) return;

            // spawn indicator bên cạnh visual (offset sang phải)
            var inst = Instantiate(indicatorPrefab);
            inst.transform.SetParent(transform, false);
            inst.transform.localPosition = new Vector3(0f, 1.5f, 0f); // bên cạnh cây/bò
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = indicatorPrefab.transform.localScale;

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
