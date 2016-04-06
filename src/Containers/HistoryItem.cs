using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Abstract class contains abstract methods for undo and redo to be implemented
    /// </summary>
    public abstract class HistoryItem
    {
        /// <summary>
        /// Action for undo
        /// </summary>
        public abstract void ApplyUndo();

        /// <summary>
        /// Action for redo
        /// </summary>
        public abstract void ApplyRedo();
    }
}
