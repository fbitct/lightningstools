using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using NDesk.Options;
using System.IO;

namespace UrlToImage
{
    class Program
    {
        public const int ERR_LEVEL__ERROR_OCCURRED = 255;
        public const int ERR_LEVEL__SUCCESS= 0;
        [STAThread]
        public static int Main(string[] args)
        {
            //variables to hold raw preparsed command-line options
            OptionSet options = null;

            Uri url = null;
            int timeout = Timeout.Infinite;
            byte[] postData = null;
            string additionalHeaders = null;
            bool showHelp = false;
            bool hide = false;
            string errorMessage = null;
            string fileName = null;
            int delayAfterLoad = 0;
            int browserWidth = 0;
            int browserHeight = 0;
            List<String> extraOptions = null;

            //preparse command line options using NDesk.Options
            bool successfullyParsed = ParseCommandLineOptions(out options, args, out url, out timeout, out postData, out additionalHeaders, out fileName, out showHelp, out hide, out delayAfterLoad, out browserWidth, out browserHeight, out extraOptions, out errorMessage); 

            if (successfullyParsed)
            {
                if (!showHelp)
                {
                    if (hide)
                    {
                        HideConsoleWindow();
                    }
                    try
                    {
                        UrlRenderer.Render(url, fileName, timeout, postData, additionalHeaders, delayAfterLoad,browserWidth, browserHeight);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.ToString());
                        return ERR_LEVEL__ERROR_OCCURRED;
                    }
                }
                return ERR_LEVEL__SUCCESS;
            }
            else
            {
                WriteErrorAndShowUsage(errorMessage, options);
                return ERR_LEVEL__ERROR_OCCURRED;
            }
        }
        private static void HideConsoleWindow()
        {
            IntPtr hWnd = NativeMethods.FindWindow(null, Console.Title);

            if (hWnd != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hWnd, 0);
            }
        }        
        private static bool ParseCommandLineOptions(out OptionSet options, string[] args, out Uri url, out int timeout, out byte[] postData, out string additionalHeaders, out string fileName, out bool showHelp, out bool hide, out int delayAfterLoad, out int browserWidth, out int browserHeight, out List<String> extraOptions, out string errorMessage)
        {

            //set default values for output parms
            {
                options=null;
                url=null;
                timeout=Timeout.Infinite;
                postData=null;
                additionalHeaders=null;
                fileName = null;
                showHelp=false;
                extraOptions=null;
                errorMessage = null;
                hide = false;
                delayAfterLoad = 0;
                browserWidth = 0;
                browserHeight = 0;
            }

            //create variables to hold pre-parsed command line args
            string urlString = null;
            string timeoutString = null;
            string postData__Base64 = null;
            string additionalHeadersInternal = null;
            string fileNameInternal = null;
            bool showHelpInternal = false;
            bool hideInternal = false;
            string delayAfterLoadInternal = null;
            string browserWidthInternal = null;
            string browserHeightInternal = null;
            //create an OptionSet to describe and parse the command line syntax 
            //(consult the Options.cs file for OptionSet syntax)
            options = new OptionSet() 
            {
                {"url=", arg=> urlString=arg},
                {"file=", arg=> fileNameInternal=arg},
                {"timeout=", arg=>timeoutString=arg},
                {"postData=", arg=>postData__Base64=arg},
                {"additionalHeaders=", arg=>additionalHeadersInternal=arg},
                {"hide", arg=>hideInternal=arg !=null},
                {"delay=", arg=>delayAfterLoadInternal=arg},
                {"browserWidth=", arg=>browserWidthInternal=arg},
                {"browserHeight=", arg=>browserHeightInternal=arg},
                {"?|help", arg=>showHelpInternal= arg !=null}
            };
            try
            {
                //pre-parse the command line options
                extraOptions = options.Parse(args);
            }
            catch (OptionException ex)
            {
                errorMessage = ex.Message;
                return false;

            }
            //copy the variables which were populated by the parser lambdas 
            //into their corresponding output params
            showHelp = showHelpInternal;
            additionalHeaders = additionalHeadersInternal;
            fileName = fileNameInternal;
            hide = hideInternal;

            if (!string.IsNullOrEmpty(delayAfterLoadInternal))
            {
                bool delayParsedSuccessfully = Int32.TryParse(delayAfterLoadInternal, out delayAfterLoad);
                if (!delayParsedSuccessfully)
                {
                    errorMessage = string.Format("Invalid delay parameter value: {0}", delayAfterLoadInternal);
                    return false;
                }
            }
            //now, perform additional post-processing command line args

            bool fileNameValid = !string.IsNullOrEmpty(fileName);

            if (!fileNameValid)
            {
                errorMessage = string.Format("Invalid filespec: {0}", fileName);
                return false;
            }

            

            //verify that the URL parameter actually parses to a legitimately formatted URI
            bool urlParsedSuccessfully = Uri.TryCreate(urlString, UriKind.Absolute, out url);
            if (!urlParsedSuccessfully)
            {
                errorMessage = string.Format("Invalid URL: {0}", urlString);
                return false;
            }


            //verify that the supplied timeout, if any, is valid
            if (!string.IsNullOrEmpty(timeoutString))
            {
                bool timeoutParsedSuccessfully = Int32.TryParse(timeoutString, out timeout);
                if (
                    !timeoutParsedSuccessfully
                        ||
                    (timeout < 0 && timeout != Timeout.Infinite)
                 )
                {
                    errorMessage = string.Format("Invalid timeout: {0}", timeoutString);
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(browserWidthInternal))
            {
                bool browserWidthParsedSuccessfully = Int32.TryParse(browserWidthInternal, out browserWidth);
                if (!browserWidthParsedSuccessfully || browserWidth < 0) 
                {
                    errorMessage = string.Format("Invalid browser width: {0}", browserWidth);
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(browserHeightInternal))
            {
                bool browserHeightParsedSuccessfully = Int32.TryParse(browserHeightInternal, out browserHeight);
                if (!browserHeightParsedSuccessfully || browserHeight < 0)
                {
                    errorMessage = string.Format("Invalid browser height: {0}", browserHeight);
                    return false;
                }
            }

            
            //verify that we can parse the PostData as base64 (if supplied)
            if (!string.IsNullOrEmpty(postData__Base64))
            {
                try
                {
                    postData = Convert.FromBase64String(postData__Base64);
                }
                catch (Exception)
                {
                    errorMessage = string.Format("Invalid Base64 string:{0}", postData__Base64);
                    return false;
                }
            }


            return true;
        }
        private static void WriteErrorAndShowUsage(string errorMessage, OptionSet options)
        {
            TextWriter errWriter = Console.Out;
            errWriter.WriteLine(errorMessage);
            errWriter.WriteLine();
            ShowUsage(options, errWriter);
        }
        private static void ShowUsage(OptionSet options, TextWriter o )
        {
            string appName=new FileInfo(Application.ExecutablePath).Name;
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine(appName);
            options.WriteOptionDescriptions(o);
        }
    }
}
