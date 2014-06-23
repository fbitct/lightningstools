﻿using F4Utils;
using F4Utils.Terrain.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    internal interface ITheaterDotTdfFileReader
    {
        TheaterDotTdfFileInfo ReadTheaterDotTdfFile(string path);
    }
    internal class TheaterDotTdfFileReader:ITheaterDotTdfFileReader
    {
        private ITokenJoiner _tokenJoiner;
        public TheaterDotTdfFileReader(ITokenJoiner tokenJoiner = null) 
        {
            _tokenJoiner = tokenJoiner?? new TokenJoiner();
        }

        public TheaterDotTdfFileInfo ReadTheaterDotTdfFile(string path)
        {
            if (String.IsNullOrEmpty(path)) return null;

            var basePathFI = new FileInfo(path);
            if (!basePathFI.Exists) return null;

            var toReturn = new TheaterDotTdfFileInfo();
            using (var fs = new FileStream(path, FileMode.Open))
            using (var sw = new StreamReader(fs))
            {
                while (!sw.EndOfStream)
                {
                    var thisLine = sw.ReadLine();
                    var thisLineTokens = Common.Strings.Util.Tokenize(thisLine);
                    if (thisLineTokens.Count > 0)
                    {
                        if (thisLineTokens[0].ToLower() == "name")
                        {
                            toReturn.theaterName = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "desc")
                        {
                            toReturn.theaterDesc = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "bitmap")
                        {
                            toReturn.bitmap = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "campaigndir")
                        {
                            toReturn.campaignDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "terraindir")
                        {
                            toReturn.terrainDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "artdir")
                        {
                            toReturn.artDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "moviedir")
                        {
                            toReturn.movieDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "uisounddir")
                        {
                            toReturn.uiSoundDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "objectdir")
                        {
                            toReturn.objectDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "misctexdir")
                        {
                            toReturn.miscTextDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "3ddatadir")
                        {
                            toReturn.ThreeDeeDataDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "mintacan")
                        {
                            toReturn.minTacan = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "sounddir")
                        {
                            toReturn.soundDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                        else if (thisLineTokens[0].ToLower() == "acmidir")
                        {
                            toReturn.acmiDir = _tokenJoiner.JoinTokens(thisLineTokens, true);
                        }
                    }
                }
            }
            return toReturn;
        }
    }
}
