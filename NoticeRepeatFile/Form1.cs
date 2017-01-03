using Dapper;
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
        FileSystemWatcher _watchTorrent = new FileSystemWatcher();
        FileSystemWatcher _watchAvi = new FileSystemWatcher();

        public Form1()
        {
            InitializeComponent();
        }
        public class fileData
        {
            public string sourceName { get; set; }
            public string fileName { get; set; }
            public string location { get; set; }
            public string fileTime { get; set; }
        }
        #region 控制項事件
   
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
            //todo 設定欄位的文字方塊
            _watchTorrent.Path = @"J:\torrent";
            _watchTorrent.Filter = "*.torrent";
            _watchTorrent.EnableRaisingEvents = true;
            //觸發事件
            _watchTorrent.Created += new FileSystemEventHandler(watch_Created);
            
            _watchAvi.Path = @"J:\...";            
            _watchAvi.EnableRaisingEvents = true;
            _watchAvi.Created += new FileSystemEventHandler(folder_created);
            //指定使用的容器
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);           
            //建立NotifyIcon
            this.notifyIcon1.Icon = new Icon("alert.ico");
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Text = "監控中....";
            this.notifyIcon1.MouseDoubleClick += doubleClick;
        }
        private string[] filters = new string[] { ".avi", ".mp4" };
        private void folder_created(object sender, FileSystemEventArgs e)
        {
            if(filters.Contains(Path.GetExtension(e.FullPath)))
            {
                string fileName = getShortName(e.Name);


            }
            

        }

        private void doubleClick(object sender, MouseEventArgs e)
        {
            UpdateForm();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }
        
        private void watch_Created(object sender, FileSystemEventArgs e)
        {
            List<fileData> listFile = new List<fileData>();
            var dirInfo = new DirectoryInfo(e.FullPath.ToString());
            var fileName = dirInfo.Name;
            string txtKeyWord = fileName;
            string[] keyword = txtKeyWord.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in keyword)
            {
                string txtKey = name;
                if (name.IndexOf(".torrent") >= 0)
                    txtKey = name.Replace(".torrent", "");
                var result = searchKeyword(txtKey);
                if (result.Result.Count() > 0)
                {
                    var oneFile = result.Result.ToList();
                    listFile.AddRange(oneFile);
                }
                    
            }
            if(listFile.Count> 0)
            {                
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "警告！！有檔案重複。";
                notifyIcon1.BalloonTipTitle = "警告！！";
                notifyIcon1.ShowBalloonTip(5000);                
                UpdateForm();                
            }
                
            UpdateUI(listFile);
            //dg1.DataSource = listFile;

        }
        
        private delegate void UpdateFormCallBack();
        private void UpdateForm()
        {
            if (this.InvokeRequired)
            {
                UpdateFormCallBack uu = new UpdateFormCallBack(UpdateForm);
                this.Invoke(uu);
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }
        private delegate void UpdateUICallBack(List<fileData> listFile);
        private void UpdateUI(List<fileData>  listFile)
        {
            if (this.InvokeRequired)
            {
                UpdateUICallBack uu = new UpdateUICallBack(UpdateUI);
                this.Invoke(uu,listFile);
            }
            else
            {
                dg1.DataSource = listFile;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            replaceFile();
            //testRegex();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string txtKeyWord = txtKey.Text;
            //string[] keyword = txtKeyWord.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var a in keyword)
            //{
            //listMsg.Items.Add(a);
            string a = txtKeyword.Text;
            var result = searchKeyword(a);
            if (result.Result.Count() > 0)
                dg1.DataSource = result.Result.ToList();

            //}
        }
        #endregion
        public void replaceFile()
        {
            List<fileData> listFile = new List<fileData>();
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = System.IO.Path.Combine(openFolderDialog.SelectedPath);
                txtPath.Text = folderPath;
                DirectoryInfo di = new DirectoryInfo(folderPath);
                //copy file
                searchFile("*.avi",di,ref listFile);
                searchFile("*.mp4",di,ref listFile);                
            }
            dg1.DataSource = listFile;
            insertData(listFile);
        }
        /// <summary>
        /// 擷取檔案列表
        /// </summary>
        /// <param name="extendName">副檔名</param>
        /// <param name="di">目錄結構</param>
        /// <param name="listFile">資料集合</param>
        private void searchFile(string extendName, DirectoryInfo di, ref List<fileData> listFile)
        {
            foreach (var fi in di.GetFiles(extendName, SearchOption.AllDirectories))
            {
                fileData fd = new fileData()
                {
                    sourceName = fi.Name,
                    fileName = getShortName(fi.Name),
                    fileTime = fi.CreationTime.ToString(),
                    location = fi.FullName
                };
                listFile.Add(fd);
                //msg(fi.FullName);
                //msg(fi.Name);
            }
        }
        /// <summary>
        /// 取得短名字
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string getShortName(string fileName)
        {
            string input = fileName;
            if (input.IndexOf("720p") >= 0 || input.IndexOf("1080p") >= 0)
                return input;
            string urlPattern = @"^.+(.net|.com|.cc|.co)[^a-zA-Z0-9]?"; //網址
            string pattern = @"[a-zA-Z]{2,}-{0,}\d{2,}";
                // 先判斷是否有網址?網址取代:繼續
            Regex urlrgx = new Regex(urlPattern, RegexOptions.IgnoreCase);
            MatchCollection urlmatches = urlrgx.Matches(fileName);
            if (urlmatches.Count > 0)
            {
                foreach (Match match in urlmatches)
                    input = fileName.Replace(match.Value, "");
            }

            //取檔案名
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(input);
            if (matches.Count > 0)
            {                    
                foreach (Match match in matches)                    
                    return match.Value.Replace("-","");                    
            }   
            return input;
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
                        VALUES (null,'{0}','{1}','{2}','{3}','{4}');",row.sourceName,row.fileName,row.location,row.fileTime,DateTime.Now);
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
       

        private async Task<IEnumerable<fileData>> searchKeyword(string keyword)
        {
            keyword = "%" + keyword + "%";

            using (var conn = new SQLiteConnection("Data source = fileLibary.db"))
            {
                conn.Open();
                var result = await conn.QueryAsync<fileData>(@"select * from movie where fileName like @keyword;", new { keyword });
                return result;
            }
        }
        #region 測試
        /// <summary>
        /// 文字檔測試用
        /// </summary>
        public void testRegex()
        {
            List<fileData> listFile = new List<fileData>();

            OpenFileDialog openFileDg = new OpenFileDialog();
            if (openFileDg.ShowDialog() == DialogResult.OK)
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
                    if (urlmatches.Count > 0)
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
                            fileData fd = new fileData()
                            {
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
            insertData(listFile);


        }




        #endregion

 
    }
}
