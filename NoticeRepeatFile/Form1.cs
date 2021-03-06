﻿using Dapper;
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
           
            //指定使用的容器
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);           
            //建立NotifyIcon
            this.notifyIcon1.Icon = new Icon("alert.ico");
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Text = "監控中....";
            this.notifyIcon1.MouseDoubleClick += doubleClick;
            if (!txtPath.Text.Contains("Please Select"))
                TorrentWatch(txtPath.Text);
            if (!txtAvi.Text.Contains("Please Select"))
                AviWatch(txtAvi.Text);
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
        /// <summary>
        /// 監視剪貼簿
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {            
            if (Clipboard.ContainsText() || txtKeyword.Text != (String)Clipboard.GetText())
            {
                string ClipboardText = (String)Clipboard.GetText();

                if (string.IsNullOrEmpty(ClipboardText) == false &&
                    string.IsNullOrWhiteSpace(ClipboardText) == false)
                {
                    if (ClipboardText.IndexOf("-") >= 0 || ClipboardText.Length >= 6)
                        txtKeyword.Text = ClipboardText;
                }
                //Clipboard.Clear();
            }
        }
        /// <summary>
        /// 即時監控torrent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watch_Created(object sender, FileSystemEventArgs e)
        {
            List<fileData> listFile = new List<fileData>();
            var dirInfo = new DirectoryInfo(e.FullPath.ToString());
            var fileName = dirInfo.Name;
            var fileTime = dirInfo.CreationTime;
            string repeatFile = "";
            string[] keyword = fileName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in keyword)
            {
                string txtKey = name;
                if (name.IndexOf(".torrent") >= 0)
                    txtKey = name.Replace(".torrent", "").Replace("-", "");
                if (name.IndexOf("-")>=0)
                    txtKey = name.Replace("-", "");
                //if (name.IndexOf(".") >= 0)
                //    txtKey = name.Replace(".", "").Replace("-","");
                var result = searchKeyword(txtKey); //檢查資料庫
                if (result.Result.Count() > 0)
                {
                    var oneFile = result.Result.ToList();
                    listFile.AddRange(oneFile);
                    repeatFile += txtKey + ",";
                }
                else
                {
                    var resultSource = searchSourceKeyword(txtKey);
                    if (resultSource.Result.Count() > 0)
                    {
                        listFile.Add(new fileData
                        {
                            sourceName = "====SourceName====",
                            fileName = "=====SourceName======="
                        });
                        var oneFile = resultSource.Result.ToList();
                        listFile.AddRange(oneFile);
                    }
                }
                var torrentResult = getTorrentFile(txtKey,fileTime,fileName);   //檢查種子
                if (torrentResult.Count > 0 )
                {
                    repeatFile += txtKey + ",";
                    foreach (var files in torrentResult)
                    {
                        listFile.Add(new fileData
                        {
                            sourceName = txtPath.Text,
                            fileName = files
                        });                        
                    }                                        
                }
            }
            if(listFile.Count> 0)
            {
 

                //notifyIcon1.Visible = true;
                //notifyIcon1.BalloonTipText = "警告！！有檔案重複。有檔案重複。有檔案重複。有檔案重複。有檔案重複。";
                //notifyIcon1.BalloonTipTitle = "警告！！";
                //notifyIcon1.ShowBalloonTip(5000);                
                UpdateForm();
                UpdateUI(listFile);
                UpdateMsg("警告！！種子 " + repeatFile + "  重複！！！ 時間：" + DateTime.Now.ToString());
            }
                
            
            //dg1.DataSource = listFile;

        }
        private string[] filters = new string[] { ".avi", ".mp4" };
        private string lastFile;
        /// <summary>
        /// 即時監控avi資料夾，新增至資料庫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void folder_created(object sender, FileSystemEventArgs e)
        {
            //先排除掉.!ut
            //移除特定資料夾檔案
            if (e.FullPath.Contains(".unwanted"))
                return;
            if (filters.Contains(Path.GetExtension(Path.GetFileNameWithoutExtension(e.FullPath))))
            {
                List<fileData> listFile = new List<fileData>();
                string fullName = e.Name.Replace(".!ut", "").Replace(".!qB","");               ///先排除!ut
                fileData fd = new fileData()
                {
                    sourceName = fullName,
                    fileName = getShortName(fullName),
                    fileTime = DateTime.Now.ToString(),
                    location = e.FullPath
                };
                listFile.Add(fd);
                
                var result = searchKeyword(fd.fileName); //檢查資料庫
                if (result.Result.Count() > 0)
                {
                    //notifyIcon1.Visible = true;
                    //notifyIcon1.BalloonTipText = "警告！！有檔案重複。有檔案重複。有檔案重複。有檔案重複。有檔案重複。";
                    //notifyIcon1.BalloonTipTitle = "警告！！";
                    //notifyIcon1.ShowBalloonTip(5000);
                    UpdateForm();
                    UpdateUI(listFile);
                    UpdateMsg(string.Format("警告！！檔案重複 {0} ，時間：{1}", fd.fileName, DateTime.Now.ToString()));
                }
                if(lastFile != fd.fileName)
                {
                    lastFile = fd.fileName;
                    insertData(listFile);
                }

                UpdateMsg(string.Format("已插入一筆資料：{0}",fd.fileName));

                
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            if (chkTimer.Checked == true)
                ClipboardTimer.Enabled = true;
            else
                ClipboardTimer.Enabled = false;

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
                this.Activate();
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
            using (var conn = new SQLiteConnection("Data source = fileLibary.db"))
            {
                conn.Open();
                var result = conn.Query<int>(@"select count(*) from movie;");
                if (result.First<int>()>0)
                {
                    DialogResult Result = MessageBox.Show("資料庫內已有資料，是否確定匯入", "警告", MessageBoxButtons.YesNo);
                    if (Result == System.Windows.Forms.DialogResult.No)
                    {
                        return;
                    }
                }
            }
            replaceFile();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string txtKeyWord = txtKey.Text;
            //string[] keyword = txtKeyWord.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var a in keyword)
            //{
            //listMsg.Items.Add(a);
            string a = txtKeyword.Text.Replace("-","").Trim();
            var result = searchKeyword(a);
            var count = result.Result.Count();
            if (count > 0)
            {
                dg1.DataSource = result.Result.OrderBy(p => p.fileName).ToList();
                msg(string.Format("查詢：{0}筆",count));
            }
            else
            {
                //檢查關鍵字有沒有在種子內
                List<fileData> listFile = new List<fileData>();

                var folderPath = txtPath.Text;
                DirectoryInfo di = new DirectoryInfo(folderPath);
                var fileTime = di.CreationTime; 
                var torrentResult = getTorrentFile(a, fileTime);   //檢查種子
                if (torrentResult.Count > 0)
                {
                    foreach (var files in torrentResult)
                    {
                        listFile.Add(new fileData
                        {
                            sourceName = txtPath.Text,
                            fileName = files
                        });
                    }
                }
                
                if (listFile.Count > 0)
                {
                    UpdateUI(listFile);
                    msg(string.Format("查詢：{0}筆", listFile.Count()));
                }
                else
                    msg(string.Format("查無資料--{0}", DateTime.Now.ToString()));
            }
                
            //}
            
        }
        /// <summary>
        /// 監控torrent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = System.IO.Path.Combine(openFolderDialog.SelectedPath);
                TorrentWatch(folderPath);
                txtPath.Text = folderPath;                
            }
        }
        /// <summary>
        /// 監控影片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = System.IO.Path.Combine(openFolderDialog.SelectedPath);
                AviWatch(folderPath);
                txtAvi.Text = folderPath;
            }
        }

        //刪除重複的資料
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            using (sqlite_connect = new SQLiteConnection("Data source = fileLibary.db"))
            {
                //建立資料庫連線                
                sqlite_connect.Open();// Open
                sqlite_cmd = sqlite_connect.CreateCommand();
                SQLiteTransaction sqlite_trans = sqlite_connect.BeginTransaction();
                sqlite_cmd.CommandText = @"delete from movie where sn not in (select min(sn)
                                    from movie
                                    where fileName in (SELECT distinct filename FROM movie)
                                    group by filename)";
                sqlite_cmd.ExecuteNonQuery();
                sqlite_trans.Commit();
                MessageBox.Show("刪除完成");
            }
        }

        #endregion
        public void TorrentWatch(string folderPath)
        {
            _watchTorrent.Path = folderPath;
            _watchTorrent.Filter = "*.torrent";
            _watchTorrent.EnableRaisingEvents = true;
            _watchTorrent.IncludeSubdirectories = true;
            _watchTorrent.Created += new FileSystemEventHandler(watch_Created);
            msg("torrent資料夾監視開始..");
        }
        public void AviWatch(string folderPath)
        {
            _watchAvi.Path = folderPath;
            _watchAvi.Filter = "*.*";
            _watchAvi.EnableRaisingEvents = true;
            _watchAvi.IncludeSubdirectories = true;
            _watchAvi.Created += new FileSystemEventHandler(folder_created);
            msg("Avi資料夾監視開始..");
        }
        public void replaceFile()
        {
            List<fileData> listFile = new List<fileData>();            
            var folderPath = txtAvi.Text;
            txtAvi.Text = folderPath;
            DirectoryInfo di = new DirectoryInfo(folderPath);
            //copy file
            msg("建立中....");
            System.Threading.Thread.Sleep(300);
            Application.DoEvents();
            searchFile("*.avi", di, ref listFile);
            searchFile("*.mp4", di, ref listFile);
            searchFile("*.jpg", di, ref listFile);
            var distinctList = listFile.DistinctBy(p => p.fileName).ToList();
            dg1.DataSource = distinctList;
            insertData(distinctList);
        }
        /// <summary>
        /// 檢查torrent是否重複
        /// </summary>
        /// <param name="checkFile">檔案名稱</param>
        /// <param name="fileTime">檔案建立時間</param>
        /// <returns></returns>
        private List<string> getTorrentFile(string checkFile,DateTime fileTime,string fileName="")
        {

            List<string> listTorrent = new List<string>();
            var folderPath = txtPath.Text;
            DirectoryInfo di = new DirectoryInfo(folderPath);
            
            foreach(var fi in di.GetFiles("*.torrent",SearchOption.TopDirectoryOnly))
            {
                if (fi.Name == fileName)
                    continue;
                var torrentName = fi.Name.Replace("-","").ToUpper();
                if (torrentName.Contains(checkFile.Replace("-", "").ToUpper()) && fileTime != fi.CreationTime)
                    listTorrent.Add(torrentName);
            }
            return listTorrent;
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
                    fileTime = fi.CreationTime.ToString(),
                    location = fi.FullName
                };
                if (extendName == "*.jpg")
                    fd.fileName = getJpgName(fi.Name);
                else
                    fd.fileName = getShortName(fi.Name);
                if(!string.IsNullOrWhiteSpace(fd.fileName))
                    listFile.Add(fd);
                //msg(fi.FullName);
                //msg(fi.Name);
            }
        }
        private string checkRegexStr(string input)
        {
            //string urlPattern = @"^.+(\.net|\.com|\.cc|\.co|\.tv)[^a-zA-Z0-9]?"; //網址
            string urlPattern = @"\w+\.(net|com|cc|co|tv)";
            string ipPattern = @"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";    //IP
            if (regexReplaceStr(ref input, urlPattern)) // 先判斷是否有網址?網址取代:繼續
                return input;
            else if (regexReplaceStr(ref input, ipPattern)) // 先判斷是否有ip?ip取代:繼續
                return input;

            return input;

        }
        /// <summary>
        /// 找到input 並取代為empty
        /// </summary>
        /// <param name="input"></param>
        /// <param name="urlPattern"></param>
        /// <returns></returns>
        private Boolean regexReplaceStr(ref string input, string urlPattern)
        {
            Regex urlrgx = new Regex(urlPattern, RegexOptions.IgnoreCase);
            MatchCollection urlmatches = urlrgx.Matches(input);
            if (urlmatches.Count > 0)
            {
                foreach (Match match in urlmatches)
                    input = input.Replace(match.Value, "");
                return true;
            }
            return false;
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
            string urlPattern = @"^.+(.net|.com|.cc|.co|.tv)[^a-zA-Z0-9]?"; //網址
            string pattern = @"[a-zA-Z]{2,}-{0,}\d{2,}\w?(.avi|.mp4)";
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
                string finalStr = "";
                foreach (Match match in matches)
                    finalStr = match.Value;
                return finalStr.Replace("-","").Replace(".avi","").Replace(".mp4","");                    
            }   
            return input;
        }
        private string getJpgName(string fileName)
        {
            string pattern = @"[a-zA-Z]{2,}\d{3,}";               

            var input = checkRegexStr(fileName);
            //取檔案名

            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(input.Replace("-", ""));
            if (matches.Count > 0)
            {

                //todo 手動排除big,拉成設定檔
                string value="";
                foreach (Match match in matches)
                    value = match.Value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    
                    value = value.Replace("big", "");                    
                    if (value.Length >= 8)
                    {
                        string doubleZero = "00";
                        rgx = new Regex(doubleZero);
                        return rgx.Replace(value, "",1);
                    }
                    return value;                        
                }
            }
            return "";            
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
        private delegate void UpdateMsgCallBack(string message);
        private void UpdateMsg(string message)
        {
            if (this.InvokeRequired)
            {
                UpdateMsgCallBack uu = new UpdateMsgCallBack(UpdateMsg);
                this.Invoke(uu, message);
            }
            else
            {
                txtMsg.Text += Environment.NewLine + message;
                txtMsg.SelectionStart = txtMsg.Text.Length;
                txtMsg.ScrollToCaret();
            }
        }
        private void msg(string message)
        {
            txtMsg.Text += Environment.NewLine+ message;
            txtMsg.SelectionStart = txtMsg.Text.Length;
            txtMsg.ScrollToCaret();
        }
       
        /// <summary>
        /// 檢查db內有沒有資料
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
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
        private async Task<IEnumerable<fileData>> searchSourceKeyword(string keyword)
        {
            keyword = "%" + keyword + "%";

            using (var conn = new SQLiteConnection("Data source = fileLibary.db"))
            {
                conn.Open();
                var result = await conn.QueryAsync<fileData>(@"select * from movie where sourceName like @keyword;", new { keyword });
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
                
                //string pattern = @"[a-zA-Z]{2,}\d{3,}";
                while (sr.Peek() >= 0)
                {
                    var filepath = sr.ReadLine();
                    input = Path.GetFileName(filepath);
                    
                    input = checkRegexStr(input);
                                        
                    fileData fd = new fileData()
                    {
                        sourceName = Path.GetFileName(input),
                        fileName = getJpgName(input),
                        fileTime = "2016/12/30",
                        location = filepath
                    };
                    listFile.Add(fd);
                    //Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    //MatchCollection matches = rgx.Matches(input.Replace("-", ""));
                    //if (matches.Count > 0)
                    //{
                    //    //Console.WriteLine("{0} ({1} matches):", input, matches.Count);
                    //    foreach (Match match in matches)
                    //    {
                    //        fileData fd = new fileData()
                    //        {
                    //            sourceName = Path.GetFileName(input),
                    //            fileName = match.Value,
                    //            fileTime = "2016/12/30",
                    //            location = filepath
                    //        };
                    //        listFile.Add(fd);
                    //    }
                    //}
                    //else
                    //{
                    //    fileData fd = new fileData()
                    //    {
                    //        sourceName = Path.GetFileName(input),
                    //        fileName = Path.GetFileName(input),
                    //        fileTime = "2016/12/30",
                    //        location = filepath
                    //    };
                    //    listFile.Add(fd);
                    //}

                }
                sr.Close();
            }
            var distinctList = listFile.DistinctBy(p =>  p.fileName );
            dg1.DataSource = distinctList.ToList();
            msg("總筆數：" + distinctList.Count());
            //dg1.DataSource = listFile.DistinctBy(p=>new { p.fileName });
            //insertData(listFile);


        }
 






        #endregion

        private void button6_Click(object sender, EventArgs e)
        {
            //testRegex();
            string keyword = txtKeyword.Text;
            string testStr = getShortName(keyword);
            msg(testStr);
        }

        private void dg1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            dg1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dg1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dg1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dg1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private void dg1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;
            if(row>=0 && col >=0)
            {
                //msg(string.Format("value:{0}",dg1.Rows[row].Cells[col].Value));
                string filePath = Path.GetDirectoryName(dg1.Rows[row].Cells[2].Value.ToString());
                System.Diagnostics.Process.Start("explorer.exe", filePath);                
            }

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            txtKeyword.SelectionStart = 0;
            txtKeyword.SelectionLength = txtKeyword.Text.Length ;
            txtKeyword.Select();
        }
    }
    public static class EnumerableExtender
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
