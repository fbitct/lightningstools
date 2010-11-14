﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using mshtml;
using System.Runtime.InteropServices.ComTypes;

namespace UrlToImage
{
//************************************************************************************************
//WARNING: 
//************************************************************************************************
//THIS CLASS MUST BE USED ON AN STA THREAD (Single-threaded Apartment).  
//THE CALLER IS RESPONSIBLE FOR ENSURING THAT THEIR THREAD IS AN STA THREAD, PRIOR TO CALLING.
//FOR USE OF THIS CLASS DIRECTLY FROM WITHIN THE ASP.NET ENVIRONMENT, YOU MUST SET AspCompat=true IN THE <%@Page DIRECTIVE.  
//ALTERNATIVELY, USE THE COMMAND-LINE INTERFACE (.EXE) TO UTILIZE THIS CLASS' FUNCTIONALITY.  THIS WILL ENSURE THE PROPER THREADING 
//IS SET UP FOR YOU BY THE .EXE.
//************************************************************************************************
//
//The WebBrowser control used by this class requires the thread it's running on to have a Windows message pump attached to it.  
//This is handled via the use of Application.Run(), which, in turn, requires a Form object, which is 
//implemented by the private (nested inner class) called UrlRendererUtil() [contained below (in this file),
//within the (outer) UrlRenderer class].
//************************************************************************************************************************    #region UrlRenderer Class
    /// <summary>
    /// Static utility class for rendering HTML URLs to Bitmaps.
    /// </summary>
    public static class UrlRenderer
    {
        /// <summary>
        /// Renders a URL to a bitmap file.
        /// </summary>
        /// <param name="url">the URL to render</param>
        /// <param name="fileName">the destination filename</param>
        /// <param name="millisecondsTimeout">maximum time to spend loading/rendering before timing out</param>
        /// <param name="postData">raw data that needs to be sent in the POST request</param>
        /// <param name="additionalHeaders">any additional headers that need to go in the request</param>
        /// <param name="delayAfterLoad">time (in millis) to delay, after the WebBrowser says the document has loaded, before rendering its contents to the bitmap (allows for AJAX controls to finish loading)</param>
        public static void Render(Uri url, string fileName, int millisecondsTimeout, byte[] postData, string additionalHeaders, int delayAfterLoad, int browserWidth, int browserHeight)
        {
            DateTime startTime = DateTime.Now;
            using (Bitmap rendered = Render(url, millisecondsTimeout, postData, additionalHeaders, delayAfterLoad, browserWidth, browserHeight))
            {
                Console.WriteLine(string.Format("Saving rendered image to {0}...", fileName));
                SaveBitmapUsingFileTypeDerivedFromFileName(rendered, fileName);
                Console.WriteLine(string.Format("Finished!", fileName));
            }
            DateTime endTime = DateTime.Now;
            TimeSpan elapsed = endTime.Subtract(startTime);
            Console.WriteLine(string.Format("Total time spent: {0} ms", (int)elapsed.TotalMilliseconds));
        }

