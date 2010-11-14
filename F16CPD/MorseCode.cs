using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Contexts;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace F16CPD
{
    public static class MorseCodeTest
    {
        public static void Test()
        {
            MorseCode blink = new MorseCode();
            blink.PlainText = "Paris";
            blink.UnitTimeTick += new EventHandler<UnitTimeTickEventArgs>(blink_UnitTimeTick);
            blink.StartSending();
            System.Threading.Thread.Sleep(5000);
        }

        static void blink_UnitTimeTick(object sender, UnitTimeTickEventArgs e)
        {
            Debug.Write(e.CurrentSignalLineState ? "1" : "0");
        }
    }
    public class UnitTimeTickEventArgs : EventArgs
    {
        private bool _currentSignalLineState = false;
        public UnitTimeTickEventArgs()
            : base()
        {
        }
        public UnitTimeTickEventArgs(bool CurrentSignalLineState)
        {
            _currentSignalLineState = CurrentSignalLineState;
        }
        public bool CurrentSignalLineState
        {
            get
            {
                return _currentSignalLineState;
            }
            set
            {
                _currentSignalLineState = value;
            }
        }
    }
    public class MorseCode:IDisposable
    {
        public MorseCode()
            : base()
        {
            CharactersPerMinute = 120;
        }
        public event EventHandler<UnitTimeTickEventArgs> UnitTimeTick;

        private int _charactersPerMinute; //CPM
        private int _unitTimeMillis; //standard Morse time unit, in milliseconds
        private bool _keepSending = false;
        private bool _isDisposed = false;
        private BackgroundWorker _worker = null;
        private bool _sending = false;
        public int CharactersPerMinute
        {
            get
            {
                return _charactersPerMinute;
            }
            set
            {
                _charactersPerMinute = value;
                _unitTimeMillis = 6000 / value;
            }
        }
        public string PlainText
        {
            get;
            set;
        }
        public void StartSending()
        {
            if (_sending) throw new InvalidOperationException("Already sending");
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerAsync();
        }
        public bool KeepSending
        {
            get
            {
                return _keepSending;
            }
            set
            {
                _keepSending = value;
            }
        }
        public bool Sending
        {
            get
            {
                return _sending;
            }
        }
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _sending = true;
            do
            {
                Send();
                if (_keepSending)
                {
                    SendUnits(GetQuinaryStringForMorsePatternChar(' ')); //insert gap before repeating
                }
            } while (_keepSending);
            _sending = false;
        }
        public void StopSending()
        {
            _keepSending = false;
            while (_sending)
            {
                Thread.Sleep(20);
            }
        }
        public void Send()
        {
            string[] words = PlainText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int a = 0; a < words.Length; a++)
            {
                string thisWord = words[a];
                for (int i = 0; i < thisWord.Length; i++)
                {
                    char somePlainChar = thisWord[i];
                    string morsePatternCurrentPlainChar = GetMorsePatternStringForPlainChar(somePlainChar); //get the Morse code pattern for the current character
                    for (int j = 0; j < morsePatternCurrentPlainChar.Length; j++)
                    {
                        char someMorseChar = morsePatternCurrentPlainChar[j];
                        string quinary = GetQuinaryStringForMorsePatternChar(someMorseChar);
                        SendUnits(quinary);
                        if (j < morsePatternCurrentPlainChar.Length - 1)
                        {
                            SendUnits(GetQuinaryStringForMorsePatternChar(' ')); //send intracharacter gap
                        }
                    }
                    if (i < thisWord.Length - 1)
                    {
                        SendUnits(GetQuinaryStringForMorsePatternChar('_')); //send short gap (between characters in a plaintext word
                    }
                }
                if (a < words.Length - 1)
                {
                    SendUnits(GetQuinaryStringForMorsePatternChar('>')); //send word gap
                }
            }
        }
        private string GetQuinaryStringForMorsePatternChar(char patternChar)
        {
            string toReturn = "";
            switch (patternChar)
            {
                case '.': //dot
                    toReturn += "1";
                    break;
                case '-': //dash
                    toReturn += "111";
                    break;
                case ' ': //intracharacter gap
                    toReturn += "0";
                    break;
                case '_': //short gap (between characters in a word)
                    toReturn += "000";
                    break;
                case '>': //medium gap (between words)
                    toReturn += "0000000";
                    break;

                default:
                    break;
            }
            return toReturn;

        }
       
        private string GetMorsePatternStringForPlainChar(char someChar)
        {
            string toReturn = "";
            switch (Char.ToUpper(someChar))
            {
                case 'A':
                    toReturn = ".-";
                    break;
                case 'B':
                    toReturn = "-...";
                    break;
                case 'C':
                    toReturn = "-.-.";
                    break;
                case 'D':
                    toReturn = "-..";
                    break;
                case 'E':
                    toReturn = ".";
                    break;
                case 'F':
                    toReturn = "..-.";
                    break;
                case 'G':
                    toReturn = "--.";
                    break;
                case 'H':
                    toReturn = "....";
                    break;
                case 'I':
                    toReturn = "..";
                    break;
                case 'J':
                    toReturn = ".---";
                    break;
                case 'K':
                    toReturn = "-.-";
                    break;
                case 'L':
                    toReturn = ".-..";
                    break;
                case 'M':
                    toReturn = "--";
                    break;
                case 'N':
                    toReturn = "-.";
                    break;
                case 'O':
                    toReturn = "---";
                    break;
                case 'P':
                    toReturn = ".--.";
                    break;
                case 'Q':
                    toReturn = "--.-";
                    break;
                case 'R':
                    toReturn = ".-.";
                    break;
                case 'S':
                    toReturn = "...";
                    break;
                case 'T':
                    toReturn = "-";
                    break;
                case 'U':
                    toReturn = "..-";
                    break;
                case 'V':
                    toReturn = "...-";
                    break;
                case 'W':
                    toReturn = ".--";
                    break;
                case 'X':
                    toReturn = "-..-";
                    break;
                case 'Y':
                    toReturn = "-.--";
                    break;
                case 'Z':
                    toReturn = "--..";
                    break;
                case '0':
                    toReturn = "-----";
                    break;
                case '1':
                    toReturn = ".----";
                    break;
                case '2':
                    toReturn = "..---";
                    break;
                case '3':
                    toReturn = "...--";
                    break;
                case '4':
                    toReturn = "....-";
                    break;
                case '5':
                    toReturn = ".....";
                    break;
                case '6':
                    toReturn = "-....";
                    break;
                case '7':
                    toReturn = "--...";
                    break;
                case '8':
                    toReturn = "---..";
                    break;
                case '9':
                    toReturn = "----.";
                    break;
                case '.':
                    toReturn = ".-.-.-";
                    break;
                case ',':
                    toReturn = "--..--";
                    break;
                case '?':
                    toReturn = "..--..";
                    break;
                case '\'':
                    toReturn = ".----.";
                    break;
                case '!':
                    toReturn = "-.-.--";
                    break;
                case '/':
                    toReturn = "-..-.";
                    break;
                case '(':
                    toReturn = "-.--.";
                    break;
                case ')':
                    toReturn = "-.--.-";
                    break;
                case '&':
                    toReturn = ".-...";
                    break;
                case ':':
                    toReturn = "---...";
                    break;
                case ';':
                    toReturn = "-.-.-.";
                    break;
                case '+':
                    toReturn = ".-.-.";
                    break;
                case '-':
                    toReturn = "-....-";
                    break;
                case '_':
                    toReturn = "..--.-";
                    break;
                case '"':
                    toReturn = ".-..-.";
                    break;
                case '$':
                    toReturn = "...-..-";
                    break;
                case '@':
                    toReturn = ".--.-.";
                    break;
                default:
                    break;
            }
            return toReturn;
        }

        private void SendUnits(string quinary)
        {
            foreach (char aChar in quinary)
            {
                OnUnitTimeTick(this, new UnitTimeTickEventArgs(aChar == '1'));
                System.Threading.Thread.Sleep(_unitTimeMillis);
            }
        }
        public virtual void OnUnitTimeTick(object sender, UnitTimeTickEventArgs e) 
        {
            if (UnitTimeTick !=null) 
            {
                UnitTimeTick(sender,e);
            }
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopSending();
            }
            _isDisposed = true;
        }
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
            }
        }
        ~MorseCode()
        {
            Dispose();
        }
    }
}
