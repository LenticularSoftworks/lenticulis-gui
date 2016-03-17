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

        public int RedoRow { get; set; } //TODO private
        public int RedoColumn { get; set; }
        public int RedoLength { get; set; }

        public bool AddAction { get; set; }
        public bool RemoveAction { get; set; }

        public TimelineItem Instance { get; set; }

        /// <summary>
        /// Action for undo
        /// </summary>
        public override void ApplyUndo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            if (AddAction)
                mw.RemoveTimelineItem(Instance, false);
            else if (RemoveAction)
                mw.AddTimelineItem(Instance, false, false);
            else
                Instance.SetPosition(UndoRow, UndoColumn, UndoLength);
        }

        /// <summary>
        /// Action for redo
        /// </summary>
        public override void ApplyRedo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            if (AddAction)
                mw.AddTimelineItem(Instance, false, false);
            else if (RemoveAction)
                mw.RemoveTimelineItem(Instance, false);
            else
                Instance.SetPosition(RedoRow, RedoColumn, RedoLength);
        }

        /// <summary>
        /// Store new action to history list
        /// </summary>
        public override void StoreAction()
        {
            if (Instance != null)
            {
                this.RedoRow = Instance.GetLayerObject().Layer;
                this.RedoColumn = Instance.GetLayerObject().Column;
                this.RedoLength = Instance.GetLayerObject().Length;
            }
        }
    }
}
