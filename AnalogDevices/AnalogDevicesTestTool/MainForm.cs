using log4net;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
            UpdateDeviceValuesInUI();
            SelectDACChannel0();

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
                foreach (var device in availableDevices)
                {
                    device.IsThermalShutdownEnabled = true; //enable over temp protection automatically during device enumeration
                }
                cboDevices.Items.AddRange(availableDevices);
                cboDevices.DisplayMember = "SymbolicName";
                if (availableDevices.Count() > 0)
                {
                    cboDevices.SelectedItem = cboDevices.Items[0];
                }
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
        private void ReadbackSelectedDACChannelOffset()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                var dacChannelOffset = _selectedDevice.GetDacChannelOffset(selectedDacChannel);
                txtDACChannelOffset.Text = dacChannelOffset.ToString("X").PadLeft(4, '0');
            }
        }
        private void ReadbackSelectedDACChannelGain()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                var dacChannelGain = _selectedDevice.GetDacChannelGain(selectedDacChannel);
                txtDACChannelGain.Text = dacChannelGain.ToString("X").PadLeft(4, '0');
            }
        }
        private void ReadbackSelectedDACChannelDataValueA()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                var dacChannelDataValueA= _selectedDevice.GetDacChannelDataValueA(selectedDacChannel);
                txtDataValueA.Text = dacChannelDataValueA.ToString("X").PadLeft(4, '0');
            }
        }
        private void ReadbackSelectedDACChannelDataValueB()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                var dacChannelDataValueB = _selectedDevice.GetDacChannelDataValueB(selectedDacChannel);
                txtDataValueB.Text = dacChannelDataValueB.ToString("X").PadLeft(4, '0');
            }
        }
        private void ReadbackSelectedDACChannelDataSource()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                var dacChannelDataSource = _selectedDevice.GetDacChannelDataSource(selectedDacChannel);
                if (dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueA)
                {
                    rdoDataValueA.Checked = true;
                }
                else
                {
                    rdoDataValueB.Checked = true;
                }
            }
        }
        private void ReadbackOffsetDAC0()
        {
            if (_selectedDevice != null)
            {
                var offsetDAC0= _selectedDevice.OffsetDAC0;
                txtOffsetDAC0.Text = offsetDAC0.ToString("X").PadLeft(4, '0');
            }
        }
        private void ReadbackOffsetDAC1()
        {
            if (_selectedDevice != null)
            {
                var offsetDAC1 = _selectedDevice.OffsetDAC1;
                txtOffsetDAC1.Text = offsetDAC1.ToString("X").PadLeft(4, '0');
            }
        }
        private void DACChannelSelectRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIValuesForSelectedDACChannel();
        }
        private void UpdateUIValuesForSelectedDACChannel()
        {
            ReadbackSelectedDACChannelDataSource();
            ReadbackSelectedDACChannelDataValueA();
            ReadbackSelectedDACChannelDataValueB();
            ReadbackSelectedDACChannelOffset();
            ReadbackSelectedDACChannelGain();
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
            SetDACChannelOffset();
        }

        private void SetDACChannelOffset()
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
            SetDACChannelGain();
        }

        private void SetDACChannelGain()
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
            UpdateDeviceValuesInUI();
            SelectDACChannel0();
        }
        private void UpdateDeviceValuesInUI()
        {
            UpdateGroupOffsetDataInUIToDeviceValues();
            UpdateUIValuesForSelectedDACChannel();
            UpdateCalculatedOutputVoltage();
            UpdateOverTemperatureValuesInUIFromDevice();

        }
        private void UpdateOverTemperatureValuesInUIFromDevice()
        {
            if (_selectedDevice != null)
            {
                var isOverTemp = _selectedDevice.IsOverTemperature;
                if (isOverTemp)
                {
                    txtOverTempStatus.Text = "HOT";
                    txtOverTempStatus.BackColor = Color.Red;
                    txtOverTempStatus.ForeColor = Color.White;
                }
                else
                {
                    txtOverTempStatus.Text = "OK";
                    txtOverTempStatus.BackColor = Color.Green;
                    txtOverTempStatus.ForeColor = Color.White;
                }
                var overTempShutdownEnabled = _selectedDevice.IsThermalShutdownEnabled;
                chkOverTempShutdownEnabled.Checked = overTempShutdownEnabled;
            }
            else
            {
                txtOverTempStatus.Text = "N/A";
                txtOverTempStatus.BackColor = Color.Empty;
                txtOverTempStatus.ForeColor = Color.Black;
            }
        }
        private void SelectDACChannel0()
        {
            var DAC0RadioButton = gbDACOutputs.Controls.OfType<RadioButton>().Where(x=>x.Text=="DAC 0").FirstOrDefault();
            if (DAC0RadioButton != null)
            {
                DAC0RadioButton.Checked = true;
            }
        }
        private void UpdateGroupOffsetDataInUIToDeviceValues()
        {
            if (_selectedDevice != null)
            {
                txtOffsetDAC0.Text = _selectedDevice.OffsetDAC0.ToString("X").PadLeft(4,'0');
                txtOffsetDAC1.Text = _selectedDevice.OffsetDAC0.ToString("X").PadLeft(4, '0');
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
                errorProvider1.SetError(control, string.Format("Value must be a positive hexadecimal integer between {0:X} and {1:X}", low, high));
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
            UpdateCalculatedOutputVoltage();
            if (_selectedDevice != null)
            {
                SetDACChannelOffset();
                SetDACChannelGain();
                UpdateOffsetDAC0();
                UpdateOffsetDAC1();
                SetDACChannelDataSourceAndUpdateDataValue();
                _selectedDevice.UpdateAllDacOutputs();
            }
        }
        private void SetDACChannelDataSourceAndUpdateDataValue()
        {
            if (_selectedDevice != null)
            {
                var selectedDacChannel = DetermineSelectedDACChannelAddress();
                if (rdoDataValueA.Checked)
                {
                    _selectedDevice.SetDacChannelDataSource(selectedDacChannel, AnalogDevices.DacChannelDataSource.DataValueA);
                    UpdateDataValueA();
                }
                else
                {
                    _selectedDevice.SetDacChannelDataSource(selectedDacChannel, AnalogDevices.DacChannelDataSource.DataValueB);
                    UpdateDataValueB();
                }
            }

        }
        private void btnResumeAllDACOutputs_Click(object sender, EventArgs e)
        {
            UpdateCalculatedOutputVoltage();
            if (_selectedDevice != null)
            {
                _selectedDevice.ResumeAllDacOutputs();
            }
        }

        private void btnSuspendAllDACOutputs_Click(object sender, EventArgs e)
        {
            UpdateCalculatedOutputVoltage();
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
                UpdateDeviceValuesInUI();
            }
        }

       
        private void rdoDataValueA_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDataValueA.Checked)
            {
                UpdateCalculatedOutputVoltage();
            }
        }

        private void rdoDataValueB_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDataValueB.Checked)
            {
                UpdateCalculatedOutputVoltage();
            }
        }

        private void txtOffsetDAC0_Validated(object sender, EventArgs e)
        {
            UpdateOffsetDAC0();
        }

        private void UpdateOffsetDAC0()
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
            UpdateOffsetDAC1();
        }

        private void UpdateOffsetDAC1()
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
            UpdateDataValueA();
        }

        private void UpdateDataValueA()
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
            UpdateDataValueB();
        }

        private void UpdateDataValueB()
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
            var asText = calculatedOutputVoltage.ToString("F3");
            if (calculatedOutputVoltage >=0) asText = "+" + asText;
            txtVoutCalculated.Text = asText;
        }
        private float Vout(ushort dacChannelDataValue, ushort dacChannelOffsetValue, ushort dacChannelGainValue, ushort offsetDACvalue, float Vref)
        {
            var dac_code = (ushort) (((long)dacChannelDataValue * ((long)dacChannelGainValue + (long)1)) / (long)65536) + (long)dacChannelOffsetValue - (long)32768;
            if (dac_code >= 65535)
            {
                dac_code = 65535;
            }
            if (dac_code < 0)
            {
                dac_code = 0;
            }
            var Vout = (ulong)4 * Vref * (((long)dac_code - ((long)4 * (long)offsetDACvalue)) / (float)65536);
            return Vout;
        }
        private float Vout(AnalogDevices.ChannelAddress dacChannel)
        {
            var dacChannelDataSource = rdoDataValueA.Checked ? AnalogDevices.DacChannelDataSource.DataValueA: AnalogDevices.DacChannelDataSource.DataValueB;
            ushort dataValueA;
            ushort dataValueB;

            bool parsed = ushort.TryParse(txtDataValueA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueA);
            if (!parsed)
            {
                _log.ErrorFormat("Could not parse contents of txtDataValueA");
                return 0;
            }
            parsed = ushort.TryParse(txtDataValueB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dataValueB);
            if (!parsed)
            {
                _log.ErrorFormat("Could not parse contents of txtDataValueB");
                return 0;
            }

            var dacChannelDataValue = (dacChannelDataSource == AnalogDevices.DacChannelDataSource.DataValueA)
                ? dataValueA
                : dataValueB;

            ushort dacChannelOffsetValue;
            parsed = ushort.TryParse(txtDACChannelOffset.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelOffsetValue);
            if (!parsed)
            {
                _log.ErrorFormat("Could not parse contents of txtDACChannelOffset");
                return 0;
            }
            ushort dacChannelGainValue;
            parsed = ushort.TryParse(txtDACChannelGain.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dacChannelGainValue);
            if (!parsed)
            {
                _log.ErrorFormat("Could not parse contents of txtDACChannelGain");
                return 0;
            }

            ushort offsetDACValue;
            float vRef;
            if (dacChannel >= AnalogDevices.ChannelAddress.Group0Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group0Channel7)
            {
                parsed= ushort.TryParse(txtOffsetDAC0.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtOffsetDAC0");
                    return 0;
                }
                parsed = float.TryParse(txtVREF0.Text, out vRef);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtVREF0");
                    return 0;
                }
            }
            else if (dacChannel >= AnalogDevices.ChannelAddress.Group1Channel0 && dacChannel <= AnalogDevices.ChannelAddress.Group4Channel7)
            {
                parsed = ushort.TryParse(txtOffsetDAC1.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtOffsetDAC1");
                    return 0;
                }
                parsed = float.TryParse(txtVREF1.Text, out vRef);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtVREF1");
                    return 0;
                }
            }

            else
            {
                parsed = ushort.TryParse(txtOffsetDAC0.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetDACValue);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtOffsetDAC0");
                    return 0;
                }
                parsed = float.TryParse(txtVREF0.Text, out vRef);
                if (!parsed)
                {
                    _log.ErrorFormat("Could not parse contents of txtVREF0");
                    return 0;
                }
            }
            return Vout(dacChannelDataValue, dacChannelOffsetValue, dacChannelGainValue, offsetDACValue, vRef);

        }

        private void txtVREF0_Click(object sender, EventArgs e)
        {
            txtVREF0.SelectAll();
        }

        private void txtVREF1_Click(object sender, EventArgs e)
        {
            txtVREF1.SelectAll();
        }

        private void txtOffsetDAC0_Click(object sender, EventArgs e)
        {
            txtOffsetDAC0.SelectAll();
        }

        private void txtOffsetDAC1_Click(object sender, EventArgs e)
        {
            txtOffsetDAC1.SelectAll();
        }

        private void txtDACChannelOffset_Click(object sender, EventArgs e)
        {
            txtDACChannelOffset.SelectAll();
        }

        private void txtDACChannelGain_Click(object sender, EventArgs e)
        {
            txtDACChannelGain.SelectAll();
        }

        private void txtDataValueA_Click(object sender, EventArgs e)
        {
            txtDataValueA.SelectAll();
        }

        private void txtDataValueB_Click(object sender, EventArgs e)
        {
            txtDataValueB.SelectAll();
        }

        private void chkOverTempShutdownEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.IsThermalShutdownEnabled = chkOverTempShutdownEnabled.Checked;
            }
        }

       
      

    }
}
