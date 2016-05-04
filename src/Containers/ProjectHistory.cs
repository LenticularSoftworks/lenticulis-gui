using lenticulis_gui.src.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Undo /redo holder for project settings
    /// </summary>
    public class ProjectHistory : HistoryItem
    {
        /// <summary>
        /// Undo project properties
        /// </summary>
        private int UndoWidth;
        private int UndoHeight;
        private int UndoDpi;
        private int UndoLpi;
        private int UndoLayerCount;
        private int UndoImageCount;

        /// <summary>
        /// Redo project propertios
        /// </summary>
        private int RedoWidth;
        private int RedoHeight;
        private int RedoDpi;
        private int RedoLpi;
        private int RedoLayerCount;
        private int RedoImageCount;

        /// <summary>
        /// Scale X property is set when rescaling layers with canvas size
        /// </summary>
        public float ScaleX { get; set; }

        /// <summary>
        /// Scale X property is set when rescaling layers with canvas size
        /// </summary>
        public float ScaleY { get; set; }

        /// <summary>
        /// HistoryItem list of actions
        /// </summary>
        private List<HistoryItem> deletedList;

        /// <summary>
        /// Undo action
        /// </summary>
        public override void ApplyUndo()
        {
            ProjectHolder.Width = UndoWidth;
            ProjectHolder.Height = UndoHeight;
            ProjectHolder.Dpi = UndoDpi;
            ProjectHolder.Lpi = UndoLpi;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            //update fram count
            mw.UpdateImageCount(UndoImageCount, null);

            //apply undo to layers and layer objects
            if (deletedList != null)
            {
                foreach (var item in deletedList)
                    item.ApplyUndo();
            }

            //rescale all layers with 1 / scale
            if (ScaleX != 0 && ScaleY != 0)
            {
                mw.RefreshCanvasList();
                mw.RescaleLayers(1 / ScaleX, 1 / ScaleY, true);

                return;
            }

            mw.PropertyChanged3D();
            mw.RefreshCanvasList();
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public override void ApplyRedo()
        {
            ProjectHolder.Width = RedoWidth;
            ProjectHolder.Height = RedoHeight;
            ProjectHolder.Dpi = RedoDpi;
            ProjectHolder.Lpi = RedoLpi;

            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            mw.UpdateImageCount(RedoImageCount, null);

            //redo on stored history item
            if (deletedList != null)
            {
                foreach (var item in deletedList)
                    item.ApplyRedo();
            }

            //rescale layers
            if (ScaleX != 0 && ScaleY != 0)
            {
                mw.RefreshCanvasList();
                mw.RescaleLayers(ScaleX, ScaleY, true);

                return;
            }

            mw.PropertyChanged3D();
            mw.RefreshCanvasList();
        }

        /// <summary>
        /// Store undo properties
        /// </summary>
        public void SaveUndo()
        {
            UndoWidth = ProjectHolder.Width;
            UndoHeight = ProjectHolder.Height;
            UndoDpi = ProjectHolder.Dpi;
            UndoLpi = ProjectHolder.Lpi;
            UndoLayerCount = ProjectHolder.LayerCount;
            UndoImageCount = ProjectHolder.ImageCount;
        }

        /// <summary>
        /// Store redo properties
        /// </summary>
        public void SaveRedo()
        {
            RedoWidth = ProjectHolder.Width;
            RedoHeight = ProjectHolder.Height;
            RedoDpi = ProjectHolder.Dpi;
            RedoLpi = ProjectHolder.Lpi;
            RedoLayerCount = ProjectHolder.LayerCount;
            RedoImageCount = ProjectHolder.ImageCount;
        }

        /// <summary>
        /// Store timeline history items
        /// </summary>
        /// <param name="deletedItem"></param>
        public void StoreDeletedItem(TimelineItem deletedItem)
        {
            if (deletedList == null)
                deletedList = new List<HistoryItem>();

            TimelineItemHistory history = deletedItem.GetHistoryItem();
            history.RemoveAction = true;

            //insert to begin of list to keep operation order
            deletedList.Insert(0, history);
        }

        /// <summary>
        /// Store layer history items
        /// </summary>
        /// <param name="layerHistory"></param>
        public void StoreDeletedLayer(LayerHistory layerHistory)
        {
            if (deletedList == null)
                deletedList = new List<HistoryItem>();

            deletedList.Insert(0, layerHistory);
        }

    }
}
