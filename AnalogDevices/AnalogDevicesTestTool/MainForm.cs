using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalogDevicesTestTool
{
    public partial class MainForm : Form
    {

        private AnalogDevices.DenseDacEvalBoard _selectedDevice;
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainForm));
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetApplicationTitle();
            EnumerateDevices();
            AddEventHandlersForAllDACChannelSelectRadioButtions();
            SelectDACChannel0();
            UpdateCalculatedOutputVoltage();

        }
        private void SetApplicationTitle()
        {
            Text = Application.ProductName + " v" + Application.ProductVersion;
        }
        private void EnumerateDevices()
        {
            try
            {
                cboDevices.Items.Clear();
                var availableDevices = AnalogDevices.DenseDacEvalBoard.Enumerate();
                cboDevices.Items.AddRange(availableDevices);
                cboDevices.DisplayMember = "SymbolicName";
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }

        }
        private void AddEventHandlersForAllDACChannelSelectRadioButtions()
        {
            foreach (RadioButton radioButton in gbDACOutputs.Controls.OfType<RadioButton>().Where(x=>x.Text.StartsWith("DAC")))
            {
                radioButton.CheckedChanged += DACChannelSelectRadioButton_CheckedChanged;
            }
            
        }

        void DACChannelSelectRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                var dacChannel = DetermineSelectedDACChannelAddress();
                txtDACChannelGain.Text = _selectedDevice.GetDacChannelGain(dacChannel).ToString("x");
                txtDACChannelOffset.Text = _selectedDevice.GetDacChannelOffset(dacChannel).ToString("x");
                var dacChannelDataSource = _selectedDevice.GetDacChannelDataSource(dacChannel);
                rdoDataValueA.Checked = dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueA;
                rdoDataValueB.Checked = dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueB;
            }
            UpdateCalculatedOutputVoltage();
        }
        private AnalogDevices.ChannelAddress DetermineSelectedDACChannelAddress()
        {
            var radioButton = gbDACOutputs.Controls.OfType<RadioButton>().Where(x => x.Checked && x.Text.StartsWith("DAC")).FirstOrDefault();
            var text = radioButton.Text;
            var DACNumber = Int32.Parse(text.Replace("DAC ", ""));
            return (AnalogDevices.ChannelAddress)(DACNumber + 8);
        }

        private void ValidateVREF(Control control)
        {
            ValidateFloat(control, 2, 5);
        }
        private void ValidateDACChannelOffset(Control control)
        {
            ValidateUnsignedInteger(control, 0x0000, 0xFFFF);
        }
        private void ValidateDACChannelGain(Control control)
        {
            ValidateUnsignedInteger(control, 0x0000, 0xFFFF);
        }
        private void ValidateOffsetDAC(Control control)
        {
            ValidateUnsignedInteger(control, 0x0000, 0x3FFF);
        }


        private void txtVREF0_Validating(object sender, CancelEventArgs e)
        {
            ValidateVREF(txtVREF0);
        }

        private void txtVREF1_Validating(object sender, CancelEventArgs e)
        {
            ValidateVREF(txtVREF1);
        }

        private void txtOffsetDAC0_Validating(object sender, CancelEventArgs e)
        {
            ValidateOffsetDAC(txtOffsetDAC0);
        }
        private void txtOffsetDAC1_Validating(object sender, CancelEventArgs e)
        {
            ValidateOffsetDAC(txtOffsetDAC1);
        }

        private void txtDACChannelOffset_Validating(object sender, CancelEventArgs e)
        {
            ValidateDACChannelOffset(txtDACChannelOffset);
        }
        private void txtDACChannelOffset_Validated(object sender, EventArgs e)
        {
            ushort dacChannelOffset;
            var parsed = ushort.TryParse(txtDACChannelOffset.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelOffset);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelOffset(selectedDacChannel, dacChannelOffset);
                }
            }
        }
        private void txtDACChannelGain_Validated(object sender, EventArgs e)
        {
            ushort dacChannelGain;
            var parsed = ushort.TryParse(txtDACChannelGain.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelGain);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelGain(selectedDacChannel, dacChannelGain);
                }
                
            }
        }
        private void txtDACChannelGain_Validating(object sender, CancelEventArgs e)
        {
            ValidateDACChannelGain(txtDACChannelGain);
        }


        private void cboDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedDevice = (AnalogDevices.DenseDacEvalBoard)cboDevices.SelectedItem;
            UpdateGroupOffsetDataInUIWithDataFromDevice();
            SelectDACChannel0();
            UpdateCalculatedOutputVoltage();
        }
        private void SelectDACChannel0()
        {
            var DAC0RadioButton = gbDACOutputs.Controls.OfType<RadioButton>().Where(x=>x.Text=="DAC 0").FirstOrDefault();
            if (DAC0RadioButton != null)
            {
                DAC0RadioButton.Checked = true;
            }
        }
        private void UpdateGroupOffsetDataInUIWithDataFromDevice()
        {
            if (_selectedDevice != null)
            {
                txtOffsetDAC0.Text = _selectedDevice.OffsetDAC0.ToString("x");
                txtOffsetDAC1.Text = _selectedDevice.OffsetDAC1.ToString("x");
            }
        }


        private void txtDataValueA_Validating(object sender, CancelEventArgs e)
        {
            ValidateUnsignedInteger(txtDataValueA, 0x0000, 0xFFFF);
        }
        private void txtDataValueB_Validating(object sender, CancelEventArgs e)
        {
            ValidateUnsignedInteger(txtDataValueB, 0x0000, 0xFFFF);
        }
        private ushort ValidateUnsignedInteger(Control control, int low, int high)
        {
            errorProvider1.SetError(control, String.Empty);
            var text = control.Text;
            ushort val;
            bool parsed = ushort.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val);
            if (!parsed || val < low || val > high)
            {
                errorProvider1.SetError(control, string.Format("Value must be a positive hexadecimal integer between {0} and {1}", low, high));
            }
            return val;
        }
        private void ValidateFloat(Control control, float low, float high)
        {
            errorProvider1.SetError(control, String.Empty);
            var text = control.Text;
            float val;
            bool parsed = float.TryParse(text, out val);
            if (!parsed || float.IsNaN(val) || float.IsInfinity(val) || val < low || val > high)
            {
                errorProvider1.SetError(control, string.Format("Value must be a positive integer or decimal between {0} and {1}", low, high));
            }
        }

        private void btnUpdateAllDacOutputs_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.UpdateAllDacOutputs();
            }
        }

        private void btnResumeAllDACOutputs_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.ResumeAllDacOutputs();
            }
        }

        private void btnSuspendAllDACOutputs_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.SuspendAllDacOutputs();
            }
        }

        private void btnSoftPowerUp_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.PerformSoftPowerUp();
            }
        }

        private void btnPerformSoftPowerDown_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.PerformSoftPowerDown();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.Reset();
            }
        }

        private void UpdateDACValuesFromDevice(AnalogDevices.ChannelAddress dacChannel)
        {
            if (_selectedDevice != null)
            {
                txtDataValueA.Text = _selectedDevice.GetDacChannelDataValueA(dacChannel).ToString();
                txtDataValueB.Text = _selectedDevice.GetDacChannelDataValueB(dacChannel).ToString();

            }
        }



       
        private void rdoDataValueA_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDataValueA.Checked)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelDataSource(selectedDacChannel, AnalogDevices.DacChannelDataSource.DataValueA);
                }
            }
        }

        private void rdoDataValueB_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDataValueB.Checked)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelDataSource(selectedDacChannel, AnalogDevices.DacChannelDataSource.DataValueB);
                }
            }
        }

        private void txtOffsetDAC0_Validated(object sender, EventArgs e)
        {
            ushort offsetDAC0;
            var parsed = ushort.TryParse(txtOffsetDAC0.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDAC0);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    _selectedDevice.OffsetDAC0 = offsetDAC0;
                }
            }
        }

        private void txtOffsetDAC1_Validated(object sender, EventArgs e)
        {
            ushort offsetDAC1;
            var parsed = ushort.TryParse(txtOffsetDAC1.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDAC1);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    _selectedDevice.OffsetDAC1 = offsetDAC1;
                }
            }
        }

        private void txtDataValueA_Validated(object sender, EventArgs e)
        {
            ushort dataValueA;
            var parsed = ushort.TryParse(txtDataValueA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueA);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelDataValueA(selectedDacChannel, dataValueA);
                }
            }
        }

        private void txtDataValueB_Validated(object sender, EventArgs e)
        {
            ushort dataValueB;
            var parsed = ushort.TryParse(txtDataValueB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueB);
            if (parsed)
            {
                UpdateCalculatedOutputVoltage();
                if (_selectedDevice != null)
                {
                    var selectedDacChannel = DetermineSelectedDACChannelAddress();
                    _selectedDevice.SetDacChannelDataValueB(selectedDacChannel, dataValueB);
                }
            }
        }
        private void UpdateCalculatedOutputVoltage()
        {
            var dacChannel = DetermineSelectedDACChannelAddress();
            var calculatedOutputVoltage = Vout(dacChannel);
            var asText = calculatedOutputVoltage.ToString("F4");
            if (calculatedOutputVoltage >=0) asText = "+" + asText;
            txtVoutCalculated.Text = asText;
        }
        private float Vout(ushort dacChannelDataValue, ushort dacChannelOffsetValue, ushort dacChannelGainValue, ushort offsetDACvalue, float Vref)
        {
            var dac_code = (ushort) (((long)dacChannelDataValue * ((long)dacChannelGainValue + (long)1)) / (long)65536) + (long)dacChannelOffsetValue - (long)32768;
            var Vout = (ulong)4 * Vref * (((long)dac_code - ((long)4 * (long)offsetDACvalue)) / (float)65536);
            return Vout;
        }
        private float Vout(AnalogDevices.ChannelAddress dacChannel)
        {
            if (_selectedDevice != null)
            {

                var dacChannelDataSource = _selectedDevice.GetDacChannelDataSource(dacChannel);
                var dacChannelDataValue = (dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueA)
                    ? _selectedDevice.GetDacChannelDataValueA(dacChannel)
                    : _selectedDevice.GetDacChannelDataValueB(dacChannel);
                var dacChannelOffsetValue = _selectedDevice.GetDacChannelOffset(dacChannel);
                var dacChannelGainValue = _selectedDevice.GetDacChannelGain(dacChannel);
                ushort offsetDACValue;
                float vRef;
                if (dacChannel >= AnalogDevices.ChannelAddress.Group0Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group0Channel7)
                {
                    offsetDACValue = _selectedDevice.OffsetDAC0;
                    bool parsed = float.TryParse(txtVREF0.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }
                else if (dacChannel >= AnalogDevices.ChannelAddress.Group1Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group4Channel7)
                {
                    offsetDACValue = _selectedDevice.OffsetDAC1;
                    bool parsed = float.TryParse(txtVREF1.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }

                else
                {
                    offsetDACValue = _selectedDevice.OffsetDAC0;
                    bool parsed = float.TryParse(txtVREF0.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }
                return Vout(dacChannelDataValue, dacChannelOffsetValue, dacChannelGainValue, offsetDACValue, vRef);
            }
            else
            {
                var dacChannelDataSource = rdoDataValueA.Checked ? AnalogDevices.DacChannelDataSource.DataValueA: AnalogDevices.DacChannelDataSource.DataValueB;
                ushort dataValueA;
                ushort dataValueB;

                bool parsed = ushort.TryParse(txtDataValueA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueA);
                if (!parsed)
                {
                    return 0;
                }
                parsed = ushort.TryParse(txtDataValueB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueB);
                if (!parsed)
                {
                    return 0;
                }

                var dacChannelDataValue = (dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueA)
                    ? dataValueA
                    : dataValueB;

                ushort dacChannelOffsetValue;
                parsed = ushort.TryParse(txtDACChannelOffset.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelOffsetValue);
                if (!parsed)
                {
                    return 0;
                }
                ushort dacChannelGainValue;
                parsed = ushort.TryParse(txtDACChannelGain.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelGainValue);
                if (!parsed)
                {
                    return 0;
                }

                ushort offsetDACValue;
                float vRef;
                if (dacChannel >= AnalogDevices.ChannelAddress.Group0Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group0Channel7)
                {
                    parsed= ushort.TryParse(txtOffsetDAC0.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                    if (!parsed)
                    {
                        return 0;
                    }
                    parsed = float.TryParse(txtVREF0.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }
                else if (dacChannel >= AnalogDevices.ChannelAddress.Group1Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group4Channel7)
                {
                    parsed = ushort.TryParse(txtOffsetDAC1.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                    if (!parsed)
                    {
                        return 0;
                    }
                    parsed = float.TryParse(txtVREF1.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }

                else
                {
                    parsed = ushort.TryParse(txtOffsetDAC0.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                    if (!parsed)
                    {
                        return 0;
                    }
                    parsed = float.TryParse(txtVREF0.Text, out vRef);
                    if (!parsed)
                    {
                        return 0;
                    }
                }
                return Vout(dacChannelDataValue, dacChannelOffsetValue, dacChannelGainValue, offsetDACValue, vRef);
            }

        }

    }
}
