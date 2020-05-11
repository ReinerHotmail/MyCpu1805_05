using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace MyCpu1805_05
{
    public partial class MainWindow
    {

        // Long: MyDocument\\MyCpu1805\\RcaFile\\Prg01.rca
        // Path: MyDocument\\MyCpu1805\\RcaFile
        // Name: Prg01.rca
        (string Long, string Path, string Name) RcaFile;
       


        private void IniMyFolder()
        {

            if (!File.Exists("SettingFile.txt"))
            {

                // SettingFile.Txt erzeugen im DEBUG oder RELEASE Pfad der EXE
                using (StreamWriter stream = File.AppendText("SettingFile.txt"))
                {
                    stream.WriteLine("key" + ";" + "content");
                }
            }


      
            RcaFile.Path = GetSetting("PathRca");



            bool dirSettingEmty =  RcaFile.Path == "" ;

            bool dirErr = false;


            if (RcaFile.Path != "" && !Directory.Exists(RcaFile.Path))
                dirErr = true;

            if (dirSettingEmty || dirErr)
            {
                MessageBoxResult result =
                MessageBox.Show("Standard-Ordner für RCA-Files \nim Dokumenten-Pfad erstellen ?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RcaFile.Path = FolderBrowserDialog("RcaFile-Directory");

                    if (RcaFile.Path=="")
                        Application.Current.Shutdown();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }

            PutSetting("PathRca", RcaFile.Path);

            ShowFileName(RcaFile.Path);
       
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
