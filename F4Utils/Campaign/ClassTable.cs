using System;
using System.IO;

namespace F4Utils.Campaign
{
    public static class ClassTable
    {
        public static Falcon4EntityClassType[] ReadClassTable(string classTableFilePath)
        {
            if (classTableFilePath == null) throw new ArgumentNullException("classTableFilePath");
            var ctFileInfo = new FileInfo(classTableFilePath);
            if (!ctFileInfo.Exists) throw new FileNotFoundException(classTableFilePath);
            var bytes = new byte[ctFileInfo.Length];
            using (var fs = new FileStream(classTableFilePath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes, 0, (int) ctFileInfo.Length);
                fs.Close();
            }
            var curByte = 0;
            var numEntities = BitConverter.ToInt16(bytes, curByte);
            curByte += 2;
            var classTable = new Falcon4EntityClassType[numEntities];
            for (var i = 0; i < numEntities; i++)
            {
                var thisClass = new Falcon4EntityClassType();
                thisClass.vuClassData = new VuEntityType();
                thisClass.vuClassData.id_ = BitConverter.ToUInt16(bytes, curByte);
                curByte += 2;
                thisClass.vuClassData.collisionType_ = BitConverter.ToUInt16(bytes, curByte);
                curByte += 2;
                thisClass.vuClassData.collisionRadius_ = BitConverter.ToSingle(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.classInfo_ = new byte[8];
                for (var j = 0; j < 8; j++)
                {
                    thisClass.vuClassData.classInfo_[j] = bytes[curByte];
                    curByte++;
                }
                thisClass.vuClassData.updateRate_ = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.updateTolerance_ = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.fineUpdateRange_ = BitConverter.ToSingle(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.fineUpdateForceRange_ = BitConverter.ToSingle(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.fineUpdateMultiplier_ = BitConverter.ToSingle(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.damageSeed_ = BitConverter.ToUInt32(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.hitpoints_ = BitConverter.ToInt32(bytes, curByte);
                curByte += 4;
                thisClass.vuClassData.majorRevisionNumber_ = BitConverter.ToUInt16(bytes, curByte);
                curByte += 2;
                thisClass.vuClassData.minorRevisionNumber_ = BitConverter.ToUInt16(bytes, curByte);
                curByte += 2;
                thisClass.vuClassData.createPriority_ = BitConverter.ToUInt16(bytes, curByte);
                curByte += 2;
                thisClass.vuClassData.managementDomain_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.transferable_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.private_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.tangible_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.collidable_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.global_ = bytes[curByte];
                curByte++;
                thisClass.vuClassData.persistent_ = bytes[curByte];
                curByte++;
                curByte += 3; //align on int32 boundary


                thisClass.visType = new short[7];
                for (var j = 0; j < 7; j++)
                {
                    thisClass.visType[j] = BitConverter.ToInt16(bytes, curByte);
                    curByte += 2;
                }
                thisClass.vehicleDataIndex = BitConverter.ToInt16(bytes, curByte);
                curByte += 2;
                thisClass.dataType = bytes[curByte];
                curByte++;


                //skip dataPtr
                curByte += 4;
                classTable[i] = thisClass;
            }
            return classTable;
        }
    }
}