using System;
using F4Utils.Campaign.F4Structs;
using log4net;

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
            int curByte = 0;
            units = new Unit[numUnits];
            int i = 0;
            while( i < numUnits)
            {
                Unit thisUnit = null;
                short thisUnitType = BitConverter.ToInt16(bytes, curByte);
                curByte += 2;
                if (thisUnitType > 0)
                {
                    Falcon4EntityClassType classTableEntry = classTable[thisUnitType - 100];
                    if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_DOMAIN] == (byte)Classtable_Domains.DOMAIN_AIR)
                    {
                        if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_FLIGHT)
                        {
                            thisUnit = new Flight(bytes, ref curByte, version);
                        }
                        else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_SQUADRON)
                        {
                            thisUnit = new Squadron(bytes, ref curByte, version);
                        }
                        else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_PACKAGE)
                        {
                            thisUnit = new Package(bytes, ref curByte, version);
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
                            thisUnit = new Brigade(bytes, ref curByte, version);
                        }
                        else if (classTableEntry.vuClassData.classInfo_[(int)VuClassHierarchy.VU_TYPE] == (byte)Classtable_Types.TYPE_BATTALION)
                        {
                            thisUnit = new Battalion(bytes, ref curByte, version);
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
                            thisUnit = new TaskForce(bytes, ref curByte, version);
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
                    Log.ErrorFormat("unexpected unit type:{0} at location: {1}", thisUnitType, curByte);
                }
            }

            if (curByte < bytes.Length - 1)
            {
                throw new InvalidOperationException();
            }

        }
        protected static byte[] Expand(byte[] compressed, out short numUnits)
        {
            int curByte = 0;
            int cSize = BitConverter.ToInt32(compressed, curByte);
            curByte += 4;
            numUnits = BitConverter.ToInt16(compressed, curByte);
            curByte += 2;
            int uncompressedSize = BitConverter.ToInt32(compressed, curByte);
            curByte += 4;
            if (uncompressedSize == 0) return null;
            byte[] actualCompressed = new byte[compressed.Length - 10];
            Array.Copy(compressed, 10, actualCompressed, 0, actualCompressed.Length);
            byte[] uncompressed = null;
            uncompressed = Lzss.Codec.Decompress(actualCompressed, uncompressedSize);
            return uncompressed;
        }
    }
}