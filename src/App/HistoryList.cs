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
        /// Initial max process memory usage [MB]
        /// </summary>
        private const int initSize = 1000;

        /// <summary>
        /// Minimum of history list items
        /// </summary>
        private const int minHistoryListSize = 10;

        private static long memorySize;
        /// <summary>
        /// Max memory usage of process i MB. Used to 
        /// reduce memory usage by removing history list items
        /// </summary>
        public static long MemorySize
        {
            get
            {
                return HistoryList.memorySize;
            }
            set
            {
                if (value > 0)
                    memorySize = value;
            }
        }

        /// <summary>
        /// reates new list
        /// </summary>
        public HistoryList()
        {
            historyList = new List<HistoryItem>();
            historyListPointer = -1;
            MemorySize = initSize;
        }

        /// <summary>
        /// Adds new item to history list
        /// </summary>
        /// <param name="item">history item</param>
        public void AddHistoryItem(HistoryItem item)
        {
            int index = historyList.Count - 1;

            //remove all items above history pointer
            while (index != historyListPointer)
            {
                DisposeHistoryItem(index);
                index--;
            }

            historyList.Add(item);
            historyListPointer++;

            //check history list size and free memory if needed
            FreeHistoryList();

            //Debug.WriteLine("add {0}", historyListPointer);
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

                //Debug.WriteLine("Undo: {0}, pointer: {1}", historyListPointer + 1, historyListPointer);
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

                //Debug.WriteLine("Redo: {0}, pointer: {1}", historyListPointer - 1, historyListPointer);
            }
        }

        /// <summary>
        /// Check process memory usage and remove history items if memory
        /// is more than defined value
        /// </summary>
        private void FreeHistoryList()
        {
            Process process = Process.GetCurrentProcess();

            process.Refresh();
            long megaBytes = (long)(process.WorkingSet64 / (1024f * 1024f));

            //remove first N items to reduce  memory
            //if historyList count is more than minimum and memory more than maximum memory value, remove items
            while (megaBytes > memorySize && historyList.Count > minHistoryListSize)
            {
                DisposeHistoryItem(0);
                historyListPointer--;
            }

            //process.Refresh();
            //Debug.WriteLine("{0} | {1}", megaBytes, (long)(process.WorkingSet64 / (1024f * 1024f)));
        }

        /// <summary>
        /// Remove history item from history list.
        /// </summary>
        /// <param name="index">Index of item in history list</param>
        private void DisposeHistoryItem(int index)
        {
            HistoryItem item = historyList.ElementAt(index);

            //if is type of timelinehistory call dispose to unload image from storage
            if (item.GetType() == typeof(TimelineItemHistory))
            {
                ((TimelineItemHistory)item).Dispose();
            }

            historyList.RemoveAt(index);

            //Debug.WriteLine("free " + index);
        }
    }
}
