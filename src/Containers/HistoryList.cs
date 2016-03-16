using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class HistoryList
    {
        /// <summary>
        /// Stored actions in linked list for undo redo
        /// </summary>
        private LinkedList<HistoryItem> historyList;

        /// <summary>
        /// Index to historyList
        /// </summary>
        private int historyListPointer;

        public HistoryList()
        {
            historyList = new LinkedList<HistoryItem>();
            historyListPointer = 0;
        }

        /// <summary>
        /// Adds new item to history list
        /// </summary>
        /// <param name="item">history item</param>
        public void AddHistoryItem(HistoryItem item)
        {
            if (historyList.Count > 0)
            {
                while (historyList.Count - 1 != historyListPointer)
                    historyList.RemoveLast();
            }

            historyList.AddLast(item);
            historyListPointer = historyList.Count - 1;
        }

        /// <summary>
        /// Undo action
        /// </summary>
        public void Undo()
        {
            if (historyList.Count == 0)
                return;

            historyList.ElementAt(historyListPointer).ApplyUndo();

            if (historyListPointer > 0)
                historyListPointer--;
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public void Redo() 
        {
            if (historyListPointer < historyList.Count - 1)
                historyListPointer++;

            historyList.ElementAt(historyListPointer).ApplyRedo();
        }
    }
}
