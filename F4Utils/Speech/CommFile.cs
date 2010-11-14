using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace F4Utils.Speech
{
    public class CommFile
    {
        public CommFileHeaderRecord[] headers;
        private CommFile()
        {
        }
        public static CommFile LoadFromBinary(string commFilePath)
        {
            FileInfo fi = new FileInfo(commFilePath);
            if (!fi.Exists) throw new FileNotFoundException(commFilePath);

            CommFile commFile = new CommFile();
            byte[] bytes = new byte[fi.Length];
            using (FileStream fs = new FileStream(commFilePath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes, 0, (int)fi.Length);
            }


            CommFileHeaderRecord firstHeader = ReadHeader(bytes, 0);
            uint numHeaders = firstHeader.commOffset / 14;
            commFile.headers = new CommFileHeaderRecord[numHeaders];
            commFile.headers[0] = firstHeader;
            for (int i=1;i<numHeaders;i++)
            {
                commFile.headers[i] = ReadHeader(bytes, i);
            }

            return commFile;
        }

        private static CommFileHeaderRecord ReadHeader(byte[] bytes, int headerNum)
        {
            int pCommHeader = headerNum * 14;
            CommFileHeaderRecord thisHeader = new CommFileHeaderRecord();
            thisHeader.commHdrNbr = BitConverter.ToUInt16(bytes, pCommHeader);
            pCommHeader += 2;
            thisHeader.warp = BitConverter.ToUInt16(bytes, pCommHeader);
            pCommHeader += 2;
            thisHeader.priority = bytes[pCommHeader];
            pCommHeader++;
            thisHeader.positionElement = (sbyte)bytes[pCommHeader];
            pCommHeader++;
            thisHeader.bullseye = BitConverter.ToInt16(bytes, pCommHeader);
            pCommHeader += 2;
            thisHeader.totalElements = bytes[pCommHeader];
            pCommHeader++;
            thisHeader.totalEvals = bytes[pCommHeader];
            pCommHeader++;
            thisHeader.commOffset = BitConverter.ToUInt32(bytes, pCommHeader);
            pCommHeader += 4;
            thisHeader.data = new CommFileDataRecord[thisHeader.totalElements];
            uint pData = thisHeader.commOffset;
            for (int i = 0; i < thisHeader.totalElements; i++)
            {
                CommFileDataRecord thisData = new CommFileDataRecord();
                thisData.fragIdOrEvalId = BitConverter.ToInt16(bytes, (int)pData);
                pData += 2;
                thisHeader.data[i] = thisData;
            }
            return thisHeader;
        }
        public void FixupOffsets()
        {
            if (this.headers == null) return;
            uint offset = (uint)this.headers.Length *14;
            for (ushort i = 0; i < this.headers.Length; i++)
            {
                CommFileHeaderRecord thisHeader = this.headers[i];
                thisHeader.commOffset = offset;
                this.headers[i] = thisHeader;
                offset += (uint)2 * thisHeader.totalElements;
            }
        }
        public void SaveAsBinary(string commFilePath)
        {
            FileInfo fi = new FileInfo(commFilePath);

            using (FileStream fs = new FileStream(commFilePath, FileMode.Create))
            {
                //write headers
                if (this.headers != null)
                {
                    Array.Sort(this.headers, delegate(CommFileHeaderRecord hdr1, CommFileHeaderRecord hdr2)
                    {
                        return hdr1.commHdrNbr.CompareTo(hdr2.commHdrNbr);
                    });
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        CommFileHeaderRecord thisHeader = this.headers[i];
                        fs.Write(BitConverter.GetBytes(thisHeader.commHdrNbr), 0, 2);
                        fs.Write(BitConverter.GetBytes(thisHeader.warp), 0, 2);
                        fs.WriteByte(thisHeader.priority);
                        fs.WriteByte((byte)thisHeader.positionElement);
                        fs.Write(BitConverter.GetBytes(thisHeader.bullseye), 0, 2);
                        fs.WriteByte((byte)thisHeader.totalElements);
                        fs.WriteByte((byte)thisHeader.totalEvals);
                        fs.Write(BitConverter.GetBytes(thisHeader.commOffset), 0, 4);
                    }

                    //write data
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        CommFileHeaderRecord thisHeader = this.headers[i];
                        if (thisHeader.data != null)
                        {
                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                CommFileDataRecord thisDataRecord = thisHeader.data[j];
                                fs.Write(BitConverter.GetBytes(thisDataRecord.fragIdOrEvalId), 0, 2);
                            }
                        }
                    }
                }
                fs.Flush();
                fs.Close();
            }
        }
        public void SaveAsXml(string commXmlFilePath)
        {
            FileInfo fi = new FileInfo(commXmlFilePath);
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.NewLineOnAttributes = false;
            xws.OmitXmlDeclaration = false;
            xws.IndentChars = "\t";
            xws.CheckCharacters = true;
            xws.Encoding = Encoding.UTF8;


            using (FileStream fs = new FileStream(commXmlFilePath, FileMode.Create))
            using (XmlWriter xw = XmlTextWriter.Create(fs, xws))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("CommFile");
                //xw.WriteStartAttribute("numComms");
                //xw.WriteValue(this.headers.Length);
                //xw.WriteEndAttribute();
                if (this.headers != null)
                {
                    Array.Sort(this.headers, delegate(CommFileHeaderRecord hdr1, CommFileHeaderRecord hdr2)
                    {
                        return hdr1.commHdrNbr.CompareTo(hdr2.commHdrNbr);
                    });
                    for (int i = 0; i < this.headers.Length; i++)
                    {
                        CommFileHeaderRecord thisHeader = this.headers[i];
                        if (thisHeader.data != null )
                        {
                            xw.WriteStartElement("Comm");
                            xw.WriteStartAttribute("id");
                            xw.WriteValue(thisHeader.commHdrNbr);
                            xw.WriteEndAttribute();
                            xw.WriteStartAttribute("warp");
                            xw.WriteValue(thisHeader.warp);
                            xw.WriteEndAttribute();
                            xw.WriteStartAttribute("priority");
                            xw.WriteValue(thisHeader.priority);
                            xw.WriteEndAttribute();
                            xw.WriteStartAttribute("positionElement");
                            xw.WriteValue(thisHeader.positionElement);
                            xw.WriteEndAttribute();
                            xw.WriteStartAttribute("bullseye");
                            xw.WriteValue(thisHeader.bullseye);
                            xw.WriteEndAttribute();
                            //xw.WriteStartAttribute("totalElements");
                            //xw.WriteValue(thisHeader.totalElements);
                            //xw.WriteEndAttribute();
                            //xw.WriteStartAttribute("totalEvals");
                            //xw.WriteValue(thisHeader.totalEvals);
                            //xw.WriteEndAttribute();
                            
                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                CommFileDataRecord thisDataRecord = thisHeader.data[j];
                                xw.WriteStartElement("CommElement");
                                xw.WriteStartAttribute("index");
                                xw.WriteValue(j);
                                xw.WriteEndAttribute();
                                if (thisDataRecord.fragIdOrEvalId < 0)
                                {
                                    xw.WriteStartAttribute("evalId");
                                    xw.WriteValue(-thisDataRecord.fragIdOrEvalId);
                                    xw.WriteEndAttribute();
                                }
                                else
                                {
                                    xw.WriteStartAttribute("fragId");
                                    xw.WriteValue(thisDataRecord.fragIdOrEvalId);
                                    xw.WriteEndAttribute();
                                }
                                xw.WriteEndElement();
                            }
                            xw.WriteEndElement();
                        }
                    }
                }
                xw.WriteEndElement();
                xw.WriteEndDocument();
                xw.Flush();
                fs.Flush();
                xw.Close();
            }
        }
        public static CommFile LoadFromXml(string commXmlFilePath)
        {
            CommFile toReturn = new CommFile();
            CommFileHeaderRecord[] headers = new CommFileHeaderRecord[0];
            using (FileStream fs = new FileStream(commXmlFilePath, FileMode.Open))
            using (XmlReader xr = new XmlTextReader(fs))
            {
                CommFileHeaderRecord thisHeader = new CommFileHeaderRecord();
                CommFileDataRecord[] dataRecords = new CommFileDataRecord[0];
                while (xr.Read())
                {
                    long val = 0;
                    string attribValString = null;
                    bool parsed =false;
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "CommFile")
                    {
                        //attribValString = xr.GetAttribute("numComms");
                        //parsed = Int64.TryParse(attribValString, out val);
                        //if (parsed)
                        //{
                        // headers = new CommFileHeaderRecord[val];
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @numComms attribute in /CommFile root element.", commXmlFilePath));
                        //}
                        headers = new CommFileHeaderRecord[0];
                    }
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "Comm")
                    {
                        thisHeader = new CommFileHeaderRecord();

                        attribValString = xr.GetAttribute("id");
                        parsed = Int64.TryParse(attribValString, out val);
                        if (parsed)
                        {
                            thisHeader.commHdrNbr = (ushort)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @id attribute in /CommFile/Comm element.", commXmlFilePath));
                        }

                        attribValString = xr.GetAttribute("warp");
                        parsed = Int64.TryParse(attribValString, out val);
                        if (parsed)
                        {
                            thisHeader.warp = (ushort)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @warp attribute in /CommFile/Comm element.", commXmlFilePath));
                        }

                        attribValString = xr.GetAttribute("priority");
                        parsed = Int64.TryParse(attribValString, out val);
                        if (parsed)
                        {
                            thisHeader.priority = (byte)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @priority attribute in /CommFile/Comm element.", commXmlFilePath));
                        }
                        
                        attribValString = xr.GetAttribute("positionElement");
                        parsed = Int64.TryParse(attribValString, out val);
                        if (parsed)
                        {
                            thisHeader.positionElement = (sbyte)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @positionElement attribute in /CommFile/Comm element.", commXmlFilePath));
                        }

                        attribValString = xr.GetAttribute("bullseye");
                        parsed = Int64.TryParse(attribValString, out val);
                        if (parsed)
                        {
                            thisHeader.bullseye = (short)val;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @bullseye attribute in /CommFile/Comm element.", commXmlFilePath));
                        }

                        //attribValString = xr.GetAttribute("totalElements");
                        //parsed = Int64.TryParse(attribValString, out val);
                        //if (parsed)
                        //{
                        //    thisHeader.totalElements = (byte)val;
                        //    dataRecords = new CommFileDataRecord[thisHeader.totalElements];
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @totalElements attribute in /CommFile/Comm element.", commXmlFilePath));
                        //}
                        dataRecords = new CommFileDataRecord[0];

                        //attribValString = xr.GetAttribute("totalEvals");
                        //parsed = Int64.TryParse(attribValString, out val);
                        //if (parsed)
                        //{
                        //    thisHeader.totalEvals = (byte)val;
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @totalEvals attribute in /CommFile/Comm element.", commXmlFilePath));
                        //}

                    }
                    else if (xr.NodeType == XmlNodeType.Element && (xr.Name == "CommElement"))
                    {
                        CommFileDataRecord thisElementDataRecord = new CommFileDataRecord();
                        attribValString = xr.GetAttribute("fragId");
                        if (!string.IsNullOrEmpty(attribValString))
                        {
                            parsed = Int64.TryParse(attribValString, out val);
                            if (parsed)
                            {
                                thisElementDataRecord.fragIdOrEvalId = (short)val;
                            }
                            else
                            {
                                throw new IOException(string.Format("Could not parse {0}, bad or missing @fragId attribute in /CommFile/Comm/CommElement element.", commXmlFilePath));
                            }
                        }
                        else
                        {
                            attribValString = xr.GetAttribute("evalId");
                            if (string.IsNullOrEmpty(attribValString))
                            {
                                throw new IOException(string.Format("Could not parse {0}, missing @fragId or @evalId attribute in /CommFile/Comm/CommElement element.", commXmlFilePath));
                            }
                            parsed = Int64.TryParse(attribValString, out val);
                            if (parsed)
                            {
                                thisHeader.totalEvals++;
                                thisElementDataRecord.fragIdOrEvalId = (short)-val;
                            }
                            else
                            {
                                throw new IOException(string.Format("Could not parse {0}, bad or missing @evalId attribute in /CommFile/Comm/CommElement element.", commXmlFilePath));
                            }

                        }

                        attribValString = xr.GetAttribute("index");
                        parsed = Int64.TryParse(attribValString, out val);
                        int index = 0;
                        if (parsed)
                        {
                            index = (int)val;
                            if (index > dataRecords.Length -1)
                            {
                                //throw new IOException(string.Format("Could not parse {0}, @index attribute value in /CommFile/Comm/CommElement element exceeds (@totalElements-1) value declared in parent /CommFile/Comm element.", commXmlFilePath));
                                Array.Resize(ref dataRecords, index + 1);
                            }
                            dataRecords[index] = thisElementDataRecord;
                            thisHeader.totalElements++;
                        }
                        else
                        {
                            throw new IOException(string.Format("Could not parse {0}, bad or missing @index attribute in /CommFile/Comm/CommElement element.", commXmlFilePath));
                        }
                    }
                    else if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "Comm")
                    {
                        thisHeader.data = dataRecords;
                        if (thisHeader.commHdrNbr > headers.Length-1)
                        {
                            //throw new IOException(string.Format("Could not parse {0}, @id attribute value in /CommFile/Comm element exceeds (@numComms-1) attribute value declared in /CommFile root element.", commXmlFilePath));
                            Array.Resize(ref headers, thisHeader.commHdrNbr + 1);
                        }
                        headers[thisHeader.commHdrNbr] = thisHeader;
                    }
                }
            }
            toReturn.headers = headers;
            toReturn.FixupOffsets();
            return toReturn;
        }
    }

}
