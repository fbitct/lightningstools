using System;
using System.IO;
using F4Utils.Campaign.F4Structs;
using System.Text;

namespace F4Utils.Campaign
{
    public static class ClassTable
    {
        public static Falcon4EntityClassType[] ReadClassTable(string classTableFilePath)
        {
            if (classTableFilePath == null) throw new ArgumentNullException("classTableFilePath");
            FileInfo ctFileInfo = new FileInfo(classTableFilePath);
            if (!ctFileInfo.Exists) throw new FileNotFoundException(classTableFilePath);
            byte[] bytes = new byte[ctFileInfo.Length];
            using (var stream = new FileStream(classTableFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen:true))
            {
                short numEntities = reader.ReadInt16();
                Falcon4EntityClassType[] classTable = new Falcon4EntityClassType[numEntities];
                for (int i = 0; i < numEntities; i++)
                {
                    Falcon4EntityClassType thisClass = new Falcon4EntityClassType();
                    thisClass.vuClassData = new VuEntityType();
                    thisClass.vuClassData.id_ = reader.ReadUInt16();
                    thisClass.vuClassData.collisionType_ = reader.ReadUInt16();
                    thisClass.vuClassData.collisionRadius_ = reader.ReadSingle();
                    thisClass.vuClassData.classInfo_ = new byte[8];
                    for (int j = 0; j < 8; j++)
                    {
                        thisClass.vuClassData.classInfo_[j] = reader.ReadByte();
                    }
                    thisClass.vuClassData.updateRate_ = reader.ReadUInt32();
                    thisClass.vuClassData.updateTolerance_ = reader.ReadUInt32();
                    thisClass.vuClassData.fineUpdateRange_ = reader.ReadSingle();
                    thisClass.vuClassData.fineUpdateForceRange_ = reader.ReadSingle();
                    thisClass.vuClassData.fineUpdateMultiplier_ = reader.ReadSingle();
                    thisClass.vuClassData.damageSeed_ = reader.ReadUInt32();
                    thisClass.vuClassData.hitpoints_ = reader.ReadInt32();
                    thisClass.vuClassData.majorRevisionNumber_ = reader.ReadUInt16();
                    thisClass.vuClassData.minorRevisionNumber_ = reader.ReadUInt16();
                    thisClass.vuClassData.createPriority_ = reader.ReadUInt16();
                    thisClass.vuClassData.managementDomain_ = reader.ReadByte();
                    thisClass.vuClassData.transferable_ = reader.ReadByte();
                    thisClass.vuClassData.private_ = reader.ReadByte();
                    thisClass.vuClassData.tangible_ = reader.ReadByte();
                    thisClass.vuClassData.collidable_ = reader.ReadByte();
                    thisClass.vuClassData.global_ = reader.ReadByte();
                    thisClass.vuClassData.persistent_ = reader.ReadByte();
                    reader.ReadBytes(3);            //align on int32 boundary


                    thisClass.visType = new short[7];
                    for (int j = 0; j < 7; j++)
                    {
                        thisClass.visType[j] = reader.ReadInt16();
                    }
                    thisClass.vehicleDataIndex = reader.ReadInt16();
                    thisClass.dataType = reader.ReadByte();
                    thisClass.dataPtr = reader.ReadInt32();
                    classTable[i] = thisClass;
                }
                return classTable;
            }
        }
    }
}