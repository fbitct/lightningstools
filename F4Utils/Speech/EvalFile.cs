﻿using System;
using System.IO;
using System.Text;
using System.Xml;

namespace F4Utils.Speech
{
    public class EvalFile
    {
        public EvalFileHeaderRecord[] headers;

        private EvalFile()
        {
        }

        public static EvalFile LoadFromBinary(string evalFilePath)
        {
            var fi = new FileInfo(evalFilePath);
            if (!fi.Exists) throw new FileNotFoundException(evalFilePath);

            var evalFile = new EvalFile();
            var bytes = new byte[fi.Length];
            using (var fs = new FileStream(evalFilePath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes, 0, (int) fi.Length);
            }

            int fileLen = bytes.Length;
            EvalFileHeaderRecord thisHeader = ReadHeader(bytes, 0);
            uint numEvals = thisHeader.evalOffset/8;
            evalFile.headers = new EvalFileHeaderRecord[numEvals];
            evalFile.headers[0] = thisHeader;
            for (int i = 1; i < numEvals; i++)
            {
                evalFile.headers[i] = ReadHeader(bytes, i);
            }

            return evalFile;
        }

        private static EvalFileHeaderRecord ReadHeader(byte[] bytes, int recordNum)
        {
            int pEvalHeader = recordNum*8;

            var thisHeader = new EvalFileHeaderRecord();
            thisHeader.evalHdrNbr = BitConverter.ToUInt16(bytes, pEvalHeader);
            pEvalHeader += 2;
            thisHeader.numEvals = BitConverter.ToUInt16(bytes, pEvalHeader);
            pEvalHeader += 2;
            thisHeader.evalOffset = BitConverter.ToUInt32(bytes, pEvalHeader);
            pEvalHeader += 4;

            thisHeader.data = new EvalFileDataRecord[thisHeader.numEvals];
            var pEvalData = (int) thisHeader.evalOffset;
            for (int i = 0; i < thisHeader.numEvals; i++)
            {
                var thisData = new EvalFileDataRecord();
                thisData.evalElem = BitConverter.ToInt16(bytes, pEvalData);
                pEvalData += 2;
                thisData.fragNbr = BitConverter.ToUInt16(bytes, pEvalData);
                pEvalData += 2;
                thisHeader.data[i] = thisData;
            }
            return thisHeader;
        }

        public void FixupOffsets()
        {
            uint offset = (uint) headers.Length*8;
            for (ushort i = 0; i < headers.Length; i++)
            {
                EvalFileHeaderRecord thisHeader = headers[i];
                thisHeader.numEvals = (ushort) (thisHeader.data != null ? thisHeader.data.Length : 0);
                thisHeader.evalOffset = offset;
                headers[i] = thisHeader;
                offset += (uint) (4*thisHeader.numEvals);
            }
        }

