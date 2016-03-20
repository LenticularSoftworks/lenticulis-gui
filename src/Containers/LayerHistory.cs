﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Layer operations history for undo / redo operations
    /// </summary>
    public class LayerHistory : HistoryItem
    {
        /// <summary>
        /// Layer id
        /// </summary>
        public int LayerId { get; set; }

        /// <summary>
        /// Add layer flag
        /// </summary>
        public bool AddLayer { get; set; }
        /// <summary>
        /// Remove layer flag
        /// </summary>
        public bool RemoveLayer { get; set; }
        /// <summary>
        /// LayerUp flag
        /// </summary>
        public bool UpLayer { get; set; }
        /// <summary>
        /// LayerDown flag
        /// </summary>
        public bool DownLayer { get; set; }

        /// <summary>
        /// Set to true when call undo
        /// </summary>
        private bool IsUndo = false;

        /// <summary>
        /// TimelineItemHistory list od deleted items
        /// </summary>
        private List<TimelineItemHistory> deletedList;

        /// <summary>
        /// Undo implementation of LayerHistory
        /// </summary>
        public override void ApplyUndo()
        {
            if (IsUndo)
                return;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (AddLayer)
                mw.RemoveFirstLayer();
            else if (UpLayer)
                mw.LayerDown(LayerId - 1);
            else if (DownLayer)
                mw.LayerUp(LayerId + 1);
            else if (RemoveLayer)
            {
                //add layer first
                mw.AddTimelineLayer(1, false, false);

                foreach (var item in deletedList)
                    item.ApplyUndo();
            }

            IsUndo = true;
        }

        /// <summary>
        /// Redo implementation of LayerHistory
        /// </summary>
        public override void ApplyRedo()
        {
            if (!IsUndo)
                return;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (AddLayer)
                mw.AddTimelineLayer(1, false, true);
            else if (UpLayer)
                mw.LayerUp(LayerId);
            else if (DownLayer)
                mw.LayerDown(LayerId);
            else if (RemoveLayer)
            {
                //return images first
                foreach (var item in deletedList)
                    item.ApplyRedo();

                mw.RemoveLastLayer(false);
            }

            IsUndo = false;
        }

        /// <summary>
        /// Store TimelineitemHistory items to re-add last layer
        /// </summary>
        /// <param name="deletedItems"></param>
        public void StoreAction(List<TimelineItem> deletedItems)
        {
            deletedList = new List<TimelineItemHistory>();

            foreach (var item in deletedItems)
            {
                TimelineItemHistory history = item.GetHistoryItem();
                history.RemoveAction = true;

                //insert to begin of list to keep operation order
                deletedList.Insert(0, history);
            }
        }
    }
}