using lenticulis_gui.src.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Class holds parameters for undo / redo action for LayerObject instances
    /// </summary>
    public class LayerObjectHistory : HistoryItem
    {
        //stored properties for undo
        public float UndoInitialX { get; set; }
        public float UndoInitialY { get; set; }
        public float UndoInitialAngle { get; set; }
        public float UndoInitialScaleX { get; set; }
        public float UndoInitialScaleY { get; set; }
        public Dictionary<TransformType, Transformation> UndoTransformations { get; set; }

        //stored properties for redo
        public float RedoInitialX { get; set; }
        public float RedoInitialY { get; set; }
        public float RedoInitialAngle { get; set; }
        public float RedoInitialScaleX { get; set; }
        public float RedoInitialScaleY { get; set; }
        public Dictionary<TransformType, Transformation> RedoTransformations { get; set; }

        /// <summary>
        /// Instance of LayerObject
        /// </summary>
        public LayerObject Instance { get; set; }

        /// <summary>
        /// Undo action
        /// </summary>
        public override void ApplyUndo()
        {
            Instance.InitialX = this.UndoInitialX;
            Instance.InitialY = this.UndoInitialY;
            Instance.InitialAngle = this.UndoInitialAngle;
            Instance.InitialScaleX = this.UndoInitialScaleX;
            Instance.InitialScaleY = this.UndoInitialScaleY;

            foreach (var item in UndoTransformations)
            {
                Instance.setTransformation(item.Value);
            }
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public override void ApplyRedo()
        {
            Instance.InitialX = this.RedoInitialX;
            Instance.InitialY = this.RedoInitialY;
            Instance.InitialAngle = this.RedoInitialAngle;
            Instance.InitialScaleX = this.RedoInitialScaleX;
            Instance.InitialScaleY = this.RedoInitialScaleY;

            foreach (var item in RedoTransformations)
            {
                Instance.setTransformation(item.Value);
            }
        }

        /// <summary>
        /// Store new action to history list
        /// </summary>
        public override void StoreAction()
        {
            if (Instance != null)
            {
                this.RedoInitialX = Instance.InitialX;
                this.RedoInitialY = Instance.InitialY;
                this.RedoInitialScaleX = Instance.InitialScaleX;
                this.RedoInitialScaleY = Instance.InitialScaleY;
                this.RedoInitialAngle = Instance.InitialAngle;
                this.RedoTransformations = Instance.GetTransformationsCopy();
            }
        }
    }
}
