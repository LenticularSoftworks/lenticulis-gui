using lenticulis_gui.src.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.App
{
    /// <summary>
    /// Class represents list of operation for calling Undo and Redo functions in project.
    /// </summary>
    public class HistoryList
    {
        /// <summary>
        /// Stored actions in list for undo redo
        /// </summary>
        private List<HistoryItem> historyList;

        /// <summary>
        /// Index to historyList
        /// </summary>
        public int HistoryListPointer { get; private set; }

        /// <summary>
        /// Public history list size property
        /// </summary>
        public int HistoryListSize
        {
            get
            {
                return historyList.Count;
            }
        }

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
            HistoryListPointer = -1;
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
            while (index != HistoryListPointer)
            {
                DisposeHistoryItem(index);
                index--;
            }

            historyList.Add(item);
            HistoryListPointer++;

            //check history list size and free memory if needed
            FreeHistoryList();
        }

        /// <summary>
        /// Undo action
        /// </summary>
        public void Undo()
        {
            if (HistoryListPointer >= 0)
            {
                if (historyList.Count != 0)
                    historyList.ElementAt(HistoryListPointer).ApplyUndo();

                HistoryListPointer--;
            }
        }

        /// <summary>
        /// Redo action
        /// </summary>
        public void Redo()
        {
            if (HistoryListPointer <= historyList.Count - 1 && HistoryListPointer >= -1)
            {
                int tmpPointer = HistoryListPointer;

                if (HistoryListPointer < historyList.Count - 1)
                    HistoryListPointer++;
                if (HistoryListPointer >= 0 && tmpPointer != HistoryListPointer)
                    historyList.ElementAt(HistoryListPointer).ApplyRedo();
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
                HistoryListPointer--;
            }

            process.Refresh();
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
        }
    }
}
