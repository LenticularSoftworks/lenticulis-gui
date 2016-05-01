using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for TransformationsWindow.xaml
    /// </summary>
    public partial class TransformationsWindow : MetroWindow
    {
        private TimelineItem sourceItem;

        /// <summary>
        /// Only one constructor - just retains item we are setting transformations into
        /// </summary>
        /// <param name="titem">timeline item, where we put our modifications</param>
        public TransformationsWindow(TimelineItem titem)
        {
            InitializeComponent();

            Title = LangProvider.getString("TRANSFORMATION_WINDOW_TITLE");

            sourceItem = titem;

            LayerObject lobj = titem.GetLayerObject();

            // prepare interpolation type dictionaries (dynamically)
            Dictionary<InterpolationType, int> interpPos = new Dictionary<InterpolationType, int>()
            {
                { InterpolationType.Linear, 0},
                { InterpolationType.Quadratic, 1},
                { InterpolationType.Cubic, 2},
                { InterpolationType.Goniometric, 3},
            };

            // prepare array of values
            List<KeyValuePair<String, String>> cblist = new List<KeyValuePair<String, String>>();
            cblist.Add(new KeyValuePair<String, String>(LangProvider.getString("CBOX_INTERP_ITEM_LINEAR"), InterpolationType.Linear.ToString()));
            cblist.Add(new KeyValuePair<String, String>(LangProvider.getString("CBOX_INTERP_ITEM_QUADRATIC"), InterpolationType.Quadratic.ToString()));
            cblist.Add(new KeyValuePair<String, String>(LangProvider.getString("CBOX_INTERP_ITEM_CUBIC"), InterpolationType.Cubic.ToString()));
            cblist.Add(new KeyValuePair<String, String>(LangProvider.getString("CBOX_INTERP_ITEM_GONIOMETRIC"), InterpolationType.Goniometric.ToString()));

            // create items in comboboxes
            foreach (var name in cblist)
            {
                // we add unique distinctive prefix to every name of combo box item
                TranslationInterpolation.Items.Add(new ComboBoxItem() { Content = name.Key, Name = "CBT_"+name.Value });
                RotationInterpolation.Items.Add(new ComboBoxItem() { Content = name.Key, Name = "CBR_" + name.Value });
                ScaleInterpolation.Items.Add(new ComboBoxItem() { Content = name.Key, Name = "CBS_" + name.Value });
            }

            // restore parameters of every type of transformation

            // translation
            Transformation tr = lobj.getTransformation(TransformType.Translation);
            TranslationEditX.Text = tr.TransformX.ToString();
            TranslationEditY.Text = tr.TransformY.ToString();

            // combo box item value
            TranslationInterpolation.SelectedIndex = interpPos[tr.Interpolation];

            // rotation
            tr = lobj.getTransformation(TransformType.Rotate);
            RotationEdit.Text = tr.TransformAngle.ToString();

            RotationInterpolation.SelectedIndex = interpPos[tr.Interpolation];

            // scale
            tr = lobj.getTransformation(TransformType.Scale);
            ScaleEditX.Text = tr.TransformX.ToString();
            ScaleEditY.Text = tr.TransformY.ToString();

            ScaleInterpolation.SelectedIndex = interpPos[tr.Interpolation];
        }

        /// <summary>
        /// User clicked on cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// User clicked on OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            float transX, transY, rot, scaleX, scaleY;

            // translation values must be numeric
            if (!float.TryParse(TranslationEditX.Text.Replace('.', ','), out transX) || !float.TryParse(TranslationEditY.Text.Replace('.', ','), out transY))
            {
                MessageBox.Show(LangProvider.getString("MSG_TRANSLATION_NOT_FLOAT"), LangProvider.getString("MSG_TRANS_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // rotation value must be numeric
            if (!float.TryParse(RotationEdit.Text.Replace('.', ','), out rot))
            {
                MessageBox.Show(LangProvider.getString("MSG_ROTATION_NOT_FLOAT"), LangProvider.getString("MSG_TRANS_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // scale value must be numeric
            if (!float.TryParse(ScaleEditX.Text.Replace('.', ','), out scaleX) || !float.TryParse(ScaleEditY.Text.Replace('.', ','), out scaleY))
            {
                MessageBox.Show(LangProvider.getString("MSG_SCALE_NOT_FLOAT"), LangProvider.getString("MSG_TRANS_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LayerObject lobj = sourceItem.GetLayerObject();
            //save history
            LayerObjectHistory history = lobj.GetHistoryItem();

            // update transformations
            lobj.getTransformation(TransformType.Translation).setVector(transX, transY);
            lobj.getTransformation(TransformType.Rotate).setAngle(rot);
            lobj.getTransformation(TransformType.Scale).setVector(scaleX, scaleY);

            // update interpolation types
            var arr = Enum.GetValues(typeof(InterpolationType));
            ComboBoxItem cbi;
            foreach (InterpolationType it in arr)
            {
                cbi = TranslationInterpolation.SelectedItem as ComboBoxItem;
                if (cbi != null && cbi.Name.Substring(4).Equals(it.ToString()))
                    lobj.getTransformation(TransformType.Translation).Interpolation = it;

                cbi = RotationInterpolation.SelectedItem as ComboBoxItem;
                if (cbi != null && cbi.Name.Substring(4).Equals(it.ToString()))
                    lobj.getTransformation(TransformType.Rotate).Interpolation = it;

                cbi = ScaleInterpolation.SelectedItem as ComboBoxItem;
                if (cbi != null && cbi.Name.Substring(4).Equals(it.ToString()))
                    lobj.getTransformation(TransformType.Scale).Interpolation = it;
            }

            // propagate changes to main window and repaint canvas
            MainWindow mw = System.Windows.Application.Current.MainWindow as MainWindow;
            mw.RefreshCanvasList(false);
            ScrollViewer sw = mw.GetCurrentCanvas();
            if (sw != null)
            {
                WorkCanvas wc = sw.Content as WorkCanvas;
                if (wc != null)
                    wc.Paint();
            }

            //save undo action to history
            history.StoreRedo();
            mw.AddToHistoryList(history);

            Close();
        }
    }
}
