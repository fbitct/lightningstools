namespace F4Utils.Campaign
{
    public class AirUnit : Unit
    {
        protected AirUnit()
        {
        }

        public AirUnit(byte[] bytes, ref int offset, int version)
            : base(bytes, ref offset, version)
        {
        }
    }
}