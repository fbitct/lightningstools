using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using F4Utils.Speech;
using Common.Strings;
using System.Collections;
using System.Media;

namespace TlkTool.UI
{
    public partial class frmMain : Form
    {
        private string _currentTlkFilePath = null;
        private SoundPlayer _player = new SoundPlayer();
        public frmMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileExit();
        }

        private static void FileExit()
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileOpen();
        }

        private void FileOpen()
        {
            if (tabControl1.SelectedTab == tabTlkFile)
            {
                OpenTlkFile();
            }
        }
        private void OpenTlkFile()
        {
            PromptForTlkFile();
        }
        private void PromptForTlkFile()
        {
            openFileDialog1.Reset();
            openFileDialog1.AddExtension = true;
            openFileDialog1.AutoUpgradeEnabled = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.DefaultExt = ".TLK";
            openFileDialog1.DereferenceLinks = true;
            openFileDialog1.Filter = @"All Files (*.*)|*.*|TLK Files (*.TLK)|*.TLK";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;
            openFileDialog1.ReadOnlyChecked = false;
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.ShowHelp = false;
            openFileDialog1.ShowReadOnly = false;
            openFileDialog1.SupportMultiDottedExtensions = true;
            openFileDialog1.ValidateNames = true;
            openFileDialog1.FileOk += new CancelEventHandler(openTlkFileDialog_FileOk);
            openFileDialog1.ShowDialog(this);

        }

        void openTlkFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel) return;

            LoadTlkFile(openFileDialog1.FileName);
        }
        void LoadTlkFile(string tlkFilePath)
        {
            PopulateTreeViewWithNamesOfFilesInTlkFile(tlkFilePath, tvTlkFileContents);
            _currentTlkFilePath = tlkFilePath;
        }
        void PopulateTreeViewWithNamesOfFilesInTlkFile(string tlkFilePath, TreeView treeview)
        {
            TlkFile tlkFile=TlkFile.Load(tlkFilePath);
            SetCurrentTlkFile(tlkFile, tlkFilePath);
            string codecType = tlkFile.DetectTlkFileCodecType().ToString();
            for(int i=0;i<tlkFile.Records.Length;i++)
            {
                treeview.Nodes[0].Nodes.Add(string.Format("{0}.{1}", i.ToString(), codecType));
            }
            
            //treeview.TreeViewNodeSorter =  new StringLogicalComparer();
            treeview.ExpandAll();
            treeview.SelectedNode = treeview.Nodes[0];
            treeview.Refresh();
            treeview.Select();
        }
        void DisplayTlkNodeOptions()
        {
        }

        private void cmdPlayFrag_Click(object sender, EventArgs e)
        {
            PlaySelectedFrag();
        }
        void StopPlayingFrag()
        {
            if (_player != null)
            {
                try
                {
                    _player.Stop();
                }
                catch (Exception e)
                {
                }
            }
        }
        void PlaySelectedFrag()
        {
            if (tvTlkFileContents.SelectedNode == null ||
                tvTlkFileContents.SelectedNode == tvTlkFileContents.Nodes[0])
            {
                return;
            }

            string selectedFragName=tvTlkFileContents.SelectedNode.Text;
            int selectedTlkId = Int32.Parse(Path.GetFileNameWithoutExtension(selectedFragName));
            TlkFile currentTlkFile = GetCurrentTlkFile();
            using (MemoryStream ms = new MemoryStream())
            {
                StopPlayingFrag();
                using (_player = new SoundPlayer(ms))
                {
                    currentTlkFile.DecompressRecordAndWriteToStream(
                        currentTlkFile.DetectTlkFileCodecType(),
                        currentTlkFile.Records[selectedTlkId],
                        ms);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    _player.Play();
                }
            }
        }
        TlkFile GetCurrentTlkFile()
        {
            if (tvTlkFileContents.Nodes != null && tvTlkFileContents.Nodes.Count > 0 && tvTlkFileContents.Nodes[0] != null)
            {
                return (TlkFile)((tvTlkFileContents.Nodes[0]).Tag);
            }
            else return null;
        }
        void SetCurrentTlkFile(TlkFile tlkFile, string tlkFilePath)
        {
            tvTlkFileContents.Nodes.Clear();
            TreeNode rootNode = tvTlkFileContents.Nodes.Add(tlkFilePath);
            rootNode.Tag = tlkFile;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposePlayer();
        }

        private void DisposePlayer()
        {
            if (_player != null)
            {
                try
                {
                    _player.Dispose();
                }
                catch (Exception e)
                {
                }
                _player = null;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void cmdStopPlayingFrag_Click(object sender, EventArgs e)
        {
            StopPlayingFrag();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileNew();
        }
        void FileNew()
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSave();
        }
        void FileSave()
        {
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileSaveAs();
        }
        void FileSaveAs()
        {
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolsOptions();
        }
        void ToolsOptions()
        {
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            FileNew();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            FileOpen();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            FileSave();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpAbout();
        }
        private void HelpAbout()
        {
            new frmHelpAbout().ShowDialog(this);
        }

        private void tvTlkFileContents_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string droppedFileName in droppedFileNames)
                {
                    AddFragNode(droppedFileName);
                }
            }
        }
        private void AddFragNode(string fileName)
        {
            TlkFile currentTlkFile = GetCurrentTlkFile();
            string fileFormat = Path.GetExtension(fileName);
            while (!string.IsNullOrEmpty(fileFormat) && fileFormat.StartsWith("."))
            {
                fileFormat = fileFormat.Substring(1, fileFormat.Length - 1);
            }
            if (!string.IsNullOrEmpty(fileFormat))
            {
                int index=-1;
                bool parsed = Int32.TryParse(fileName, out index);
                if (parsed)
                {
                }
                else
                {
                    MessageBox.Show(
                        string.Format(
                            "Could not add file {0}. File name must be numeric "
                             + "and must end with .WAV, .SPX, or .LH.", fileName
                             ), Application.ProductName);
                }
            }

        }
        private void tvTlkFileContents_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        
    }
}