        /// <summary>
        /// Renders a URL to a Bitmap using the IE WebBrowser control
        /// </summary>
        /// <param name="url">the URL to render</param>
        /// <param name="millisecondsTimeout">maximum time to spend loading/rendering before timing out</param>
        /// <param name="postData">raw data that needs to be sent in the POST request</param>
        /// <param name="additionalHeaders">any additional headers that need to go in the request</param>
        /// <param name="delayAfterLoad">time (in millis) to delay, after the WebBrowser says the document has loaded, before rendering its contents to the bitmap (allows for AJAX controls to finish loading)</param>
        /// <returns></returns>
        public static Bitmap Render(Uri url, int millisecondsTimeout, byte[] postData, string additionalHeaders, int delayAfterLoad, int browserWidth, int browserHeight)
        {
            using (UrlRendererUtil renderer = new UrlRendererUtil(url, millisecondsTimeout, postData, additionalHeaders, delayAfterLoad, browserWidth, browserHeight))
            {
                Console.Write(string.Format("Retrieving and rendering {0}\n...", url.ToString()));
                renderer.ProgressChanged += new WebBrowserProgressChangedEventHandler(
                    (sender, e ) => Console.Write(".")
                );
                Application.Run(renderer); //this sets up a Window message queue on the current
                //thread, and then the UrlRendererUtil object onto that 
                //thread.  Since UrlRendererUtil inherits from Form,
                //it is technically a WinForm (although unorthodox)
                //The reason a Form object is used is because the 
                //WebBrowser control used internally by UrlRendererUtil
                //needs for a UI thread to exist due to how it's 
                //architected internally.
                //This slightly bizzarre fact is hidden from the user
                //behind this static method.
                Console.WriteLine();
                Console.WriteLine("Finished!");
                return renderer.RenderTarget;
            }
        }
        private static void SaveBitmapUsingFileTypeDerivedFromFileName(Image image, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                int encoderValue = -1;
                string codecMimeType = "image/png";
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                string compressionType = null;
                string imageFormat = null;
                FileInfo fi = new FileInfo(filename);
                string extension = fi.Extension;
                if (!string.IsNullOrEmpty(extension))
                {
                    extension = extension.Replace(".", "").ToUpperInvariant();
                }
                imageFormat = extension;
                switch (imageFormat)
                {
                    case "BMP":
                        format = System.Drawing.Imaging.ImageFormat.Bmp;
                        codecMimeType = "image/bmp";
                        compressionType = "RLE";
                        break;
                    case "GIF":
                        format = System.Drawing.Imaging.ImageFormat.Gif;
                        codecMimeType = "image/gif";
                        compressionType = "LZW";
                        break;
                    case "JPEG":
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        codecMimeType = "image/jpeg";
                        break;
                    case "JPG":
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        codecMimeType = "image/jpeg";
                        break;
                    case "PNG":
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        codecMimeType = "image/png";
                        break;
                    case "TIFF":
                        format = System.Drawing.Imaging.ImageFormat.Tiff;
                        codecMimeType = "image/tiff";
                        compressionType = "LZW";
                        break;
                    case "TIF":
                        format = System.Drawing.Imaging.ImageFormat.Tiff;
                        codecMimeType = "image/tiff";
                        compressionType = "LZW";
                        break;
                }

                switch (compressionType)
                {
                    case "LZW":
                        encoderValue = (int)EncoderValue.CompressionLZW;
                        break;
                    case "RLE":
                        encoderValue = (int)EncoderValue.CompressionRle;
                        break;
                    default:
                        encoderValue = (int)EncoderValue.CompressionNone;
                        break;
                }

                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Compression;
                EncoderParameters codecParams = new EncoderParameters(1);
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo codecToUse = null;
                codecParams.Param[0] = new EncoderParameter(encoder, encoderValue);
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.MimeType == codecMimeType)
                    {
                        codecToUse = codec;
                        break;
                    }
                }
                try
                {
                    image.Save(fs, codecToUse, codecParams);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

        }
        
        #region UrlRendererUtil inner class
        private class UrlRendererUtil : Form, IDisposable
        {
            public event WebBrowserProgressChangedEventHandler ProgressChanged;

            #region Instance variables
            private long _currentProgress = 0;
            private long _maxProgress= 0;
            private DateTime _lastProgressTime = DateTime.MinValue;
            private WebBrowser _browserControl = new WebBrowser();
            private AutoResetEvent _allFinished = new AutoResetEvent(false);
            private WebBrowserDocumentCompletedEventHandler _documentCompletedHandler = null;
            private WebBrowserProgressChangedEventHandler _progressChangedHandler= null;
            private Bitmap _renderTarget = null;
            private Uri _lastCompletedUrl = null;
            #endregion

            #region Constructors
            public UrlRendererUtil()
                : base()
            {
                this.Load += new EventHandler(UrlRenderer_Load);
                this.Visible = false;
                this.BrowserWidth= Screen.PrimaryScreen.Bounds.Width;
                this.BrowserHeight= Screen.PrimaryScreen.Bounds.Height;
            }

            public UrlRendererUtil(Uri url)
                : this()
            {
                this.Url = url;
                _browserControl.ScrollBarsEnabled = false; //turn off rendering of scroll bars in the WebBrowsere control
                _browserControl.ScriptErrorsSuppressed = true;

                //register for the DocumentCompleted event on the WebBrowser control
                _documentCompletedHandler = new WebBrowserDocumentCompletedEventHandler(browserControl_DocumentCompleted);
                _progressChangedHandler = new WebBrowserProgressChangedEventHandler(_browserControl_ProgressChanged);
                _browserControl.ProgressChanged += _progressChangedHandler;
                _browserControl.DocumentCompleted += _documentCompletedHandler;
            }