        public void SaveAsBinary(string evalFilePath)
        {
            var fi = new FileInfo(evalFilePath);

            using (var fs = new FileStream(evalFilePath, FileMode.Create))
            {
                //write headers
                if (headers != null)
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        EvalFileHeaderRecord thisHeader = headers[i];
                        fs.Write(BitConverter.GetBytes(thisHeader.evalHdrNbr), 0, 2);
                        fs.Write(BitConverter.GetBytes(thisHeader.numEvals), 0, 2);
                        fs.Write(BitConverter.GetBytes(thisHeader.evalOffset), 0, 4);
                    }

                    //write data
                    for (int i = 0; i < headers.Length; i++)
                    {
                        EvalFileHeaderRecord thisHeader = headers[i];
                        if (thisHeader.data != null)
                        {
                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                EvalFileDataRecord thisDataRecord = thisHeader.data[j];
                                fs.Write(BitConverter.GetBytes(thisDataRecord.evalElem), 0, 2);
                                fs.Write(BitConverter.GetBytes(thisDataRecord.fragNbr), 0, 2);
                            }
                        }
                    }
                }
                fs.Flush();
                fs.Close();
            }
        }

        public void SaveAsXml(string evalXmlFilePath)
        {
            var fi = new FileInfo(evalXmlFilePath);
            var xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.NewLineOnAttributes = false;
            xws.OmitXmlDeclaration = false;
            xws.IndentChars = "\t";
            xws.CheckCharacters = true;
            xws.Encoding = Encoding.UTF8;

            using (var fs = new FileStream(evalXmlFilePath, FileMode.Create))
            using (XmlWriter xw = XmlWriter.Create(fs, xws))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("EvalFile");
                //xw.WriteStartAttribute("numEvals");
                //xw.WriteValue(this.headers.Length);
                //xw.WriteEndAttribute();
                if (headers != null)
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        EvalFileHeaderRecord thisHeader = headers[i];
                        if (thisHeader.data != null)
                        {
                            xw.WriteStartElement("Eval"); //<Eval>
                            xw.WriteStartAttribute("id");
                            xw.WriteValue(thisHeader.evalHdrNbr);
                            xw.WriteEndAttribute();

                            //xw.WriteStartAttribute("numElements");
                            //xw.WriteValue(thisHeader.numEvals);
                            //xw.WriteEndAttribute();

                            for (int j = 0; j < thisHeader.data.Length; j++)
                            {
                                EvalFileDataRecord thisDataRecord = thisHeader.data[j];
                                xw.WriteStartElement("Element"); //<Element>
                                xw.WriteStartAttribute("evalElem");
                                xw.WriteValue(thisDataRecord.evalElem);
                                xw.WriteEndAttribute();
                                xw.WriteStartAttribute("fragId");
                                xw.WriteValue(thisDataRecord.fragNbr);
                                xw.WriteEndAttribute();
                                xw.WriteEndElement(); //</Element>
                            }
                            xw.WriteEndElement(); //<Eval>
                        }
                    }
                }
                xw.WriteEndElement(); //</EvalFile>
                xw.WriteEndDocument();
                xw.Flush();
                fs.Flush();
                xw.Close();
            }
        }

        public static EvalFile LoadFromXml(string evalXmlFilePath)
        {
            var toReturn = new EvalFile();
            var headers = new EvalFileHeaderRecord[0];
            using (var fs = new FileStream(evalXmlFilePath, FileMode.Open))
            using (XmlReader xr = new XmlTextReader(fs))
            {
                var thisHeader = new EvalFileHeaderRecord();
                var dataRecords = new EvalFileDataRecord[0];
                long val = 0;
                bool parsed = false;
                long thisEvalElement = 0;

                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "EvalFile")
                    {
                        //string numEvalsString = xr.GetAttribute("numEvals");
                        //parsed = Int64.TryParse(numEvalsString, out val);
                        //if (parsed)
                        //{
                        //    headers = new EvalFileHeaderRecord[val];
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @numEvals attribute in /EvalFile root element.", evalXmlFilePath));
                        //}
                        headers = new EvalFileHeaderRecord[0];
                    }
                    if (xr.NodeType == XmlNodeType.Element && xr.Name == "Eval")
                    {
                        thisHeader = new EvalFileHeaderRecord();
                        string evalIdString = xr.GetAttribute("id");
                        parsed = Int64.TryParse(evalIdString, out val);
                        if (parsed)
                        {
                            thisHeader.evalHdrNbr = (ushort) val;
                        }
                        else
                        {
                            throw new IOException(
                                string.Format(
                                    "Could not parse {0}, bad or missing @id attribute in /EvalFile/Eval element.",
                                    evalXmlFilePath));
                        }

                        //string numEvalsString = xr.GetAttribute("numElements");
                        //parsed = Int64.TryParse(numEvalsString, out val);
                        //if (parsed)
                        //{
                        //    dataRecords = new EvalFileDataRecord[val];
                        //    thisEvalElement = 0;
                        //}
                        //else
                        //{
                        //    throw new IOException(string.Format("Could not parse {0}, bad or missing @numElements attribute in /EvalFile/Eval element.", evalXmlFilePath));
                        //}
                        dataRecords = new EvalFileDataRecord[0];
                        thisEvalElement = 0;
                    }
                    else if (xr.NodeType == XmlNodeType.Element && xr.Name == "Element")
                    {
                        var thisDataRecord = new EvalFileDataRecord();
                        string evalElem = xr.GetAttribute("evalElem");
                        parsed = Int64.TryParse(evalElem, out val);
                        if (parsed)
                        {
                            thisDataRecord.evalElem = (short) val;
                        }
                        else
                        {
                            throw new IOException(
                                string.Format(
                                    "Could not parse {0}, bad or missing @evalElem attribute in /EvalFile/Eval/Element element.",
                                    evalXmlFilePath));
                        }

                        string fragNbr = xr.GetAttribute("fragId");
                        parsed = Int64.TryParse(fragNbr, out val);
                        if (parsed)
                        {
                            thisDataRecord.fragNbr = (ushort) val;
                        }
                        else
                        {
                            throw new IOException(
                                string.Format(
                                    "Could not parse {0}, bad or missing @fragId attribute in /EvalFile/Eval/Element element.",
                                    evalXmlFilePath));
                        }

                        if (thisEvalElement > dataRecords.Length - 1)
                        {
                            //throw new IOException(string.Format("Could not parse {0}, number of /EvalFile/Eval/Element elements exceeds @numElements attribute value declared in parent /EvalFile/Eval tag.", evalXmlFilePath));
                            Array.Resize(ref dataRecords, (int) thisEvalElement + 1);
                        }
                        //else
                        //{
                        dataRecords[thisEvalElement] = thisDataRecord;
                        thisEvalElement++;
                        //}
                    }
                    else if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "Eval")
                    {
                        thisHeader.data = dataRecords;
                        if (thisHeader.evalHdrNbr > headers.Length - 1)
                        {
                            //throw new IOException(string.Format("Could not parse {0}, @id attribute in /EvalFile/Eval element exceeds (@numEvals-1) attribute value declared in /EvalFile root element.", evalXmlFilePath));
                            Array.Resize(ref headers, thisHeader.evalHdrNbr + 1);
                        }
                        //else
                        //{
                        headers[thisHeader.evalHdrNbr] = thisHeader;
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