using System;
using System.Drawing;
using System.Windows.Forms;

namespace MFDExtractor.UI
{
    public partial class InstrumentForm : DraggableForm
    {
        private const int MIN_WINDOW_WIDTH = 50;
        private const int MIN_WINDOW_HEIGHT = 50;
        private bool _adjustLocation;
        private bool _alwaysOnTop;
        private Rectangle _boundsOnResizeBegin = Rectangle.Empty;
        private Cursor _cursorOnResizeBegin;
        private Rectangle _lastBounds = Rectangle.Empty;
        private bool _monochrome;
        private ResizeHelper _resizeHelper;
        private RotateFlipType _rotation;
        private bool _stretchChanging;
        private bool _stretchToFill;

        public InstrumentForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            if (!base.DesignMode)
            {
                _resizeHelper = new ResizeHelper(this);
            }
        }

        public bool RenderImmediately { get; set; }
        public DateTime LastRenderedOn { get; set; }

        public bool StretchToFill
        {
            get { return _stretchToFill; }
            set
            {
                _stretchChanging = true;
                _stretchToFill = value;
                ctxStretchToFill.Checked = _stretchToFill;
                if (_stretchToFill)
                {
                    SetFullScreen();
                }
                else
                {
                    Size = _lastBounds.Size;
                    Location = _lastBounds.Location;
                }
                OnDataChanged(new EventArgs());
                _stretchChanging = false;
            }
        }

        public bool Monochrome
        {
            get { return _monochrome; }
            set
            {
                _monochrome = value;
                ctxMonochrome.Checked = _monochrome;
                OnDataChanged(new EventArgs());
            }
        }

        public bool AlwaysOnTop
        {
            get { return _alwaysOnTop; }
            set
            {
                _alwaysOnTop = value;
                TopMost = _alwaysOnTop;
                ctxAlwaysOnTop.Checked = _alwaysOnTop;
                OnDataChanged(new EventArgs());
            }
        }