            public UrlRendererUtil(Uri url, int millisecondsTimeout)
                : this(url)
            {
                this.Timeout = millisecondsTimeout;
            }
            public UrlRendererUtil(Uri url, byte[] postData, string additionalHeaders)
                : this(url)
            {
                this.PostData = postData;
                this.AdditionalHeaders = additionalHeaders;
            }
            public UrlRendererUtil(Uri url, int millisecondsTimeout, byte[] postData, string additionalHeaders)
                : this(url, postData, additionalHeaders)
            {
                this.Timeout = millisecondsTimeout;
            }
            public UrlRendererUtil(Uri url, int millisecondsTimeout, byte[] postData, string additionalHeaders, int delayAfterLoad)
                : this(url,millisecondsTimeout, postData, additionalHeaders)
            {
                this.DelayAfterLoad=delayAfterLoad;
            }
            public UrlRendererUtil(Uri url, int millisecondsTimeout, byte[] postData, string additionalHeaders, int delayAfterLoad, int browserWidth, int browserHeight)
                : this(url, millisecondsTimeout, postData, additionalHeaders, delayAfterLoad)
            {
                if (browserWidth > 0)
                {
                    this.BrowserWidth= browserWidth;
                }
                if (browserHeight > 0)
                {
                    this.BrowserHeight= browserHeight;
                }

            }
            
            #endregion

            #region Public Properties
            public int BrowserWidth { get; set; }
            public int BrowserHeight { get; set; }

            public int DelayAfterLoad {get;set;}
            public int Timeout { get; set; }
            public Uri Url { get; set; }
            public new bool Disposed { get; set; }
            public byte[] PostData { get; set; }
            public string AdditionalHeaders { get; set; }
            public Bitmap RenderTarget
            {
                get
                {
                    return _renderTarget;
                }
                private set
                {
                    _renderTarget = value;
                }
            }
            #endregion
            

