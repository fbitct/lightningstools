namespace AnalogDevices
{
    internal class Test
    {
        public static void Main(string[] args)
        {
            var evalBoardsAttachedToThisSystem = DenseDacEvalBoard.Enumerate();
            foreach (var thisEvalBoard in evalBoardsAttachedToThisSystem)
            {
                thisEvalBoard.DacPrecision = DacPrecision.SixteenBit;
                var voltage = -9.75f;
                var value = (ushort) (((voltage + 10)/20)*0xFFFF);

                thisEvalBoard.SetDacChannelDataValueA(ChannelAddress.Group0Channel0, value);
                thisEvalBoard.UpdateAllDacOutputs();
            }
        }
    }
}