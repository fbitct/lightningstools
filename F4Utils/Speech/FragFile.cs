using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace F4Utils.Speech
{
    public class FragFile
    {
        public FragFileHeaderRecord[] headers;
        private FragFile()
        {
        }
        public static FragFile LoadFromBinary(string fragFilePath)
        {
            FileInfo fi = new FileInfo(fragFilePath);
            if (!fi.Exists) throw new FileNotFoundException(fragFilePath);

            FragFile fragFile = new FragFile();
            byte[] bytes = new byte[fi.Length];
            using (FileStream fs = new FileStream(fragFilePath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes, 0, (int)fi.Length);
            }

            int fileLen = bytes.Length;
            FragFileHeaderRecord thisHeader = ReadFragHeader(bytes, 0);
            int numFragHeaders = (int)thisHeader.fragOffset/8;
            fragFile.headers = new FragFileHeaderRecord[numFragHeaders];
            fragFile.headers[0] = thisHeader;

            for (int i = 1; i < numFragHeaders;i++ )
            {
                fragFile.headers[i] = ReadFragHeader(bytes, i);
            }

            return fragFile;
        }

        private static FragFileHeaderRecord ReadFragHeader(byte[] bytes, int hdrNum)
        {
            int pFragHeader = hdrNum * 8;
            FragFileHeaderRecord thisHeader = new FragFileHeaderRecord();
            thisHeader.fragHdrNbr = BitConverter.ToUInt16(bytes, pFragHeader) ;
            pFragHeader += 2;
            thisHeader.totalSpeakers = BitConverter.ToUInt16(bytes, pFragHeader);
            pFragHeader += 2;
            thisHeader.fragOffset = BitConverter.ToUInt32(bytes, pFragHeader);
            pFragHeader += 4;

            thisHeader.data = new FragFileDataRecord[thisHeader.totalSpeakers];

            uint pSpeakerData = thisHeader.fragOffset;
            int thisDataCount = 0;
            for (int i = 0; i < thisHeader.totalSpeakers; i++)
            {
                FragFileDataRecord thisData = new FragFileDataRecord();
                thisData.speaker = BitConverter.ToUInt16(bytes, (int)pSpeakerData);
                pSpeakerData += 2;
                thisData.fileNbr = BitConverter.ToUInt16(bytes, (int)pSpeakerData);
                pSpeakerData += 2;
                thisHeader.data[thisDataCount] = thisData;
                thisDataCount++;
            }
            return thisHeader;
        }
        public void FixupOffsets()
        {
            if (this.headers == null) return;
            uint offset = (uint)this.headers.Length * 8;

            for (ushort i = 0; i < this.headers.Length; i++)
            {
                FragFileHeaderRecord thisHeader = this.headers[i];
                if (thisHeader.totalSpeakers == 0)
                {
                    thisHeader.fragOffset = 1000;
                }
                else
                {
                    thisHeader.fragOffset = offset;
                }
                this.headers[i] = thisHeader;
                offset += (uint)4 * thisHeader.totalSpeakers;
            }
        }
        public void SaveAsBinary(string fragFilePath)
        {
            FileInfo fi = new FileInfo(fragFilePath);

            using (FileStream fs = new FileStream(fragFilePath, FileMode.Create))
            {
                //write headers
                if (this.headers != null)
                {
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        FragFileHeaderRecord thisHeader = this.headers[i];
                        fs.Write(BitConverter.GetBytes(thisHeader.fragHdrNbr), 0, 2);
                        fs.Write(BitConverter.GetBytes(thisHeader.totalSpeakers), 0, 2);
                        fs.Write(BitConverter.GetBytes(thisHeader.fragOffset), 0, 4);
                    }

                    //write data
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        FragFileHeaderRecord thisHeader = this.headers[i];
                        if (thisHeader.data != null)
                        {

                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                FragFileDataRecord thisDataRecord = thisHeader.data[j];
                                fs.Write(BitConverter.GetBytes(thisDataRecord.speaker), 0, 2);
                                fs.Write(BitConverter.GetBytes(thisDataRecord.fileNbr), 0, 2);
                            }
                        }
                    }
                }
                fs.Flush();
                fs.Close();
            }
        }
        public void SaveAsXml(string fragFilePath)
        {
            FileInfo fi = new FileInfo(fragFilePath);
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.NewLineOnAttributes = false;
            xws.OmitXmlDeclaration = false;
            xws.IndentChars = "\t";
            xws.CheckCharacters = true;
            xws.Encoding = Encoding.UTF8;

            using (FileStream fs = new FileStream(fragFilePath, FileMode.Create))
            using (XmlWriter xw = XmlTextWriter.Create(fs, xws))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("FragFile");
                //xw.WriteStartAttribute("numFrags");
                //xw.WriteValue(this.headers.Length);
                //xw.WriteEndAttribute();
                if (this.headers != null)
                {
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        xw.WriteStartElement("Frag");

                        FragFileHeaderRecord thisHeader = this.headers[i];
                        xw.WriteStartAttribute("id");
                        xw.WriteValue(thisHeader.fragHdrNbr);
                        xw.WriteEndAttribute();
                        //xw.WriteStartAttribute("numSpeakers");
                        //xw.WriteValue(thisHeader.totalSpeakers);
                        //xw.WriteEndAttribute();
                        if (thisHeader.data != null)
                        {
                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                FragFileDataRecord thisDataRecord = thisHeader.data[j];
                                xw.WriteStartElement("Speaker");
                                xw.WriteStartAttribute("voice");
                                xw.WriteValue(thisDataRecord.speaker);
                                xw.WriteEndAttribute();
                                xw.WriteStartAttribute("tlkId");
                                xw.WriteValue(thisDataRecord.fileNbr);
                                xw.WriteEndAttribute();
                                xw.WriteEndElement();
                            }
                        }
                        xw.WriteEndElement();
                    }
                }
                xw.WriteEndElement();
                xw.WriteEndDocument();
                xw.Flush();
                fs.Flush();
                xw.Close();
            }
        }
        public static FragFile LoadFromXml(string fragXmlFilePath)
        {
            FragFile toReturn = new FragFile();
            FragFileHeaderRecord[] headers = new FragFileHeaderRecord[0];
            FragFileHeaderRecord thisHeader = new FragFileHeaderRecord();
            bool parsed = false;
            long val = 0;
            using (FileStream fs = new FileStream(fragXmlFilePath, FileMode.Open))
            using (XmlReader xr = new XmlTextReader(fs))
            {
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "FragFile")
                    {
                        //string numFragsString= xr.GetAttribute("numFrags");
                        //parsed = Int64.TryParse(numFragsString, out val);
                        //if (parsed)
                        //{
                        //  headers = new FragFileHeaderRecord[val];
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @numFrags attribute in /FragFile root element.", fragXmlFilePath));
                        //}
                        headers = new FragFileHeaderRecord[0];
                    }
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "Frag")
                    {
                        thisHeader = new FragFileHeaderRecord();

                        string fragIdString=xr.GetAttribute("id");
                        parsed = Int64.TryParse(fragIdString, out val);
                        if (parsed)
                        {
                            thisHeader.fragHdrNbr = (ushort)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @id attribute in /FragFile/Frag element.", fragXmlFilePath));
                        }
                        //string totalSpeakersString= xr.GetAttribute("numSpeakers");
                        //parsed = Int64.TryParse(totalSpeakersString, out val);
                        //if (parsed)
                        //{
                        //    thisHeader.data = new FragFileDataRecord[thisHeader.totalSpeakers];
                        //    thisHeader.totalSpeakers = (ushort)val;
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @numSpeakers attribute in /FragFile/Frag element.", fragXmlFilePath));
                        //}
                        thisHeader.data = new FragFileDataRecord[0];
                    }
                    else if (xr.NodeType == XmlNodeType.Element && xr.Name == "Speaker")
                    {
                        FragFileDataRecord data = new FragFileDataRecord();
                        string voiceNumString = xr.GetAttribute("voice");
                        parsed = Int64.TryParse(voiceNumString, out val);
                        if (parsed)
                        {
                            data.speaker = (ushort)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @voice attribute in /FragFile/Frag/Speaker element.", fragXmlFilePath));
                        }
                        string tlkId = xr.GetAttribute("tlkId");
                        parsed = Int64.TryParse(tlkId, out val);
                        if (parsed)
                        {
                            data.fileNbr = (ushort)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @tlkId attribute in /FragFile/Frag/Speaker element.", fragXmlFilePath));
                        }
                        Array.Resize(ref thisHeader.data, thisHeader.data.Length + 1); 
                        thisHeader.data[thisHeader.data.Length-1] = data;
                        thisHeader.totalSpeakers++;
                    }
                    else if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "Frag")
                    {
                        if (thisHeader.fragHdrNbr > headers.Length-1)
                        {
                            //throw new IOException(string.Format("Could not parse {0}, @id attribute value in /FragFile/Frag element exceeds (@numFrags-1) declared in the /FragFile root element.", fragXmlFilePath));
                            Array.Resize(ref headers, thisHeader.fragHdrNbr + 1);
                        }
                        //else
                        //{
                            if (thisHeader.totalSpeakers > 0)
                            {
                                headers[thisHeader.fragHdrNbr] = thisHeader;
                            }
                        //}
                    }
                }
            }
            toReturn.headers = headers;
            toReturn.FixupOffsets();
            return toReturn;
        }
    }
}
