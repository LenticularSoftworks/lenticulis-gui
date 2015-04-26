using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace lenticulis_gui.src.App
{
    public class LangConverter : MarkupExtension
    {
        [ConstructorArgument("str")]
        public object str { get; set; }

        public LangConverter()
        {
            //
        }

        public LangConverter(object str)
        {
            this.str = str;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (str == null)
                return "";

            String val;

            // Just for WPF designer complains; the WPF designer runs this code at design time, so it's logical, that
            // we don't have our language strings loaded. In this case, just return the input parameter
            try { val = LangProvider.getString(str.ToString()).ToString(); }
            catch (Exception) { val = str.ToString(); }

            return val.ToString();
        }
    }
}
