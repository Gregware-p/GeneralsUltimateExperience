using System;
using System.IO;

namespace GeneralsUltimateExperience
{
    public static class IniHelper
    {
        #region structs

        public struct OptionIni
        {
            public int ResolutionNoLigne;
            public int ResolutionX;
            public int ResolutionY;
            public string[] Lignes;
        }
        #endregion

        public static void LineChanger(string newText, string path, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(path);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(path, arrLine);
        }

        public static OptionIni GetOptionIni(string path)
        {
            OptionIni optionIni = new OptionIni();
            optionIni.Lignes = File.ReadAllLines(path);
            optionIni.ResolutionNoLigne = Array.FindIndex(optionIni.Lignes, s => s.TrimStart().StartsWith("Resolution"));
            optionIni.ResolutionX = Int32.Parse(optionIni.Lignes[optionIni.ResolutionNoLigne].Split('=')[1].Trim().Split(' ')[0]);
            optionIni.ResolutionY = Int32.Parse(optionIni.Lignes[optionIni.ResolutionNoLigne].Split('=')[1].Trim().Split(' ')[1]);
            return optionIni;
        }
    }
}