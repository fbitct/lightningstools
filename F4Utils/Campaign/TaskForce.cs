using System;
using System.IO;
using System.Text;
namespace F4Utils.Campaign
{
    public class TaskForce : Unit
    {
        #region Public Fields
        public byte orders;
        public byte supply;
        #endregion

        protected TaskForce()
            : base()
        {
        }
        public TaskForce(Stream stream, int version)
            : base(stream, version)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen: true))
            {
                orders = reader.ReadByte();
                supply = reader.ReadByte();
            }
        }
    }
}