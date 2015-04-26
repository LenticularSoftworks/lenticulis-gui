using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace lenticulis_gui.src.App
{
    public static class LangProvider
    {
        /// <summary>
        /// Default language used at first start
        /// </summary>
        public const String DEFAULT_LANG = "cs";

        /// <summary>
        /// Current language is now determined 
        /// </summary>
        public static String CurrentLang { get; private set; }

        /// <summary>
        /// Current language dictionary
        /// </summary>
        private static Dictionary<String, String> CurrentLangStrings = null;

        /// <summary>
        /// Dictionary of available languages; Key = identifier, Value = [filename, lang name]
        /// </summary>
        private static Dictionary<String, KeyValuePair<String, String>> AvailableLangs = new Dictionary<String, KeyValuePair<String, String>>();

        /// <summary>
        /// Initializes language provider with lang to use
        /// </summary>
        /// <param name="useLang">language to be used</param>
        /// <returns>can application be launched?</returns>
        public static bool Initialize(String useLang = null)
        {
            // language directory does not exist, we can't continue
            if (!Directory.Exists(@"lang\"))
            {
                MessageBox.Show("Could not find directory 'lang' within application directory, the program could not be loaded!", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // get all *.lng files in lang directory
            String[] files = Directory.GetFiles(@"lang\", "*.lng");

            foreach (String file in files)
            {
                StreamReader f = new StreamReader(file);
                // read metadata from all files
                String metadata = f.ReadLine();

                // it has to start with /
                if (!metadata.StartsWith("/"))
                {
                    MessageBox.Show("File "+file+" is missing metadata string on first line.", "Missing metadata in language file", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    continue;
                }

                // 0 = language identifier, 1 = language name
                String[] metaParts = metadata.Substring(1).Split(';');

                if (metaParts.Length < 2)
                {
                    MessageBox.Show("File " + file + " contains wrong metadata string.", "Corrupted metadata in language file", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    continue;
                }

                // add to available languages dictionary
                AvailableLangs.Add(metaParts[0], new KeyValuePair<String,String>(file, metaParts[1]));
            }

            // use supplied language, unless it's null - then use default lang
            if (useLang == null)
                CurrentLang = DEFAULT_LANG;
            else
                CurrentLang = useLang;

            // no default lang file, we can't continue
            if (!AvailableLangs.ContainsKey(CurrentLang))
            {
                MessageBox.Show("Default language '" + DEFAULT_LANG + "' could not be found. Application cannot be launched", "Language file not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // use language that has been chosen or fallen back onto
            UseLanguage(CurrentLang);

            return true;
        }

        /// <summary>
        /// Retrieves dictionary of available languages in identifier-name format
        /// </summary>
        /// <returns>Available languages</returns>
        public static Dictionary<String, String> GetAvailableLangs()
        {
            Dictionary<String, String> langs = new Dictionary<String, String>();

            // fetch internal data into reduced dictionary
            foreach (KeyValuePair<String, KeyValuePair<String, String>> kvp in AvailableLangs)
                langs.Add(kvp.Key, kvp.Value.Value);

            return langs;
        }

        /// <summary>
        /// Loads language from file and refreshes internal dictionary
        /// </summary>
        /// <param name="lang"></param>
        public static void UseLanguage(String lang)
        {
            CurrentLang = lang;

            // if no such language exists, fall back to default
            if (!AvailableLangs.ContainsKey(CurrentLang))
            {
                MessageBox.Show("Chosen language file could not be found, falling back to default language: "+DEFAULT_LANG, "Language file not found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CurrentLang = DEFAULT_LANG;
            }

            String filename = AvailableLangs[CurrentLang].Key;

            CurrentLangStrings = new Dictionary<String, String>();

            // read all translations
            StreamReader f = new StreamReader(filename);
            String line;
            while (!f.EndOfStream)
            {
                line = f.ReadLine();

                // ignore comments
                if (line.StartsWith("#"))
                    continue;

                String[] str = line.Split('=');
                if (str.Length < 2)
                    continue;

                CurrentLangStrings.Add(str[0], line.Substring(str[0].Length + 1));
            }
        }

        /// <summary>
        /// Retrieves translation string if available
        /// </summary>
        /// <param name="input">input translation string</param>
        /// <returns>translated string</returns>
        public static String getString(String input)
        {
            if (CurrentLangStrings.ContainsKey(input))
                return CurrentLangStrings[input];
            return input;
        }
    }
}
