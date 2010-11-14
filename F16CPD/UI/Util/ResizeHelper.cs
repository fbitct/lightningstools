using System.Drawing;
using System.Windows.Forms;
using Common.Win32;

namespace F16CPD.UI.Util
{
    /// <summary>
    /// Contains helper code to assist in intercepting window messages associated with form events
    /// to determine if a resizable border should be presented on the form or not, depending
    /// on the cursor's location relative to the form's (invisible) border.  Also provides
    /// the behavior for actually resizing the form if these conditions are true and the user
    /// does a drag-type operation within the form's border area
    /// </summary>
    class ResizeHelper : IMessageFilter
    {
        private Form _TheWindow;
        public Form TheWindow
        {
            get
            {
                return _TheWindow;
            }
            set
            {
                _TheWindow = value;
            }
        }

        public ResizeHelper(Form form)
        {
            this._TheWindow = form;
            Application.AddMessageFilter(this);
        }

        #region IMessageFilter Members

        //[System.Diagnostics.DebuggerHidden()]
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (!this._TheWindow.IsDisposed && this._TheWindow.Visible && 0 == this._TheWindow.OwnedForms.Length)
            {
                Point pt = Cursor.Position;
                Rectangle formBounds = this._TheWindow.DesktopBounds;
                if (formBounds.Contains(pt))
                {
                    // Create a rectangular area which is 7 pix smaller than windows's size
                    // This is the area beyond which we need to simulate non client area
                    Rectangle bounds = new Rectangle(formBounds.X, formBounds.Y, formBounds.Width, formBounds.Height);
                    bounds.Inflate(-7, -7);

                    int htValue = NativeMethods.HT.HTNOWHERE;

                    // If the cursor is outside this inner rectangle, we need to
                    // check for resize
                    if (!bounds.Contains(pt))
                    {

                        // Mouse is moving, if it is around the border edge, then we need
                        // to set the cursor
                        if (pt.X < bounds.Left) // Cursor left side
                        {
                            if (pt.Y < bounds.Top)
                            {
                                // Cursor top left
                                this.TheWindow.Cursor = Cursors.SizeNWSE;
                                htValue = NativeMethods.HT.HTTOPLEFT;
                            }
                            else if (pt.Y > bounds.Bottom)
                            {
                                // Cursor bottom left
                                this.TheWindow.Cursor = Cursors.SizeNESW;
                                htValue = NativeMethods.HT.HTBOTTOMLEFT;
                            }
                            else
                            {
                                // cursor left
                                this.TheWindow.Cursor = Cursors.SizeWE;
                                htValue = NativeMethods.HT.HTLEFT;
                            }
                        }
                        else if (pt.X > bounds.Right) // Cursor right side
                        {
                            if (pt.Y < bounds.Top)
                            {
                                // Cursor top right
                                this.TheWindow.Cursor = Cursors.SizeNESW;
                                htValue = NativeMethods.HT.HTTOPRIGHT;
                            }
                            else if (pt.Y > bounds.Bottom)
                            {
                                // Cursor bottom right
                                this.TheWindow.Cursor = Cursors.SizeNWSE;
                                htValue = NativeMethods.HT.HTBOTTOMRIGHT;
                            }
                            else
                            {
                                // cursor right
                                this.TheWindow.Cursor = Cursors.SizeWE;
                                htValue = NativeMethods.HT.HTRIGHT;
                            }
                        }
                        else // cursor is in between the form
                        {
                            if (pt.Y < bounds.Top)
                            {
                                // cursor is top
                                this.TheWindow.Cursor = Cursors.SizeNS;
                                htValue = NativeMethods.HT.HTTOP;
                            }
                            else if (pt.Y > bounds.Bottom)
                            {
                                // Cursor is bottom
                                this.TheWindow.Cursor = Cursors.SizeNS;
                                htValue = NativeMethods.HT.HTBOTTOM;
                            }
                        }

                        if (m.Msg == NativeMethods.WM.WM_MOUSEMOVE)
                        {
                            // the cursor is already set, we have nothing to do
                            //this._TheWindow.Cursor = cursor;
                            return true; // The message is handled
                        }
                        else if (m.Msg == NativeMethods.WM.WM_LBUTTONDOWN)
                        {
                            // Start resizing
                            NativeMethods.ReleaseCapture();
                            NativeMethods.SendMessage(this._TheWindow.Handle, NativeMethods.WM.WM_NCLBUTTONDOWN,
                                htValue, 0);
                            return true; // The message is handled					
                        }
                        else
                        {
                            this._TheWindow.Cursor = Cursors.Default;
                            return false; // The message is NOT handled					
                        }
                    }
                    else // Cursor inside the window
                    {
                        bounds.Inflate(-50, -50);
                        if (bounds.Contains(pt))
                        {
                            this._TheWindow.Cursor = Cursors.SizeAll;
                        }
                        else
                        {
                            this._TheWindow.Cursor = Cursors.Default;
                        }
                        return false;
                    }
                }
                else
                {
                    this._TheWindow.Cursor = Cursors.Default;
                    return false;
                }
            }
            return false;
        }

        #endregion
    }
}
