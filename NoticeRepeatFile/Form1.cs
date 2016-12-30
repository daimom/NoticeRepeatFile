using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoticeRepeatFile
{
    public partial class Form1 : Form
    {
        private SQLiteConnection sqlite_connect;
        private SQLiteCommand sqlite_cmd;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Application.StartupPath + @"\fileLibary.db"))
            {
                SQLiteConnection.CreateFile("fileLibary.db");
                sqlite_connect = new SQLiteConnection("Data source = fileLibary.db");
                //建立資料庫連線

                sqlite_connect.Open();// Open                
                sqlite_cmd = sqlite_connect.CreateCommand();//create command

                sqlite_cmd.CommandText = @"CREATE TABLE IF NOT EXISTS movie 
            (sn INTEGER PRIMARY KEY AUTOINCREMENT,sourceName Text, fileName TEXT,location TEXT,fileTime text ,createTime Text)";
                //create table header
                //INTEGER PRIMARY KEY AUTOINCREMENT=>auto increase index
                sqlite_cmd.ExecuteNonQuery(); //using behind every write cmd
            }
        }
        public class fileData
        {
            public string sourceName { get; set; }
            public string fileName { get; set; }
            public string location { get; set; }
            public string fileTime { get; set; }
        }
        public void replaceFile()
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = System.IO.Path.Combine(openFolderDialog.SelectedPath);
                txtPath.Text = folderPath;
                DirectoryInfo di = new DirectoryInfo(folderPath);
                //copy file
                foreach (var fi in di.GetFiles("*.txt", SearchOption.AllDirectories))
                {
                    msg(fi.FullName);
                    msg(fi.Name);
                }
            }
            
        }
        public void testRegex()
        {
            List<fileData> listFile = new List<fileData>();

            OpenFileDialog openFileDg = new OpenFileDialog();
            if(openFileDg.ShowDialog() == DialogResult.OK)
            {
                var fileName = Path.Combine(openFileDg.FileName);
                StreamReader sr = new StreamReader(fileName);
                string input;
                string urlPattern = @"^.+(.net|.com|.cc|.co)[^a-zA-Z0-9]?"; //網址
                string pattern = @"[a-zA-Z]{2,}\d{3,}";
                while (sr.Peek() >= 0)
                {
                    input = sr.ReadLine();
                    // 先判斷是否有網址?網址取代:繼續
                    Regex urlrgx = new Regex(urlPattern, RegexOptions.IgnoreCase);
                    MatchCollection urlmatches = urlrgx.Matches(input);
                    if(urlmatches.Count> 0)
                    {
                        foreach (Match match in urlmatches)
                            input = input.Replace(match.Value, "");
                    }   

                    //取檔案名
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(input);
                    if (matches.Count > 0)
                    {
                        //Console.WriteLine("{0} ({1} matches):", input, matches.Count);
                        foreach (Match match in matches)
                        {
                            fileData fd = new fileData() {
                                sourceName = input,
                                fileName = match.Value,
                                fileTime = "2016/12/30",
                                location = fileName
                            };
                            listFile.Add(fd);                         
                        }
                    }
                    else
                    {                        
                        fileData fd = new fileData()
                        {
                            sourceName = input,
                            fileName = input,
                            fileTime = "2016/12/30",
                            location = fileName
                        };
                        listFile.Add(fd);
                    }

                }
                sr.Close();
            }
            dg1.DataSource = listFile;
            
        }
        private void insertData(List<fileData> fd)
        {
            using (sqlite_connect = new SQLiteConnection("Data source = fileLibary.db"))
            {
                //建立資料庫連線                
                sqlite_connect.Open();// Open
                sqlite_cmd = sqlite_connect.CreateCommand();
                SQLiteTransaction sqlite_trans = sqlite_connect.BeginTransaction();
                foreach(var row in fd)
                {
                    sqlite_cmd.CommandText = string.Format(@"INSERT INTO movie 
                        VALUES (null,'{0}','{1}','{2}','{3}','{4}');",row.sourceName,row.fileName,row.location,row.fileName,DateTime.Now);
                    sqlite_cmd.ExecuteNonQuery();
                }
                sqlite_trans.Commit();
            }
        }
        private void msg(string message)
        {
            txtMsg.Text += Environment.NewLine + message;
            txtMsg.SelectionStart = txtMsg.Text.Length;
            txtMsg.ScrollToCaret();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //replaceFile();
            testRegex();
        }
    }
}
