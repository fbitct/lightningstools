using System;
using F4Utils.Campaign.F4Structs;
using log4net;
using System.IO;
using System.Text;

namespace F4Utils.Campaign
{
    public class UniFile
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UniFile));
        #region Public Fields
        public Unit[] units;
        #endregion

        protected int _version = 0;

        protected UniFile()
            : base()
        {
        }
        public UniFile(byte[] compressed, int version, Falcon4EntityClassType[] classTable)
            : this()
        {
            _version = version;
            short numUnits = 0;
            byte[] expanded = Expand(compressed, out numUnits);
            if (expanded != null) Decode(expanded, version, numUnits, classTable);
        }
        protected void Decode(byte[] bytes, int version, short numUnits, Falcon4EntityClassType[] classTable)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                units = new Unit[numUnits];
                int i = 0;
                while (i < numUnits)
                {
                    Unit thisUnit = null;
                    short thisUnitType = reader.ReadInt16();
                    if (thisUnitType > 0)
                    {
                        Falcon4EntityClassType classTableEntry = classTable[thisUnitType - 100];
                        if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_DOMAIN] == (byte)Classtable_Domains.DOMAIN_AIR)
                        {
                            if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_FLIGHT)
                            {
                                thisUnit = new Flight(stream, version);
                            }
                            else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_SQUADRON)
                            {
                                thisUnit = new Squadron(stream, version);
                            }
                            else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_PACKAGE)
                            {
                                thisUnit = new Package(stream, version);
                            }
                            else
                            {
                                thisUnit = null;
                            }
                        }
                        else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_DOMAIN] == (byte)Classtable_Domains.DOMAIN_LAND)
                        {
                            if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_BRIGADE)
                            {
                                thisUnit = new Brigade(stream, version);
                            }
                            else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_BATTALION)
                            {
                                thisUnit = new Battalion(stream, version);
                            }
                            else
                            {
                                thisUnit = null;
                            }

                        }
                        else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_DOMAIN] == (byte)Classtable_Domains.DOMAIN_SEA)
                        {
                            if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_TASKFORCE)
                            {
                                thisUnit = new TaskForce(stream, version);
                            }
                            else
                            {
                                thisUnit = null;
                            }
                        }
                        else
                        {
                            thisUnit = null;
                        }
                    }
                    if (thisUnit != null)
                    {
                        units[i] = thisUnit;
                        i++;
                    }
                    else
                    {
                        Log.ErrorFormat("unexpected unit type:{0} at location: {1}", thisUnitType, stream.Position);
                    }
                }

                if (stream.Position < stream.Length - 1)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        protected static byte[] Expand(byte[] compressed, out short numUnits)
        {
            using (var stream = new MemoryStream(compressed))
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                int cSize = reader.ReadInt32();
                numUnits = reader.ReadInt16();
                int uncompressedSize = reader.ReadInt32();
                if (uncompressedSize == 0) return null;
                byte[] actualCompressed = reader.ReadBytes((int)(compressed.Length - 10));
                byte[] uncompressed = null;
                uncompressed = Lzss.Codec.Decompress(actualCompressed, uncompressedSize);
                return uncompressed;
            }
        }
    }
}