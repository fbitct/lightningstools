using System;
using F4Utils.Campaign.F4Structs;
using System.IO;
using System.Text;

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
        public PstFile(Stream stream, int version)
            : this()
        {
            Decode(stream, version);
        }
        protected void Decode(Stream stream, int version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                numPersistantObjects = 0;

                if (version < 69)
                {
                    return;
                }
                numPersistantObjects = reader.ReadInt32();
                persistantObjects = new PersistantObject[numPersistantObjects];
                for (int i = 0; i < numPersistantObjects; i++)
                {
                    PersistantObject thisObject = new PersistantObject();
                    thisObject.x = reader.ReadSingle();
                    thisObject.y = reader.ReadSingle();
                    thisObject.unionData = new PackedVUID();
                    thisObject.unionData.creator_ = reader.ReadUInt32();
                    thisObject.unionData.num_ = reader.ReadUInt32();
                    thisObject.unionData.index_ = reader.ReadByte();
                    reader.ReadBytes(3); //align on Int32 boundary
                    thisObject.visType = reader.ReadInt16();
                    thisObject.flags = reader.ReadInt16();
                    persistantObjects[i] = thisObject;
                }
            }
        }
    }
}