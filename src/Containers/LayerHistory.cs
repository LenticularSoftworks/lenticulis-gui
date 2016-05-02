using System;
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
        /// Depth in inches
        /// </summary>
        public double LayerDepth { get; set; }
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
        /// Redo depth value
        /// </summary>
        public double DepthRedo {get; set;}

        /// <summary>
        /// Depth change
        /// </summary>
        public bool DepthChange { get; set; }

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

            //select operation
            if (AddLayer)
                mw.RemoveFirstLayer();
            else if (UpLayer)
                mw.LayerDown(LayerId - 1);
            else if (DownLayer)
                mw.LayerUp(LayerId + 1);
            else if (RemoveLayer)
            {
                //add layer first
                mw.AddTimelineLayer(1, false, false, LayerDepth);

                foreach (var item in deletedList)
                    item.ApplyUndo();
            }
            else if(DepthChange)
                mw.ConvertDepthBoxSelection(LayerId, LayerDepth);

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

            //Select operation
            if (AddLayer)
                mw.AddTimelineLayer(1, false, true, LayerDepth); //adding and removing layer doesnt change depth
            else if (UpLayer)
                mw.LayerUp(LayerId);
            else if (DownLayer)
                mw.LayerDown(LayerId);
            else if (RemoveLayer)
            {
                //return images first
                foreach (var item in deletedList)
                    item.ApplyRedo();

                mw.RemoveLastLayer(false, null);
            }
            else if(DepthChange)
                mw.ConvertDepthBoxSelection(LayerId, DepthRedo);

            IsUndo = false;
        }

        /// <summary>
        /// Store TimelineitemHistory items to re-add last layer
        /// </summary>
        /// <param name="deletedItems"></param>
        public void StoreDeletedItem(List<TimelineItem> deletedItems)
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

        /// <summary>
        /// If stored action was remove layer call dispose to every
        /// TimelineHistoryItem object to unload image from storage
        /// </summary>
        public void Dispose()
        {
            if (RemoveLayer && deletedList != null)
            {
                foreach (var item in deletedList)
                {
                    item.Dispose();
                }
            }
        }
    }
}
