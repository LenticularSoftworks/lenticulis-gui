using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// History of global 3D settings
    /// </summary>
    public class ProjectHistory3D : HistoryItem
    {
        // Undo text values
        public string UndoDistance { get; set; }
        public string UndoAngle { get; set; }
        public string UndoForeground { get; set; }
        public string UndoBackground { get; set; }
        public string UndoUnits { get; set; }

        // Redo text values
        public string RedoDistance { get; set; }
        public string RedoAngle { get; set; }
        public string RedoForeground { get; set; }
        public string RedoBackground { get; set; }
        public string RedoUnits { get; set; }

        /// <summary>
        /// Undo action
        /// </summary>
        public override void ApplyUndo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            mw.Set3DInputs(UndoAngle, UndoDistance, UndoForeground, UndoBackground, UndoUnits);
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public override void ApplyRedo()
        {
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            mw.Set3DInputs(RedoAngle, RedoDistance, RedoForeground, RedoBackground, RedoUnits);
        }
    }
}
