using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class TimelineItemHistory : HistoryItem
    {

        public int UndoRow { get; set; }
        public int UndoColumn { get; set; }
        public int UndoLength { get; set; }

        public int RedoRow { get; set; }
        public int RedoColumn { get; set; }
        public int RedoLength { get; set; }

        public TimelineItem Instance { get; set; }

        /// <summary>
        /// Action for undo
        /// </summary>
        public override void ApplyUndo()
        {
            Instance.SetPosition(UndoRow, UndoColumn, UndoLength);
        }

        /// <summary>
        /// Action for redo
        /// </summary>
        public override void ApplyRedo()
        {
            Instance.SetPosition(RedoRow, RedoColumn, RedoLength);
        }

        /// <summary>
        /// Store new action to history list
        /// </summary>
        public override void StoreAction()
        {
            if (Instance != null)
            {
                this.RedoRow= Instance.GetLayerObject().Layer;
                this.RedoColumn = Instance.GetLayerObject().Column;
                this.RedoLength = Instance.GetLayerObject().Length;
            }
        }
    }
}