            #region Private Methods
            /// <summary>
            /// Renders the URL to a bitmap
            /// </summary>
            /// <returns>a Bitmap containing the rendered web page</returns>
            private Bitmap RenderToBitmap()
            {
                if (this.Url == null) throw new InvalidOperationException();
                if (this.Disposed) throw new ObjectDisposedException(this.GetType().Name);

                _browserControl.ClientSize = new Size(this.BrowserWidth, this.BrowserHeight);
                _lastCompletedUrl = null;
                _currentProgress = 0;
                _maxProgress = 0;
                _lastProgressTime = DateTime.MinValue;
                //navigate to the URL
                NavigateToUrl();
                //wait for rendering to be finished or to time out
                WaitForRenderFinishOrTimeout(this.Timeout);
                return this.RenderTarget;

            }
            private void NavigateToUrl()
            {
                try
                {
                    //navigate to the desired URL
                    if (this.PostData == null && string.IsNullOrEmpty(this.AdditionalHeaders))
                    {
                        _browserControl.Navigate(this.Url); //this will fire the browserControl_DocumentCompleted() method on completion
                        //_browserControl.Url = this.Url;
                    }
                    else
                    {
                        //"_new"
                        _browserControl.Navigate(this.Url, null, this.PostData, this.AdditionalHeaders); //this will fire the browserControl_DocumentCompleted() method on completion
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
            private bool BrowserIsDoneRetrieving()
            {
                if (_browserControl == null) return false;
                if (_browserControl.Document == null) return false;
                bool browserIsReady = _browserControl.ReadyState == WebBrowserReadyState.Complete;
                bool browserIs100Percent = _currentProgress == _maxProgress;
                bool browserIsNotBusy = !_browserControl.IsBusy;
                bool browserFinishedMainDocument = _lastCompletedUrl == _browserControl.Document.Url;
                return browserIsReady && browserIs100Percent && browserIsNotBusy && browserFinishedMainDocument;// && browserStatusReadsDone;
            }
            private void WaitForRenderFinishOrTimeout(int millisecondsTimeout)
            {
                DateTime _lastProgressTime = DateTime.Now;
                int millisRemaining = millisecondsTimeout;
                while (!BrowserIsDoneRetrieving())
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); //the WebBrowser requires DoEvents() to be called or it does not progress...
                    if (millisecondsTimeout != System.Threading.Timeout.Infinite)
                    {

                        millisRemaining = millisecondsTimeout - (int)DateTime.Now.Subtract(_lastProgressTime).TotalMilliseconds;

                        //throw an exception if we've timed out
                        if (millisRemaining <= 0)
                        {
                            throw new TimeoutException();
                        }
                    }
                }
                if (millisecondsTimeout != System.Threading.Timeout.Infinite)
                {
                    while (millisRemaining > 0)
                    {
                        //if a timeout period was set
                        bool waitedSuccessfully =_allFinished.WaitOne(5); //wait for rendering to be complete or timeout to expire
                        Application.DoEvents();
                        if (waitedSuccessfully) break;
                        millisRemaining -= 5;
                        if (millisRemaining <= 0) throw new TimeoutException();
                    }
                }
                else //no timeout was set, so we're ok to block forever and wait for rendering to complete
                {
                    bool waitedSuccessfully=false;
                    while (!waitedSuccessfully)
                    {
                        waitedSuccessfully=_allFinished.WaitOne(5); //wait for rendering to complete
                        Application.DoEvents();
                    }
                }
            }
            private Bitmap RenderToBitmap(WebBrowser wb)
            {
                if (wb == null)
                    return null;
                Bitmap bmp = null;
                //Get doc2, doc3, and viewobject
                IHTMLDocument2 pDoc2 = wb.Document.DomDocument as IHTMLDocument2;
                if (pDoc2 == null) return null;
                IHTMLDocument3 pDoc3 = wb.Document.DomDocument as IHTMLDocument3;
                if (pDoc3 == null) return null;
                IViewObject pViewObject = pDoc2 as IViewObject;
                if (pViewObject == null) return null;

                IHTMLBodyElement pBody = null;
                IHTMLElement pElem = null;
                IHTMLElement2 pBodyElem = null;
                IHTMLStyle pStyle = null;
                string strStyle;
                int bodyHeight = 0;
                int bodyWidth = 0;
                int rootHeight = 0;
                int rootWidth = 0;
                int height = 0;
                int width = 0;

                //get the IHtmlelement
                pElem = pDoc2.body;

                //Get the IHTMLStyle
                if (pElem != null)
                    pStyle = pElem.style;

                //Get the borderstyle so we can reset it
                strStyle = pStyle.borderStyle;
                //Hide 3D border
                pStyle.borderStyle = "none";
                pBody = pElem as IHTMLBodyElement;
                //No scrollbars
                if (pBody != null)
                    pBody.scroll = "no";

                //Get root scroll h + w
                //QI for IHTMLElement2
                pBodyElem = pElem as IHTMLElement2;
                if (pBodyElem != null)
                {
                    bodyWidth = pBodyElem.scrollWidth;
                    bodyHeight = pBodyElem.scrollHeight;
                }

                //release
                pElem = null;
                pBodyElem = null;
                //Get docelem
                pElem = pDoc3.documentElement;
                //QI for IHTMLElement2
                if (pElem != null)
                {
                    pBodyElem = pElem as IHTMLElement2;
                    //Get scroll H + W
                    if (pBodyElem != null)
                    {
                        rootWidth = pBodyElem.scrollWidth;
                        rootHeight = pBodyElem.scrollHeight;
                    }
                    //calc actual W + H
                    width = rootWidth > bodyWidth ? rootWidth : bodyWidth;
                    height = rootHeight > bodyHeight ? rootHeight : bodyHeight;
                }
                //Set up a rect
                COMRECT rWPos = new COMRECT() { top =0, left =0, right = width, bottom = height};
                wb.Width = width;
                wb.Height  = height;
                //Create bmp
                bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                //draw

                int hr = (int)HRESULTS.S_FALSE;
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    //get hdc
                    IntPtr hdc = gr.GetHdc();
                    hr = pViewObject.Draw(
                        DVASPECT.DVASPECT_CONTENT,
                        (int)-1, IntPtr.Zero, IntPtr.Zero,
                        IntPtr.Zero, hdc, ref rWPos, ref rWPos,
                        IntPtr.Zero, (uint)0);
                    gr.ReleaseHdc(hdc);
                }

                //reset
                if (!string.IsNullOrEmpty(strStyle))
                    pStyle.borderStyle = strStyle;
                if (pBody != null)
                    pBody.scroll = "auto";

                if (hr == (int)HRESULTS.S_OK)
                {
                    return bmp;
                }
                return null;
            }

