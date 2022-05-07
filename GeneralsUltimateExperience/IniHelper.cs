using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralsUltimateExperience
{
    public static class IniHelper
    {
        #region structs
        public struct GameDataIni
        {
            public int NoLigneCameraHeight;
            public int NoLigneMaxCameraHeight;
            public int NoLigneMinCameraHeight;
            public int NoLigneCameraAdjustSpeed;
            public int NoLigneDrawEntireTerrain;
            public int NoLigneWindowed;
            public string[] Lignes;
        }

        public struct OptionIni
        {
            public int ResolutionNoLigne;
            public int ResolutionX;
            public int ResolutionY;
            public int ScrollFactor;
            public int ScrollFactorNoLigne;
            public string[] Lignes;
        }
        #endregion

        public static void SetGameDataIniZoomValuesFromOtherGameDataIni(string sourcePath, string targetPath)
        {
            int maxCameraHeight;
            double cameraAdjustSpeed;
            bool drawEntireTerrain;

            GameDataIni gameDataIni = GetGameDataIni(sourcePath);
            maxCameraHeight = Int32.Parse(gameDataIni.Lignes[gameDataIni.NoLigneMaxCameraHeight].Split('=')[1].Split('.')[0].Trim());
            cameraAdjustSpeed = Double.Parse(gameDataIni.Lignes[gameDataIni.NoLigneCameraAdjustSpeed].Split('=')[1].Trim(), CultureInfo.InvariantCulture);
            drawEntireTerrain = gameDataIni.Lignes[gameDataIni.NoLigneDrawEntireTerrain].Split('=')[1].Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase);
            SetGameDataIniZoomValues(targetPath, maxCameraHeight, cameraAdjustSpeed, drawEntireTerrain);
        }

        public static void SetGameDataIniZoomValues(string path, int maxCameraHeight, double cameraAdjustSpeed, bool drawEntireTerrain)
        {
            GameDataIni gameDataIni = GetGameDataIni(path);
            LineChanger(string.Format("  MaxCameraHeight = {0}", maxCameraHeight), path, gameDataIni.NoLigneMaxCameraHeight);
            LineChanger(string.Format("  CameraAdjustSpeed = {0}", cameraAdjustSpeed.ToString(CultureInfo.InvariantCulture)), path, gameDataIni.NoLigneCameraAdjustSpeed);
            LineChanger(string.Format("  DrawEntireTerrain = {0}", drawEntireTerrain ? "Yes" : "No"), path, gameDataIni.NoLigneDrawEntireTerrain);
        }

        public static void SetGameDataIniWindowed(string path, bool windowed)
        {
            GameDataIni gameDataIni = GetGameDataIni(path);
            LineChanger(string.Format("  Windowed = {0}", windowed ? "Yes" : "No"), path, gameDataIni.NoLigneWindowed);
        }

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
            optionIni.ScrollFactorNoLigne = Array.FindIndex(optionIni.Lignes, s => s.TrimStart().StartsWith("ScrollFactor"));
            optionIni.ScrollFactor = Int32.Parse(optionIni.Lignes[optionIni.ScrollFactorNoLigne].Split('=')[1].Trim());
            return optionIni;
        }

        private static GameDataIni GetGameDataIni(string path)
        {
            GameDataIni gameDataIni = new GameDataIni();
            string[] linesIni = File.ReadAllLines(path);
            gameDataIni.NoLigneCameraHeight = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("CameraHeight "));
            gameDataIni.NoLigneMaxCameraHeight = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("MaxCameraHeight "));
            gameDataIni.NoLigneMinCameraHeight = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("MinCameraHeight "));
            gameDataIni.NoLigneCameraAdjustSpeed = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("CameraAdjustSpeed "));
            gameDataIni.NoLigneDrawEntireTerrain = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("DrawEntireTerrain "));
            gameDataIni.NoLigneWindowed = Array.FindIndex(linesIni, s => s.TrimStart().StartsWith("Windowed "));
            gameDataIni.Lignes = linesIni;
            return gameDataIni;
        }
    }
}