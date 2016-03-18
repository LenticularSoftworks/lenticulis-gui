using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class TimelineItemHistory : HistoryItem
    {
        /// <summary>
        /// Undo properties
        /// </summary>
        public int UndoRow { get; set; }
        public int UndoColumn { get; set; }
        public int UndoLength { get; set; }

        /// <summary>
        /// Redo properties
        /// </summary>
        private int RedoRow { get; set; }
        private int RedoColumn { get; set; }
        private int RedoLength { get; set; }

        /// <summary>
        /// Timeline item was added to timeline
        /// </summary>
        public bool AddAction { get; set; }

        /// <summary>
        /// Timelineitem was removed from timeline
        /// </summary>
        public bool RemoveAction { get; set; }

        /// <summary>
        /// Instance of TimelineItem
        /// </summary>
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

            Instance.SetPosition(RedoRow, RedoColumn, RedoLength);
        }

        /// <summary>
        /// Store new action to history list
        /// </summary>
        public void StoreAction()
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