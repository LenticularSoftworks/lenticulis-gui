using lenticulis_gui.src.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.App
{
    public class HistoryList
    {
        /// <summary>
        /// Stored actions in linked list for undo redo
        /// </summary>
        private List<HistoryItem> historyList;

        /// <summary>
        /// Index to historyList
        /// </summary>
        private int historyListPointer;

        /// <summary>
        /// Initial max history list size
        /// </summary>
        private const int initSize = 4;

        private static int historyListSize;
        /// <summary>
        /// Max size of history list
        /// </summary>
        public static int HistoryListSize
        {
            get
            {
                return HistoryList.historyListSize;
            }
            set
            {
                if (value > 0)
                    historyListSize = value;
            }
        }

        /// <summary>
        /// reates new list
        /// </summary>
        public HistoryList()
        {
            historyList = new List<HistoryItem>();
            historyListPointer = -1;
            HistoryListSize = initSize;
        }

        /// <summary>
        /// Adds new item to history list
        /// </summary>
        /// <param name="item">history item</param>
        public void AddHistoryItem(HistoryItem item)
        {
            int index = historyList.Count - 1;
            while (index != historyListPointer)
            {
                historyList.RemoveAt(index);
                index--;

                Debug.WriteLine("removing");
            }

            historyList.Add(item);
            historyListPointer++;

            Debug.WriteLine("add {0}", historyListPointer);
        }

        /// <summary>
        /// Undo action
        /// </summary>
        public void Undo()
        {
            if (historyListPointer >= 0)
            {
                if (historyList.Count != 0)
                    historyList.ElementAt(historyListPointer).ApplyUndo();

                historyListPointer--;

                Debug.WriteLine("Undo: {0}, pointer: {1}", historyListPointer + 1, historyListPointer);
            }
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public void Redo()
        {
            if (historyListPointer <= historyList.Count - 1 && historyListPointer >= -1)
            {
                int tmpPointer = historyListPointer;

                if (historyListPointer < historyList.Count - 1)
                    historyListPointer++;
                if (historyListPointer >= 0 && tmpPointer != historyListPointer)
                    historyList.ElementAt(historyListPointer).ApplyRedo();

                Debug.WriteLine("Redo: {0}, pointer: {1}", historyListPointer + 1, historyListPointer);
            }
        }
    }
}
