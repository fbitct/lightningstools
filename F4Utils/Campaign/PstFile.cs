using System;
using F4Utils.Campaign.F4Structs;

namespace F4Utils.Campaign
{
    public class PstFile
    {
        #region Public Fields
        public int numPersistantObjects;
        public PersistantObject[] persistantObjects;
        #endregion

        protected PstFile()
            : base()
        {
        }
        public PstFile(byte[] bytes, int version)
            : this()
        {
            Decode(bytes, version);
        }
        protected void Decode(byte[] bytes, int version)
        {
            int offset = 0;
            numPersistantObjects = 0;

            if (version < 69)
            {
                return;
            }
            numPersistantObjects = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            persistantObjects = new PersistantObject[numPersistantObjects];
            for (int i = 0; i < numPersistantObjects; i++)
            {
                PersistantObject thisObject = new PersistantObject();
                thisObject.x = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                thisObject.y = BitConverter.ToSingle(bytes, offset);
                offset += 4;
                thisObject.unionData = new PackedVUID();
                thisObject.unionData.creator_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisObject.unionData.num_ = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                thisObject.unionData.index_ = bytes[offset];
                offset++;
                offset += 3; //align on Int32 boundary
                thisObject.visType = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                thisObject.flags = BitConverter.ToInt16(bytes, offset);
                offset += 2;
                persistantObjects[i] = thisObject;
            }
        }
    }
}