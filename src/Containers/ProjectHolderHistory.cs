using lenticulis_gui.src.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lenticulis_gui.src.Containers
{
    public class ProjectHolderHistory : HistoryItem
    {
        private string UndoProjectName;
        private string UndoProjectFileName;
        private int UndoImageCount;
        private int UndoLayerCount;
        private int UndoWidth;
        private int UndoHeight;
        private int UndoDpi;
        private int UndoLpi;
        private double UndoViewDistance;
        private double UndoViewAngle;
        private double UndoForeground;
        private double UndoBackground;

        private string RedoProjectName;
        private string RedoProjectFileName;
        private int RedoImageCount;
        private int RedoLayerCount;
        private int RedoWidth;
        private int RedoHeight;
        private int RedoDpi;
        private int RedoLpi;
        private double RedoViewDistance;
        private double RedoViewAngle;
        private double RedoForeground;
        private double RedoBackground;


        public override void ApplyUndo()
        {
            throw new NotImplementedException();
        }

        public override void ApplyRedo()
        {
            throw new NotImplementedException();
        }

        public void SaveUndo()
        {
            UndoProjectName = ProjectHolder.ProjectName;
            UndoProjectFileName = ProjectHolder.ProjectFileName;
            UndoImageCount = ProjectHolder.ImageCount;
            UndoLayerCount = ProjectHolder.LayerCount;
            UndoWidth = ProjectHolder.Width;
            UndoHeight = ProjectHolder.Height;
            UndoDpi = ProjectHolder.Dpi;
            UndoLpi = ProjectHolder.Lpi;
            UndoViewDistance = ProjectHolder.ViewDistance;
            UndoViewAngle = ProjectHolder.ViewAngle;
            UndoForeground = ProjectHolder.Foreground;
            UndoBackground = ProjectHolder.Background;
        }

        public void SaveRedo()
        {
            RedoProjectName = ProjectHolder.ProjectName;
            RedoProjectFileName = ProjectHolder.ProjectFileName;
            RedoImageCount = ProjectHolder.ImageCount;
            RedoLayerCount = ProjectHolder.LayerCount;
            RedoWidth = ProjectHolder.Width;
            RedoHeight = ProjectHolder.Height;
            RedoDpi = ProjectHolder.Dpi;
            RedoLpi = ProjectHolder.Lpi;
            RedoViewDistance = ProjectHolder.ViewDistance;
            RedoViewAngle = ProjectHolder.ViewAngle;
            RedoForeground = ProjectHolder.Foreground;
            RedoBackground = ProjectHolder.Background;
        }
    }
}
