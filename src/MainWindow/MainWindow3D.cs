using lenticulis_gui.src.App;
using lenticulis_gui.src.Containers;
using lenticulis_gui.src.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace lenticulis_gui
{
    public partial class MainWindow
    {
        /// <summary>
        /// Centimeters to one inch
        /// </summary>
        private const float cmToInch = 2.54f;

        /// <summary>
        /// 5% of width
        /// </summary>
        private const float depthPercent = 0.05f;

        /// <summary>
        /// Unit convert multiplier
        /// </summary>
        private float unitConvert = 1;

        /// <summary>
        /// Real width of image
        /// </summary>
        private float realWidth;

        /// <summary>
        /// Frame disparity spacing
        /// </summary>
        private int frameSpacing = 0;

        /// <summary>
        /// Generate flag
        /// </summary>
        private bool generate = false;

        /// <summary>
        /// Selected units
        /// </summary>
        private LengthUnits units;

        /// <summary>
        /// Conversion value
        /// </summary>
        private float unitToInches = 1;

        /// <summary>
        /// wont fire textchange event if false
        /// </summary>
        private bool textChange = true;

        /// <summary>
        /// Serve lost focus depth textbox if true
        /// </summary>
        private bool checkFocus = false;

        /// <summary>
        /// Layer history item
        /// </summary>
        private HistoryItem historyItem = null;

        #region 3D methods
        /// <summary>
        /// Sets width text in 3D panel
        /// </summary>
        private void SetWidthText()
        {
            if (ProjectHolder.Width == 0 || ProjectHolder.Dpi == 0)
            {
                Width3D.Content = "";
                return;
            }

            float width = (ProjectHolder.Width / (float)ProjectHolder.Dpi) * unitToInches;
            realWidth = (int)Math.Round(width * 1000) / 1000.0f;

            Width3D.Content = realWidth + " " + Units3D.SelectedValue;
        }

        /// <summary>
        /// Set recommended foreground and background values: +- 5% of image width
        /// </summary>
        private void SetDepthBounds()
        {
            if (realWidth != 0.0f && Foreground3D.Text == "" && Background3D.Text == "")
            {
                float depthBound = realWidth * depthPercent;
                depthBound = (int)(depthBound * 100) / 100.0f;

                Foreground3D.Text = depthBound.ToString();
                Background3D.Text = (-1 * depthBound).ToString();
            }
        }

        /// <summary>
        /// Set frame spacing text
        /// </summary>
        private void SetSpacingText()
        {
            //initial
            double distance = Double.NegativeInfinity;
            double angle = Double.NegativeInfinity;

            if (Double.TryParse(ViewAngle3D.Text, out angle) && Double.TryParse(ViewDist3D.Text, out distance))
            {
                //must be higher than 0
                if (distance > 0.0 && angle > 0.0)
                {
                    //show frame spacing
                    frameSpacing = Generator3D.CalculateZoneDistance(distance / unitToInches, angle, ProjectHolder.ImageCount);
                    FrameSpacing3D.Content = frameSpacing.ToString();
                }

                //enable / disable generate button
                if (frameSpacing >= 1)
                {
                    ViewAngle3D.Background = Brushes.White;
                    ViewDist3D.Background = Brushes.White;
                }
                else
                {
                    ViewAngle3D.Background = Brushes.Firebrick;
                    ViewDist3D.Background = Brushes.Firebrick;
                }
            }
        }

        /// <summary>
        /// Converts text inputs when units are changed
        /// </summary>
        /// <param name="input"></param>
        private void ConvertTextInput(TextBox input)
        {
            double value;

            if (input.Text == "")
                return;

            if (Double.TryParse(input.Text, out value))
            {
                value *= unitConvert;
                value = Math.Round(value * 1000) / 1000.0;

                input.Text = value + "";
                input.Background = Brushes.White;
            }
            else
            {
                Warning3D.Content = LangProvider.getString("INVALID_3D_PARAMETERS");
                input.Background = Brushes.Firebrick;
            }
        }

        /// <summary>
        /// Reset 3D inputs
        /// </summary>
        public void Clear3D()
        {
            UnitsDepth.SelectedItem = LengthUnits.@in;
            Units3D.SelectedItem = LengthUnits.@in;
        }

        /// <summary>
        /// Set 3D parameters inputs
        /// </summary>
        /// <param name="angle">Angle</param>
        /// <param name="distance">Distance</param>
        /// <param name="foreground">Foreground</param>
        /// <param name="background">Background</param>
        /// <param name="units">units</param>
        public void Set3DInputs(string angle, string distance, string foreground, string background, string units)
        {
            var values = Enum.GetValues(typeof(LengthUnits));
            foreach (var value in values)
            {
                if (value.ToString() == units)
                {
                    Units3D.SelectedItem = value;
                }
            }

            ViewAngle3D.Text = angle;
            ViewDist3D.Text = distance;
            Foreground3D.Text = foreground;
            Background3D.Text = background;

            PropertyChanged3D();
        }

        /// <summary>
        /// Set values in 3D panel and generate
        /// </summary>
        public void PropertyChanged3D()
        {
            SetWidthText();

            ConvertTextInput(ViewDist3D);
            ConvertTextInput(Background3D);
            ConvertTextInput(Foreground3D);

            if (UnitsDepth.SelectedItem != null)
            {
                if (UnitsDepth.SelectedItem.ToString() != "%")
                {
                    foreach (TextBox tb in LayerDepth.Children)
                    {
                        ConvertTextInput(tb);
                    }
                }
            }

            SetSpacingText();
            unitConvert = 1;
            Generate3D();
        }

        /// <summary>
        /// Action to generate shifts for 3D print
        /// </summary>
        public void Generate3D()
        {
            if (!ProjectHolder.ValidProject || timelineList.Count == 0 || !Panel3D.IsEnabled || !generate)
                return;

            //Input values
            double viewDist, viewAngle, foreground, background;

            if (Double.TryParse(ViewDist3D.Text, out viewDist) && Double.TryParse(ViewAngle3D.Text, out viewAngle) && Double.TryParse(Foreground3D.Text, out foreground) && Double.TryParse(Background3D.Text, out background))
            {
                //set new positions
                Generator3D.Generate3D(viewDist / unitToInches, viewAngle, ProjectHolder.ImageCount, ProjectHolder.Width, ProjectHolder.Dpi, timelineList);

                //repaint result
                RepaintCanvas();

                Warning3D.Content = "";

                //save to project holder
                ProjectHolder.ViewDistance = viewDist / unitToInches;
                ProjectHolder.ViewAngle = viewAngle;
                ProjectHolder.Foreground = foreground / unitToInches;
                ProjectHolder.Background = background / unitToInches;
            }
            else
                Warning3D.Content = LangProvider.getString("INVALID_3D_PARAMETERS");
        }

        /// <summary>
        /// Returns Layer instance from project holder by depth TextBox id
        /// </summary>
        /// <param name="depthBox">Depth TextBox</param>
        /// <returns>Layer if exists or null</returns>
        private Layer GetProjectLayer(TextBox depthBox)
        {
            Layer returnLayer = null;

            int layerID = LayerDepth.Children.IndexOf(depthBox);
            if (layerID == -1) //== -1 when it is not in LayerDepth.Children
                return null;
            //if layer exists return its instance
            if (layerID < ProjectHolder.Layers.Count)
                returnLayer = ProjectHolder.Layers[layerID];

            return returnLayer;
        }

        #endregion 3D methods

        #region 3D listeners
        /// <summary>
        /// Calculates frame spacing and enable 3D generate button if higher than 0.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View3D_Changed(object sender, TextChangedEventArgs e)
        {
            if (!ProjectHolder.ValidProject)
                return;

            if (ViewAngle3D.Text.Trim().Equals("") || ViewDist3D.Text.Trim().Equals(""))
            {
                FrameSpacing3D.Content = "";

                return;
            }

            SetSpacingText();
            Generate3D();

            checkFocus = true;
        }

        /// <summary>
        /// Fill combobox with length units enum values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Units3D_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            //values of length units enumerator
            var values = LengthUnits.GetValues(typeof(LengthUnits));

            cb.ItemsSource = values;
            cb.SelectedItem = LengthUnits.@in;
            units = LengthUnits.@in;
        }

        /// <summary>
        /// Change unit conversion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Units3D_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //convert
            if (Units3D.SelectedItem.Equals(LengthUnits.cm))
            {
                if (units == LengthUnits.mm)
                    unitConvert /= 100.0f;
                else
                    unitConvert = cmToInch;

                unitToInches = cmToInch;
                units = LengthUnits.cm;
            }
            else if (Units3D.SelectedItem.Equals(LengthUnits.@in))
            {
                if (units == LengthUnits.cm)
                    unitConvert = 1 / cmToInch;
                else
                    unitConvert = 1 / (cmToInch * 100);

                unitToInches = 1;
                units = LengthUnits.@in;
            }
            else
            {
                if (units == LengthUnits.cm)
                    unitConvert *= 100.0f;
                else
                    unitConvert = cmToInch * 100;

                unitToInches = cmToInch * 100.0f;
                units = LengthUnits.mm;
            }

            //update UnitsDepth ComboBox
            if (UnitsDepth.Items.Count > 0)
            {
                textChange = false;

                string selectedItem = UnitsDepth.SelectedItem.ToString();

                UnitsDepth.Items.RemoveAt(0);
                UnitsDepth.Items.Insert(0, Units3D.SelectedItem);

                //if selected != % change selected item to new units
                if (!selectedItem.Equals("%"))
                    UnitsDepth.SelectedItem = Units3D.SelectedItem;

                textChange = true;
            }

            PropertyChanged3D();

            checkFocus = true;
        }

        /// <summary>
        /// ComboBox Loaded method for UnitsDepth in Timeline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitsDepth_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            cb.Items.Add(Units3D.SelectedItem);
            cb.Items.Add("%");
            cb.SelectedItem = Units3D.SelectedItem;
        }

        /// <summary>
        /// Listener that controls layer depths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckLayerDepthInput(sender);
            checkFocus = true;
        }

        /// <summary>
        /// Controls layer depths if they're between foreground and background
        /// </summary>
        /// <param name="sender">text box</param>
        private void CheckLayerDepthInput(object sender)
        {
            double foreground;
            double background;

            if (UnitsDepth.SelectedItem == null || !Panel3D.IsEnabled)
                return;
            if (!(Double.TryParse(Foreground3D.Text, out foreground) && Double.TryParse(Background3D.Text, out background)))
                return;

            TextBox tb = sender as TextBox;

            //parse textbox and change background if is out of bounds
            double value;
            if (Double.TryParse(tb.Text, out value))
            {
                //if is set in %
                if (UnitsDepth.SelectedItem.Equals("%"))
                {
                    if (value > 0)
                        value = (value / 100.0) * foreground;
                    else if (value < 0)
                        value = (value / 100.0) * background * -1;
                }

                //must be between foreground and background
                if (value <= foreground && value >= background)
                {
                    //store value to layer object
                    Layer layer = GetProjectLayer(tb);
                    if (layer != null)
                    {
                        layer.Depth = value / unitToInches;
                        tb.Background = Brushes.White;
                    }
                }
                else
                    tb.Background = Brushes.Firebrick;
            }

            Generate3D();
        }

        /// <summary>
        /// Prepare new history item when got focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepthBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Layer layer = ProjectHolder.Layers[LayerDepth.Children.IndexOf((TextBox)sender)];
            historyItem = layer.GetHistoryItem();
        }

        /// <summary>
        /// Adds layer history when 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepthBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!checkFocus)
            {
                historyItem = null;
                return;
            }

            Layer layer = GetProjectLayer((TextBox)sender);
            if (layer != null)
            {
                ((LayerHistory)historyItem).DepthRedo = layer.Depth;
                ProjectHolder.HistoryList.AddHistoryItem(historyItem);
            }

            checkFocus = false;
        }

        /// <summary>
        /// If hase focus prepares new historyItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Param3D_GotFocus(object sender, RoutedEventArgs e)
        {
            historyItem = new ProjectHistory3D()
            {
                UndoAngle = ViewAngle3D.Text,
                UndoDistance = ViewDist3D.Text,
                UndoBackground = Background3D.Text,
                UndoForeground = Foreground3D.Text,
                UndoUnits = Units3D.SelectedItem.ToString()
            };
        }

        /// <summary>
        /// If needed and lost focus, store historyItem to history list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Param3D_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!checkFocus)
            {
                historyItem = null;
                return;
            }

            ProjectHistory3D history = historyItem as ProjectHistory3D;
            history.RedoAngle = ViewAngle3D.Text;
            history.RedoDistance = ViewDist3D.Text;
            history.RedoBackground = Background3D.Text;
            history.RedoForeground = Foreground3D.Text;
            history.RedoUnits = Units3D.SelectedItem.ToString();

            ProjectHolder.HistoryList.AddHistoryItem(history);

            checkFocus = false;
        }

        /// <summary>
        /// Check every depth layer text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepthBox_PropertyChanged(object sender, EventArgs e)
        {
            if (!Panel3D.IsEnabled)
                return;

            for (int i = 0; i < ProjectHolder.LayerCount; i++)
            {
                CheckLayerDepthInput((object)LayerDepth.Children[i]);
            }

            checkFocus = true;
        }

        /// <summary>
        /// Make conversion between in,cm,mm and %
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitsDepth_SelectionChanged(object sender, EventArgs e)
        {
            if (!Panel3D.IsEnabled || !textChange)
                return;

            double foreground;
            double background;
            if (!Double.TryParse(Foreground3D.Text, out foreground) || !Double.TryParse(Background3D.Text, out background))
                return;

            TextBox tb;
            for (int i = 0; i < ProjectHolder.LayerCount; i++)
            {
                tb = (TextBox)LayerDepth.Children[i];

                SetDepthText_SelectionChanged(tb, foreground, background);
            }
        }

        /// <summary>
        /// Convert value by selected units
        /// </summary>
        /// <param name="textBox">textbox</param>
        /// <param name="foreground">foreground value</param>
        /// <param name="background">background value</param>
        public void SetDepthText_SelectionChanged(TextBox textBox, double foreground, double background)
        {
            double value;
            double newValue = 0;

            if (Double.TryParse(textBox.Text, out value))
            {
                if (value == 0)
                    return;

                //conversion between % and length units
                if (UnitsDepth.SelectedItem.Equals("%"))
                {
                    if (value > 0)
                        newValue = (value / foreground) * 100;
                    else
                        newValue = (value / background) * -100;

                    if (newValue > 100.0) newValue = 100;
                    if (newValue < -100.0) newValue = -100;
                }
                else
                {
                    if (value > 0)
                        newValue = foreground * (value / 100);
                    else
                        newValue = background * (value / -100);
                }
            }

            newValue = Math.Round(newValue * 1000) / 1000;
            textBox.Text = newValue.ToString();
        }

        /// <summary>
        /// Disable / enable 3D tool panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TD_Checked(object sender, RoutedEventArgs e)
        {
            if (!ProjectHolder.ValidProject)
                return;

            if (Panel3D.IsEnabled)
            {
                Panel3D.IsEnabled = false;
                LayerDepth.IsEnabled = false;
                UnitsDepth.IsEnabled = false;

                Reset3DTranslation();
            }
            else
            {
                Panel3D.IsEnabled = true;
                LayerDepth.IsEnabled = true;
                UnitsDepth.IsEnabled = true;

                SetWidthText();
                SetDepthBounds();

                generate = true;
                PropertyChanged3D();
            }
        }

        /// <summary>
        /// Reset 3D translation of each object
        /// </summary>
        private void Reset3DTranslation()
        {
            foreach (var item in timelineList)
            {
                item.GetLayerObject().reset3DTranslation();
            }

            RepaintCanvas();
        }

        /// <summary>
        /// Anaglyph button listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Anaglyph_Click(object sender, RoutedEventArgs e)
        {
            ShowAnaglyph(false);
        }

        /// <summary>
        /// Anaglyph grayscale listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnaglyphGS_Click(object sender, RoutedEventArgs e)
        {
            ShowAnaglyph(true);
        }

        /// <summary>
        /// Creates and show window with grayscale or color anaglyph preview
        /// </summary>
        /// <param name="grayScale">grayscale if true else color</param>
        private void ShowAnaglyph(bool grayScale)
        {
            if (!ProjectHolder.ValidProject)
                return;

            if (canvasList.Count >= frameSpacing && frameSpacing > 0)
                new AnaglyphPreview(canvasList[0].GetCanvas(), canvasList[frameSpacing - 1].GetCanvas(), grayScale);
            else
                MessageBox.Show(LangProvider.getString("ANAGLYPH_ERROR"), LangProvider.getString("ANAGLYPH_ERROR_TITLE"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        #endregion 3D listeners
    }
}
