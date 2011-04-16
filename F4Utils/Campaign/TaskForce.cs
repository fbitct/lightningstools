namespace F4Utils.Campaign
{
    public class TaskForce : Unit
    {
        #region Public Fields

        public byte orders;
        public byte supply;

        #endregion

        protected TaskForce()
        {
        }

        public TaskForce(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
            orders = bytes[offset];
            offset++;
            supply = bytes[offset];
            offset++;
        }
    }
}