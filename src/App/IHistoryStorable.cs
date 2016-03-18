using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    /// <summary>
    /// Interface defines method that return its historyItem instance 
    /// </summary>
    /// <typeparam name="T">HistoryItem type</typeparam>
    interface IHistoryStorable<T> where T: HistoryItem
    {
        /// <summary>
        /// Returns HistoryItem instance
        /// </summary>
        /// <returns>History item type</returns>
        T GetHistoryItem();
    }
}
