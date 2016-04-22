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
        /// Item is in project flag
        /// </summary>
        private bool inProject = true;

        private bool addAction;
        /// <summary>
        /// Timeline item was added to timeline
        /// </summary>
        public bool AddAction
        {
            get
            {
                return addAction;
            }
            set
            {
                addAction = value;
                if (value == true)
                    inProject = true;
            }
        }

        private bool removeAction;
        /// <summary>
        /// Timelineitem was removed from timeline
        /// </summary>
        public bool RemoveAction
        {
            get
            {
                return removeAction;
            }
            set
            {
                removeAction = value;
                if (value == true)
                    inProject = false;
            }
        }

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
            {
                mw.RemoveTimelineItem(Instance, false);
                inProject = false;
            }
            else if (RemoveAction)
            {
                mw.AddTimelineItem(Instance, false, false);
                inProject = true;
            }

            Instance.SetPosition(UndoRow, UndoColumn, UndoLength);
        }

        /// <summary>
        /// Action for redo
        /// </summary>
        public override void ApplyRedo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (AddAction)
            {
                mw.AddTimelineItem(Instance, false, false);
                inProject = true;
            }
            else if (RemoveAction)
            {
                mw.RemoveTimelineItem(Instance, false);
                inProject = false;
            }

            Instance.SetPosition(RedoRow, RedoColumn, RedoLength);
        }

        /// <summary>
        /// Store new action to history list
        /// </summary>
        public void StoreRedo()
        {
            if (Instance != null)
            {
                this.RedoRow = Instance.GetLayerObject().Layer;
                this.RedoColumn = Instance.GetLayerObject().Column;
                this.RedoLength = Instance.GetLayerObject().Length;
            }
        }

        /// <summary>
        /// Unload image from storage if current action is RemoveImage
        /// </summary>
        public void Dispose()
        {
            if (!inProject)
            {
                Instance.GetLayerObject().unloadImage();
                //Debug.WriteLine("---> unloading image from storage " + Instance.Name);
            }
        }
    }
}