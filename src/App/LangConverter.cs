using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace lenticulis_gui.src.App
{
    public class LangDataSource : INotifyPropertyChanged
    {
        /// <summary>
        /// Instance list to be able to dynamically update values on language change
        /// </summary>
        private static List<LangDataSource> instances = new List<LangDataSource>();

        /// <summary>
        /// String to be translated
        /// </summary>
        private String stringName;
        /// <summary>
        /// Property changed event set by control itself, it just have to be public
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property the control will try to retrieve
        /// </summary>
        public object Value
        {
            /// we retrieve translated value from LangProvider
            get
            {
                String val;

                // Just for WPF designer complains; the WPF designer runs this code at design time, so it's logical, that
                // we don't have our language strings loaded. In this case, just return the input parameter
                try { val = LangProvider.getString(stringName.ToString()).ToString(); }
                catch (Exception) { val = stringName.ToString(); }

                return val.ToString();
            }
            set
            {
                // empty, we don't need to set anything
            }
        }

        /// <summary>
        /// Creates new instance with string to be translated as parameter
        /// </summary>
        /// <param name="val">string to be translated</param>
        public LangDataSource(String val)
        {
            stringName = val;
            // add to static list
            instances.Add(this);
        }

        /// <summary>
        /// In destructor, we just remove instance from list, to not update that reference anymore
        /// </summary>
        ~LangDataSource()
        {
            instances.Remove(this);
        }

        /// <summary>
        /// Notify method to send property changed event to control; this will refresh its bound value
        /// </summary>
        /// <param name="propertyName"></param>
        public void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Static method to update all existing bindings to this class instances
        /// </summary>
        public static void UpdateDataSources()
        {
            foreach (LangDataSource ds in instances)
                ds.NotifyPropertyChanged("Value");
        }
    }

    public class LangConverter : MarkupExtension
    {
        [ConstructorArgument("str")]
        public object str { get; set; }

        /// <summary>
        /// Empty constructor should be there, as the parent declares to implement it
        /// </summary>
        public LangConverter()
        {
            //
        }

        /// <summary>
        /// Primary constructor, initialized with string to be translated
        /// </summary>
        /// <param name="str"></param>
        public LangConverter(object str)
        {
            this.str = str;
        }

        /// <summary>
        /// Main method to provide translation (binding to translation data source)
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns>binding to translation data source</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (str == null)
                return "";

            var binding = new Binding("Value")
            {
                Source = new LangDataSource(str.ToString())
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