            #endregion
            #region Event Handlers

            /// <summary>
            /// This event fires when the browser control's loading/rendering progress has changed.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void _browserControl_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
            {
                _lastProgressTime = DateTime.Now;
                _currentProgress = e.CurrentProgress;
                _maxProgress = e.MaximumProgress;
                if (this.ProgressChanged != null)
                {
                    this.ProgressChanged(this, e);
                }
            }

            
            /// <summary>
            /// This event fires when the browser control has finished navigating to the requested URL and has finished loading the page
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void browserControl_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                _lastCompletedUrl = e.Url;
                if (!BrowserIsDoneRetrieving()) return;
                //delay after loading the page to allow AJAX stuff to load
                int millisRemaining = this.DelayAfterLoad;
                DateTime startTime = DateTime.Now;
                while (millisRemaining > 0)
                {
                    Thread.Sleep(1); 
                    Application.DoEvents();
                    millisRemaining = this.DelayAfterLoad- (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;
                }


                try
                {
                    WebBrowserBase bc = (WebBrowserBase)_browserControl;
                    this.RenderTarget = RenderToBitmap(_browserControl);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(e.ToString());
                }
                finally
                {
                    //raise the "Finished" signal so the thread that is blocked waiting for us to complete can unblock
                    _allFinished.Set();
                }
            }
            /// <summary>
            /// This is the Form_Load event for the UrlRenderer class.  
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void UrlRenderer_Load(object sender, EventArgs e)
            {
                RenderToBitmap();//start the rendering procecss
                this.Close(); //and close the form afterward
            }

            #endregion

            #region Object Disposal
            /// <summary>
            /// Internal implementation of IDisposable.Dispose
            /// </summary>
            /// <param name="disposing">true, if called from the public Dispose() method, or false if called from the finalizer</param>
            protected new virtual void Dispose(bool disposing)
            {
                if (!this.Disposed)
                {
                    if (disposing)
                    {
                        //dispose the BrowserControl object
                        if (_browserControl != null)
                        {
                            try
                            {
                                //unregister DocumentCompleted event handler if it exists
                                if (_documentCompletedHandler != null)
                                {
                                    try
                                    {
                                        _browserControl.DocumentCompleted -= _documentCompletedHandler;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    _documentCompletedHandler = null;
                                }

                                //unregister ProgressChanged event handler if it exists
                                if (_progressChangedHandler != null)
                                {
                                    try
                                    {
                                        _browserControl.ProgressChanged -= _progressChangedHandler;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    _progressChangedHandler = null;
                                }

                                
                                //now dispose browser control
                                _browserControl.Dispose();
                            }
                            catch (ObjectDisposedException) //if the object is already disposed, don't worry about it
                            {
                            }

                            //and null our object reference out
                            _browserControl = null;
                        }

                        //close our AutoResetEvent wait handle
                        if (_allFinished != null)
                        {
                            try
                            {
                                _allFinished.Close();
                            }
                            catch (Exception) //if it's already closed, don't worry about it
                            {
                            }

                            //and null the object reference out
                            _allFinished = null;
                        }

                        //null out any potentially long-held object references 
                        this.RenderTarget= null; //just null our reference to the render target, but don't actually dispose it, 
                        //since it could be still in use by the consumer 
                        //of this object, who will be responsible for discarding it 
                        //when they are done with it
                        this.PostData = null;
                        this.AdditionalHeaders = null;
                    }
                    //set a flag so we know this object has been disposed
                    this.Disposed = true;

                }
            }
            /// <summary>
            /// Public implementation of IDisposable.Dispose()
            /// </summary>
            public void Dispose()
            {
                if (!this.Disposed)
                {
                    Dispose(true);
                }
                GC.SuppressFinalize(this);
            }
            /// <summary>
            /// Deterministic finalizer
            /// </summary>
            ~UrlRendererUtil()
            {
                Dispose(false);
            }
            #endregion

        }
        #endregion
    }

}
