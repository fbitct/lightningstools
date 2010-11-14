using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace F4ResourceFileEditor
{
    public partial class frmMain : Form
    {
        [Serializable]
        internal class EditorState
        {
            [Serializable]
            public class F4Resource
            {
                public string ID;
                public F4Utils.Resources.F4ResourceType ResourceType;
                public byte[] Data;
            }
            public Dictionary<string, F4Resource> Resources = new Dictionary<string, F4Resource>();
            [NonSerialized]
            public string FilePath = null;
            public bool ChangesMade = false;


            public object Clone()
            {
                EditorState cloned = null;
                using (MemoryStream ms = new MemoryStream(1000))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, this);
                    ms.Seek(0, SeekOrigin.Begin);
                    cloned = (EditorState)bf.Deserialize(ms);
                    ms.Close();
                }
                return cloned;
            }
        }
        private F4Utils.Resources.F4ResourceBundleReader _reader = new F4Utils.Resources.F4ResourceBundleReader();

        private EditorState _editorState = new EditorState();
        public frmMain()
        {
            IntPtr SaveFilter = SetUnhandledExceptionFilter(IntPtr.Zero);
            InitializeComponent();
            SetUnhandledExceptionFilter(SaveFilter);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.WorkerSupportsCancellation = true;
            UpdateMenus();
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            FileOpen();
            UpdateView();
        }
        private void UpdateMenus()
        {
            if (_editorState.ChangesMade)
            {
                mnuFileSaveAs.Enabled = true;
                mnuFileSave.Enabled = true;
            }
            else
            {
                mnuFileSaveAs.Enabled = false;
                mnuFileSave.Enabled = false;
            }
        }
        private void UpdateView()
        {

            tvResources.Nodes.Clear();
            foreach (var thisStateRecord in _editorState.Resources)
            {
                string thisResourceID = thisStateRecord.Key;
                var thisResource = thisStateRecord.Value;
                string thisResourceType = thisResource.ResourceType.ToString();
                TreeNode newNode = tvResources.Nodes.Add(thisResourceID, thisResourceID + "(" + thisResourceType + ")");
                newNode.Tag = thisResource;
            }
            this.Text = Application.ProductName + " - ";
            if (_editorState.ChangesMade)
            {
                this.Text = this.Text + "*";
            }
            if (_editorState.FilePath != null)
            {
                this.Text = this.Text + new FileInfo(_editorState.FilePath).Name;
            }

        }
        private void FileOpen()
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.AddExtension = true;
            dlgOpen.AutoUpgradeEnabled = true;
            dlgOpen.CheckFileExists = true;
            dlgOpen.CheckPathExists = true;
            dlgOpen.DefaultExt = ".rsc";
            dlgOpen.Filter = "Falcon 4 Resource Files (*.rsc)|*.rsc";
            dlgOpen.FilterIndex = 0;
            dlgOpen.DereferenceLinks = true;
            //dlgOpen.InitialDirectory = new FileInfo(Application.ExecutablePath).DirectoryName;
            dlgOpen.Multiselect = false;
            dlgOpen.ReadOnlyChecked = false;
            dlgOpen.RestoreDirectory = true;
            dlgOpen.ShowHelp = false;
            dlgOpen.ShowReadOnly = false;
            dlgOpen.SupportMultiDottedExtensions = true;
            dlgOpen.Title = "Open Resource File";
            dlgOpen.ValidateNames = true;
            DialogResult result = dlgOpen.ShowDialog(this);
            if (result == DialogResult.Cancel)
            {
                return;
            }
            else if (result == DialogResult.OK)
            {
                LoadResourceFile(dlgOpen.FileName);
            }

        }
        private string GetFileNameWithoutExtension(string fileName)
        {
            string toReturn = fileName.Substring(0, fileName.Length - new FileInfo(fileName).Extension.Length);
            return toReturn;
        }
        private void LoadResourceFile(string resourceFilePath)
        {

            if (string.IsNullOrEmpty(resourceFilePath)) throw new ArgumentNullException("resourceFilePath");
            FileInfo resourceFileFI = new FileInfo(resourceFilePath);
            if (!resourceFileFI.Exists) throw new FileNotFoundException(resourceFilePath);
            FileInfo resourceIndexFileFI = new FileInfo(resourceFileFI.DirectoryName + Path.DirectorySeparatorChar + GetFileNameWithoutExtension(resourceFileFI.Name) + ".idx");
            if (!resourceIndexFileFI.Exists) throw new FileNotFoundException(resourceIndexFileFI.FullName);

            EditorState oldEditorState = (EditorState)_editorState.Clone();
            try
            {
                _editorState = new EditorState();
                _editorState.FilePath = resourceFilePath;
                _reader = new F4Utils.Resources.F4ResourceBundleReader();
                _reader.Load(resourceIndexFileFI.FullName);
                for (int i = 0; i < _reader.NumResources; i++)
                {
                    F4Utils.Resources.F4ResourceType thisResourceType = _reader.GetResourceType(i);
                    EditorState.F4Resource thisResourceStateRecord = new EditorState.F4Resource();
                    thisResourceStateRecord.ResourceType = thisResourceType;
                    thisResourceStateRecord.ID = _reader.GetResourceID(i);
                    switch (thisResourceType)
                    {
                        case F4Utils.Resources.F4ResourceType.Unknown:
                            break;
                        case F4Utils.Resources.F4ResourceType.ImageResource:
                            Bitmap resourceData = _reader.GetImageResource(i);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                resourceData.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                                ms.Flush();
                                ms.Seek(0, SeekOrigin.Begin);
                                thisResourceStateRecord.Data = ms.ToArray();
                                ms.Close();
                            }
                            break;
                        case F4Utils.Resources.F4ResourceType.SoundResource:
                            thisResourceStateRecord.Data = _reader.GetSoundResource(thisResourceStateRecord.ID);
                            break;
                        case F4Utils.Resources.F4ResourceType.FlatResource:
                            thisResourceStateRecord.Data = _reader.GetFlatResource(thisResourceStateRecord.ID);
                            break;
                        default:
                            break;
                    }
                    _editorState.Resources.Add(thisResourceStateRecord.ID, thisResourceStateRecord);
                }
                _editorState.ChangesMade = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred while loading the file.\n\n {0}", e.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                _editorState = oldEditorState;
            }
        }

        private void tvResources_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            EditorState.F4Resource thisResource = (EditorState.F4Resource)e.Node.Tag;
            splitContainer1.Panel2.Controls.Clear();

            switch (thisResource.ResourceType)
            {
                case F4Utils.Resources.F4ResourceType.Unknown:
                    break;
                case F4Utils.Resources.F4ResourceType.ImageResource:
                    RenderImageResource(thisResource.ID);
                    break;
                case F4Utils.Resources.F4ResourceType.SoundResource:
                    RenderSoundResource(thisResource.ID);
                    break;
                case F4Utils.Resources.F4ResourceType.FlatResource:
                    break;
                default:
                    break;
            }
        }
        private void RenderImageResource(string resourceId)
        {
            splitContainer1.Panel2.Controls.Clear();
            Bitmap bmp = null;
            using (MemoryStream ms = new MemoryStream(_editorState.Resources[resourceId].Data))
            {
                bmp = (Bitmap)Bitmap.FromStream(ms);
            }

            PictureBox pb = new PictureBox();
            pb.SizeMode = PictureBoxSizeMode.AutoSize;
            pb.Width = bmp.Width;
            pb.Height = bmp.Height;
            pb.Image = bmp;
            splitContainer1.Panel2.Controls.Add(pb);
        }
        private void RenderSoundResource(string resourceId)
        {
            splitContainer1.Panel2.Controls.Clear();
            Button toPlay = new Button();
            toPlay.Text = "Play Sound...";
            toPlay.Click += new EventHandler(toPlay_Click);
            toPlay.Tag = _editorState.Resources[resourceId].Data;
            splitContainer1.Panel2.Controls.Add(toPlay);
        }

        void toPlay_Click(object sender, EventArgs e)
        {
            while (backgroundWorker1.IsBusy)
            {
                Application.DoEvents();

            }
            backgroundWorker1.RunWorkerAsync(sender);
        }
        delegate void InvokeWithParms(object[] parms);
        delegate void InvokeWithParm(object parms);
        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] soundData = (byte[])((Button)e.Argument).Tag;
            
            ((Button)e.Argument).Invoke(new InvokeWithParm(DisablePlayButton), e.Argument);
            BackgroundPlay(soundData);
            ((Button)e.Argument).Invoke(new InvokeWithParm(EnablePlayButton), e.Argument);
            
        }
        void EnablePlayButton(object argument)
        {
            ((Button)argument).Enabled = true;
        }
        void DisablePlayButton(object argument)
        {
            ((Button)argument).Enabled = false;
        }
        void BackgroundPlay(object soundBytes)
        {
            byte[] soundData = (byte[])soundBytes;
            string tempFile = Path.GetTempFileName();
            try
            {
                using (FileStream fs = new FileStream(tempFile, FileMode.Create))
                {
                    fs.Write(soundData, 0, soundData.Length);
                    fs.Flush();
                    fs.Close();
                }
                PlaySound(tempFile, IntPtr.Zero, SoundFlags.SND_FILENAME);
            }
            finally
            {
                try
                {
                    new FileInfo(tempFile).Delete();
                }
                catch (IOException)
                {
                }
            }
        }
        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            FileSave();
        }
        private void FileSave()
        {
            //   _editorState.FilePath
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            FileExit();
        }
        private void FileExit()
        {
            Application.Exit();
        }
        [DllImport("winmm.DLL", EntryPoint = "PlaySound", SetLastError = true, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        private static extern bool PlaySound(string szSound, System.IntPtr hMod, SoundFlags flags);
        [DllImport("kernel32.dll")]
        static extern IntPtr SetUnhandledExceptionFilter(IntPtr lpFilter);

    }

    [Flags]
    public enum SoundFlags : int
    {
        SND_SYNC = 0x0000,  // play synchronously (default) 
        SND_ASYNC = 0x0001,  // play asynchronously 
        SND_NODEFAULT = 0x0002,  // silence (!default) if sound not found 
        SND_MEMORY = 0x0004,  // pszSound points to a memory file
        SND_LOOP = 0x0008,  // loop the sound until next sndPlaySound 
        SND_NOSTOP = 0x0010,  // don't stop any currently playing sound 
        SND_NOWAIT = 0x00002000, // don't wait if the driver is busy 
        SND_ALIAS = 0x00010000, // name is a registry alias 
        SND_ALIAS_ID = 0x00110000, // alias is a predefined ID
        SND_FILENAME = 0x00020000, // name is file name 
        SND_RESOURCE = 0x00040004  // name is resource name or atom 
    }

}