        public RotateFlipType Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                ApplyRotationCheck();
                OnDataChanged(new EventArgs());
            }
        }

        public event EventHandler DataChanged;

        protected void ApplyRotationCheck()
        {
            ClearRotationChecks();
            switch (_rotation)
            {
                case RotateFlipType.RotateNoneFlipNone:
                    ctxRotationNoRotationNoFlip.Checked = true;
                    break;
                case RotateFlipType.Rotate90FlipNone:
                    ctxRotatePlus90Degrees.Checked = true;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    ctxRotateMinus90Degrees.Checked = true;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                    ctxRotate180Degrees.Checked = true;
                    break;
                case RotateFlipType.RotateNoneFlipX:
                    ctxFlipHorizontally.Checked = true;
                    break;
                case RotateFlipType.RotateNoneFlipY:
                    ctxFlipVertically.Checked = true;
                    break;
                case RotateFlipType.Rotate90FlipX:
                    ctxRotatePlus90DegreesFlipHorizontally.Checked = true;
                    break;
                case RotateFlipType.Rotate90FlipY:
                    ctxRotatePlus90DegreesFlipVertically.Checked = true;
                    break;
                default:
                    break;
            }
        }

        protected void ClearRotationChecks()
        {
            ctxFlipHorizontally.Checked = false;
            ctxFlipVertically.Checked = false;
            ctxRotate180Degrees.Checked = false;
            ctxRotateMinus90Degrees.Checked = false;
            ctxRotatePlus90Degrees.Checked = false;
            ctxRotatePlus90DegreesFlipHorizontally.Checked = false;
            ctxRotatePlus90DegreesFlipVertically.Checked = false;
            ctxRotationNoRotationNoFlip.Checked = false;
        }

        protected void OnDataChanged(EventArgs e)
        {
            if (DataChanged != null)
            {
                DataChanged(this, e);
            }
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            _cursorOnResizeBegin = Cursor;
            _boundsOnResizeBegin = Bounds;
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            _boundsOnResizeBegin = Rectangle.Empty;
            _cursorOnResizeBegin = null;
            base.OnResizeEnd(e); //TODO: verify this works (it's new)
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_stretchToFill)
            {
                return;
            }
            if (Width < MIN_WINDOW_WIDTH)
            {
                Width = MIN_WINDOW_WIDTH;
            }
            if (Height < MIN_WINDOW_HEIGHT)
            {
                Height = MIN_WINDOW_HEIGHT;
            }
            if (_cursorOnResizeBegin == Cursors.SizeNESW || _cursorOnResizeBegin == Cursors.SizeNWSE)
            {
                if (_boundsOnResizeBegin == Rectangle.Empty)
                {
                    _boundsOnResizeBegin = Bounds;
                }
                float ratio = (_boundsOnResizeBegin.Width/(float) _boundsOnResizeBegin.Height);
                Width = (int) (ratio*Height);
            }
            if (!_stretchToFill && !_stretchChanging)
            {
                _lastBounds = Bounds;
            }
            Screen thisScreen = Screen.FromRectangle(DesktopBounds);
            if (thisScreen == null)
            {
                return;
            }
            if (DesktopBounds.Size != thisScreen.Bounds.Size &&
                DesktopBounds.Location != thisScreen.Bounds.Location)
            {
                _stretchToFill = false;
                if (ctxStretchToFill != null)
                {
                    ctxStretchToFill.Checked = _stretchToFill;
                }
            }
            base.OnSizeChanged(e);
            OnDataChanged(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (_stretchToFill)
            {
                _adjustLocation = true;
            }
            else
            {
                if (!_stretchChanging)
                {
                    _lastBounds = Bounds;
                }
            }
            base.OnLocationChanged(e);
            OnDataChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _boundsOnResizeBegin = Bounds;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool raise = false;
            if (_adjustLocation)
            {
                _adjustLocation = false;
                SetFullScreen();
                raise = true;
            }
            base.OnMouseUp(e);
            if (raise)
            {
                OnDataChanged(e);
            }
        }

        private void SetFullScreen()
        {
            Screen thisScreen = Screen.FromRectangle(DesktopBounds);
            DesktopLocation = thisScreen.Bounds.Location;
            Size = thisScreen.Bounds.Size;
        }

        private void stretchToFillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StretchToFill = !StretchToFill;
        }

        private void ctxHide_Click(object sender, EventArgs e)
        {
            Visible = false;
            OnDataChanged(e);
        }

        private void ctxRotationNoRotationNoFlip_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.RotateNoneFlipNone;
        }

        private void ctxRotatePlus90Degrees_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.Rotate90FlipNone;
        }

        private void ctxRotateMinus90Degrees_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.Rotate270FlipNone;
        }

        private void ctxRotate180Degrees_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.Rotate180FlipNone;
        }

        private void ctxFlipHorizontally_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.RotateNoneFlipX;
        }

        private void ctxFlipVertically_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.RotateNoneFlipY;
        }

        private void ctxRotatePlus90DegreesFlipHorizontally_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.Rotate90FlipX;
        }

        private void ctxRotatePlus90DegreesFlipVertically_Click(object sender, EventArgs e)
        {
            Rotation = RotateFlipType.Rotate90FlipY;
        }

        private void ctxAlwaysOnTop_Click(object sender, EventArgs e)
        {
            AlwaysOnTop = !AlwaysOnTop;
        }

        private void ctxMonochrome_Click(object sender, EventArgs e)
        {
            Monochrome = !Monochrome;
        }

        private void ctxMakeSquare_Click(object sender, EventArgs e)
        {
            if (StretchToFill)
            {
                int height = Height;
                int width = Width;
                Point location = Location;
                StretchToFill = false;
                Location = location;
                Height = height;
                Width = width;
            }
            if (Height < Width)
            {
                Width = Height;
            }
            else
            {
                Height = Width;
            }
        }
    }
}