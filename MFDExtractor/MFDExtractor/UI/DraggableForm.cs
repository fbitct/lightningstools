using System.Drawing;
using System.Windows.Forms;

namespace MFDExtractor.UI
{
    /// <summary>
    /// Extension of the basic Windows Form, allowing forms that do not posess a titlebar area to be draggable/moveable
    /// </summary>
    public class DraggableForm : Form
    {

        #region Declarations
        private bool drag = false;
        private Point start_point = new Point(0, 0);
        private bool draggable = true;
        private string exclude_list = "";

        /// <SUMMARY>
        /// Required designer variable.
        /// </SUMMARY>
        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Constructor , Dispose

        public DraggableForm()
        {
            InitializeComponent();

        }

        /// <SUMMARY>
        /// Clean up any resources being used.
        /// </SUMMARY>
        /// true if managed resources should be disposed; otherwise, false.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Common.Util.DisposeObject(components);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        /// <SUMMARY>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </SUMMARY>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DraggableForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "DraggableForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "DraggableForm";
            this.ResumeLayout(false);

        }

        #endregion

        #region Overriden Functions
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            //
            //Add Mouse Event Handlers for each control added into the form,
            //if Draggable property of the form is set to true and the control
            //name is not in the ExcludeList.Exclude list is the comma seperated
            //list of the Controls for which you do not require the mouse handler 
            //to be added. For Example a button.  
            //
            if (this.Draggable && (this.ExcludeList.IndexOf(e.Control.Name, System.StringComparison.OrdinalIgnoreCase) == -1))
            {
                e.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
                e.Control.MouseUp += new MouseEventHandler(Control_MouseUp);
                e.Control.MouseMove += new MouseEventHandler(Control_MouseMove);
            }
            base.OnControlAdded(e);
        }

        #endregion

        #region Event Handlers
        private void Control_MouseDown (object sender, MouseEventArgs e) {
            OnMouseDown(e);
        }
        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }
        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.draggable)
            {
                //
                //On Mouse Down set the flag drag=true and 
                //Store the clicked point to the start_point variable
                //
                this.drag = true;
                this.start_point = new Point(e.X, e.Y);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.draggable)
            {
                //
                //Set the drag flag = false;
                //
                this.drag = false;
            }
            base.OnMouseUp(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //
            //If drag = true, drag the form
            //
            if (this.drag && this.draggable)
            {
                Point p1 = new Point(e.X, e.Y);
                Point p2 = this.PointToScreen(p1);
                Point p3 = new Point(p2.X - this.start_point.X,
                                     p2.Y - this.start_point.Y);
                this.DesktopLocation = p3;
            }
            base.OnMouseMove(e);
        }

        #endregion

        #region Properties

        public string ExcludeList
        {
            set
            {
                this.exclude_list = value;
            }
            get
            {
                return this.exclude_list.Trim();
            }
        }

        public bool Draggable
        {
            set
            {
                this.draggable = value;
            }
            get
            {
                return this.draggable;
            }
        }

        #endregion

    }
}
