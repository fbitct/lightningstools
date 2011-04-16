using System.ComponentModel;
using System.Reflection;

namespace AnalogDevices
{

    #region Channel Monitor Options class

    public class ChannelMonitorOptions : INotifyPropertyChanged
    {
        private ChannelMonitorSource _channelMonitorSource = ChannelMonitorSource.None;
        private byte _channelNumberOrInputPinNumber;

        private ChannelMonitorOptions()
        {
        }

        public ChannelMonitorOptions(ChannelMonitorSource channelMonitorSource, byte channelNumberOrInputPinNumber)
            : this()
        {
            ChannelMonitorSource = channelMonitorSource;
            ChannelNumberOrInputPinNumber = channelNumberOrInputPinNumber;
        }

        public ChannelMonitorSource ChannelMonitorSource
        {
            get { return _channelMonitorSource; }
            set
            {
                if (value != _channelMonitorSource)
                {
                    _channelMonitorSource = value;
                    _channelNumberOrInputPinNumber = 0;
                    FirePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        public byte ChannelNumberOrInputPinNumber
        {
            get { return _channelNumberOrInputPinNumber; }
            set
            {
                if (value != _channelNumberOrInputPinNumber)
                {
                    _channelNumberOrInputPinNumber = value;
                    FirePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}