using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class LayerHistory : HistoryItem
    {
        public int LayerId { get; set; }
        public string Depth { get; set; }

        public bool AddLayer { get; set; }

        private bool IsUndo = false;

        public override void ApplyUndo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (AddLayer && !IsUndo)
            {
                mw.RemoveFirstLayer();
                IsUndo = true;
            }

        }

        public override void ApplyRedo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;

            if (AddLayer && IsUndo)
            {
                mw.AddTimelineLayer(1, false);
                IsUndo = false;
            }
        }

        public override void StoreAction()
        {
            throw new NotImplementedException();
        }
    }
}
