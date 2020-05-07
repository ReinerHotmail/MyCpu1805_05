using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {
        string Path0 = "";
        (string Long, string Path, string Name) RcaFile;
        string PathLog = "";


        private void IniMyFolder()
        {

            if (!File.Exists("SettingFile.txt"))
            {

                // SettingFile.Txt erzeugen
                using (StreamWriter stream = File.AppendText("SettingFile.txt"))
                {
                    stream.WriteLine("key" + ";" + "content");
                }
            }


            Path0 = GetSetting("Path0");
            PathLog = GetSetting("PathLog");
            RcaFile.Path = GetSetting("PathRca");



            bool dirSettingEmty = Path0 == "" || PathLog == "" || RcaFile.Path == "";

            bool dirErr = false;

            if (Path0 != "" && !Directory.Exists(Path0))
                dirErr = true;

            if (PathLog != "" && !Directory.Exists(PathLog))
                dirErr = true;

            if (RcaFile.Path != "" && !Directory.Exists(RcaFile.Path))
                dirErr = true;

            if (dirSettingEmty || dirErr)
            {
                MessageBoxResult result =
                MessageBox.Show("Standard-Ordner im Dokumenten-Pfad erstellen ?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Path0 = docPath + "\\MyCpu1805";
                    PathLog = docPath + "\\MyCpu1805" + "\\Log";
                    RcaFile.Path = docPath + "\\MyCpu1805" + "\\RcaFile";

                    if (!Directory.Exists(Path0))
                        Directory.CreateDirectory(Path0);
                    if (!Directory.Exists(PathLog))
                        Directory.CreateDirectory(PathLog);
                    if (!Directory.Exists(RcaFile.Path))
                        Directory.CreateDirectory(RcaFile.Path);
                }
                else
                {
                    do
                    {
                        MessageBox.Show("Standard-Ordner  wählen oder neu erstellen");
                        Path0 = FolderBrowserDialog("CPU1805-Directory");
                    } while (Path0 == "");


                    do
                    {
                        MessageBox.Show("LogDatei-Ordner  wählen oder neu erstellen");
                        PathLog = FolderBrowserDialog("Log-Directory");
                    } while (PathLog == "");

                    do
                    {
                        MessageBox.Show("RcaFile-Ordner  wählen oder neu erstellen");
                        RcaFile.Path = FolderBrowserDialog("RcaFile-Directory");
                    } while (RcaFile.Path == "");
                }
            }

            PutSetting("Path0", Path0);
            PutSetting("PathLog", PathLog);
            PutSetting("PathRca", RcaFile.Path);


        }


        /// <summary>
        /// Setting Parameter lesen
        /// </summary>
        /// <param name="key">Schlüssel des Setting-Parameters</param>
        /// <returns>Wert des Parameters</returns>
        private string GetSetting(string key)
        {


            // Lesen der Setting Datei
            string[] lines = System.IO.File.ReadAllLines("SettingFile.txt");

            // Setting-Daten in Liste packen
            List<(string key, string content)> list = new List<(string key, string content)>();

            foreach (string item in lines)
            {
                string[] itemSplit = item.Split(';');
                list.Add((itemSplit[0], itemSplit[1]));
            }

            // Setting-Liste durchsuchen nach 'key'
            (string k, string content) found = list.Find(x => x.key == key);


            // RETURN  vom Ergebnis der Suche
            if (found == (null, null))
                return "";
            else
                return found.content;

        }

        /// <summary>
        /// Setting Parameter schreiben
        /// </summary>
        /// <param name="key">Schlüssel des Setting-Parameters</param>
        /// <param name="content">Wert des Setting-Parameters</param>
        /// <returns></returns>
        private bool PutSetting(string key, string content)
        {

            // Lesen der Setting Datei
            string[] lines = System.IO.File.ReadAllLines("SettingFile.txt");

            // Setting-Daten in Liste packen
            List<(string key, string content)> list = new List<(string key, string content)>();

            foreach (string item in lines)
            {
                string[] itemSplit = item.Split(';');
                list.Add((itemSplit[0], itemSplit[1]));
            }

            // Setting-Liste durchsuchen nach 'key'
            (string key, string content) found = list.Find(x => x.key == key);


            // Überschreiben,  falls Key vorhanden  oder hinzufügen, falls key nicht vorhanden
            if (found != (null, null))
                list.Remove(found);

            list.Add((key, content));


            // Liste zum Schreiben in Array wandeln
            string[] sArr = new string[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                sArr[i] = list[i].key + ";" + list[i].content;
            }



            File.WriteAllLines("SettingFile.txt", sArr);



            return true;


        }


        /// <summary>
        /// FolderBrowser Dialog
        /// </summary>
        /// <param name="title"></param>
        /// <returns>selected Folder</returns>
        private string FolderBrowserDialog(string title)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = title;
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;


            }
            else
            {
                return "";
            }
        }
    }
}
