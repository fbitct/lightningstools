using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common.InputSupport;
using Common.InputSupport.DirectInput;
using Common.Win32;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using log4net;
using System.Runtime.InteropServices;

namespace Common.InputSupport.UI
{
    public sealed partial class InputSourceSelector : Form
    {
        private Mediator.PhysicalControlStateChangedEventHandler _mediatorHandler = null;
        private Mediator _mediator = null;
        private static ILog _log = LogManager.GetLogger(typeof(InputSourceSelector));

        public string PromptText { get; set; }
        public InputControlSelection SelectedControl
        {
            get;
            set;
        }
        public Mediator Mediator
        {
            get
            {
                return _mediator;
            }
            set
            {
                _mediator = value;
                if (_mediator != null)
                {
                    _mediator.PhysicalControlStateChanged += _mediatorHandler;
                    _mediator.RaiseEvents = true;

                }
            }
        }

        public InputSourceSelector()
        {
            InitializeComponent();
            _mediatorHandler = new Mediator.PhysicalControlStateChangedEventHandler(_mediator_PhysicalControlStateChanged);
            this.PreviewKeyDown += new PreviewKeyDownEventHandler(PreviewKeyDownHandler);
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
            this.SelectedControl = new InputControlSelection();
        }

        private void _mediator_PhysicalControlStateChanged(object sender, PhysicalControlStateChangedEventArgs e)
        {
            if (e.Control.ControlType == ControlType.Button || e.Control.ControlType == ControlType.Pov)
            {
                rdoJoystick.Checked = true;
                DIPhysicalControlInfo control = (DIPhysicalControlInfo)e.Control;
                DIPhysicalDeviceInfo device = (DIPhysicalDeviceInfo)control.Parent;
                cbJoysticks.SelectedItem = device;
                cboJoystickControl.SelectedItem = control;
                if (control.ControlType == ControlType.Pov)
                {
                    float currentDegrees = e.CurrentState / 100;
                    if (e.CurrentState == -1) currentDegrees = -1;
                    /*  POV directions in degrees
                              0
                        337.5  22.5   
                       315         45
                     292.5           67.5
                    270                90
                     247.5           112.5
                      225          135
                        202.5  157.5
                            180
                     */
                    PovDirections? direction = null;
                    if ((currentDegrees > 337.5 && currentDegrees <= 360) || (currentDegrees >= 0 && currentDegrees <= 22.5))
                    {
                        direction = PovDirections.Up;
                        rdoPovUp.Checked = true;
                    }
                    else if (currentDegrees > 22.5 && currentDegrees <= 67.5)
                    {
                        direction = PovDirections.UpRight;
                        rdoPovUpRight.Checked = true;
                    }
                    else if (currentDegrees > 67.5 && currentDegrees <= 112.5)
                    {
                        direction = PovDirections.Right;
                        rdoPovRight.Checked = true;
                    }
                    else if (currentDegrees > 112.5 && currentDegrees <= 157.5)
                    {
                        direction = PovDirections.DownRight;
                        rdoPovDownRight.Checked = true;
                    }
                    else if (currentDegrees > 157.5 && currentDegrees <= 202.5)
                    {
                        direction = PovDirections.Down;
                        rdoPovDown.Checked = true;
                    }
                    else if (currentDegrees > 202.5 && currentDegrees <= 247.5)
                    {
                        direction = PovDirections.DownLeft;
                        rdoPovDownLeft.Checked = true;
                    }
                    else if (currentDegrees > 247.5 && currentDegrees <= 292.5)
                    {
                        direction = PovDirections.Left;
                        rdoPovLeft.Checked = true;
                    }
                    else if (currentDegrees > 292.5 && currentDegrees <= 337.5)
                    {
                        direction = PovDirections.UpLeft;
                        rdoPovUpLeft.Checked = true;
                    }
                }
            }
        }
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this.ContainsFocus) return;
            if (
                (((e.KeyCode & Keys.KeyCode) & Keys.Tab) == Keys.Tab)
                 ||
                (((e.KeyCode & Keys.KeyCode) & Keys.Up) == Keys.Up)
                 ||
                (((e.KeyCode & Keys.KeyCode) & Keys.Down) == Keys.Down)
              )
            {
                if (rdoKeystroke.Checked)
                {
                    e.Handled  = true;
                }
            }
            else
            {
                rdoKeystroke.Checked = true;
                txtKeystroke.Select();
            }
            UpdateKeyAssignmentData(e.KeyCode);

        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (
                ((keyData & Keys.KeyCode) & Keys.Tab) == Keys.Tab
                 ||
                ((keyData & Keys.KeyCode) & Keys.Up) == Keys.Up
                 ||
                ((keyData & Keys.KeyCode) & Keys.Down) == Keys.Down
                
                )
            {
                rdoKeystroke.Checked = true;
                txtKeystroke.Select();
                UpdateKeyAssignmentData(keyData);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void PreviewKeyDownHandler(object sender, PreviewKeyDownEventArgs e)
        {
            if (
                (((e.KeyCode & Keys.KeyCode) & Keys.Tab) == Keys.Tab)
                 ||
                (((e.KeyCode & Keys.KeyCode) & Keys.Up) == Keys.Up)
                 ||
                (((e.KeyCode & Keys.KeyCode) & Keys.Down) == Keys.Down)
              )
            {
                if (rdoKeystroke.Checked)
                {
                    e.IsInputKey = true;
                }
            }
        }
        private static string GetKeyName(Keys key)
        {
            return key.ToString();
        }
        private void UpdateKeyAssignmentData(Keys key)
        {
            if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
            {
                key |= Keys.Shift;
                //SHIFT is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
            {
                key |= Keys.Control;
                //CONTROL is pressed
            }
            if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
            {
                key |= Keys.Alt;
                //ALT is pressed
            }

            txtKeystroke.Text = GetKeyName(key);
        }
        private void Form_Load(object sender, EventArgs e)
        {
            txtHelpText.Lines = new string[]{
                "Press and release the desired " + 
                "keystroke/combination, or press and release " + 
                "the desired joystick input, to assign it to this control."
            };
            PopulateJoysticksComboBox();
            PopulateJoystickControlsComboBox();
            LoadSelectedControlInfo();
            EnableDisableControls();
        }
        private void EnableDisableControls()
        {
            if (cbJoysticks.Items.Count == 0)
            {
                rdoJoystick.Enabled = false;
            }
            else
            {
                rdoJoystick.Enabled = true;
            }

            if (rdoKeystroke.Checked)
            {
                this.gbPovDirections.Enabled = false;
                this.txtKeystroke.Enabled = true;
                lblDeviceName.Enabled = false;
                cbJoysticks.Enabled = false;
                lblJoystickControl.Enabled = false;
                cboJoystickControl.Enabled = false;
            }
            else if (rdoJoystick.Checked)
            {
                lblDeviceName.Enabled = true;
                cbJoysticks.Enabled = true;
                lblJoystickControl.Enabled = true;
                cboJoystickControl.Enabled = true;
                this.txtKeystroke.Enabled = false;

                SelectCurrentJoystick();
                SelectCurrentJoystickControl();

                DIPhysicalDeviceInfo device = (DIPhysicalDeviceInfo)cbJoysticks.SelectedItem;
                DIPhysicalControlInfo control = (DIPhysicalControlInfo)cboJoystickControl.SelectedItem;
                if (control != null)
                {
                    switch (control.ControlType)
                    {
                        case ControlType.Axis:
                            this.gbPovDirections.Enabled = false;
                            break;
                        case ControlType.Button:
                            this.gbPovDirections.Enabled = false;
                            break;
                        case ControlType.Pov:
                            this.gbPovDirections.Enabled = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    cboJoystickControl.SelectedIndex = 0;
                }
            }
            else if (rdoNotAssigned.Checked)
            {
                this.gbPovDirections.Enabled = false;
                this.txtKeystroke.Enabled = false;
                lblDeviceName.Enabled = false;
                cbJoysticks.Enabled = false;
                lblJoystickControl.Enabled = false;
                cboJoystickControl.Enabled = false;
            }
        }
        private List<DIPhysicalDeviceInfo> GetKnownDirectInputDevices()
        {
            List<DIPhysicalDeviceInfo> knownDevices = new List<DIPhysicalDeviceInfo>();
            if (this.Mediator != null)
            {
                foreach (var key in this.Mediator.DeviceMonitors.Keys)
                {
                    DIDeviceMonitor monitor = this.Mediator.DeviceMonitors[key];
                    knownDevices.Add(monitor.DeviceInfo);
                }
            }
            return knownDevices;
        }
        private void cmdOk_Click(object sender, EventArgs e)
        {
            bool valid = ValidateSelections();
            if (valid)
            {
                StoreSelectedControlInfo();
                this.Close();
            }
        }
        private bool ValidateSelections()
        {
            bool valid = true;
            if (rdoJoystick.Checked)
            {
                DIPhysicalControlInfo control = (DIPhysicalControlInfo)cboJoystickControl.SelectedItem;
                if (control.ControlType == ControlType.Pov)
                {
                    if (!rdoPovDown.Checked && !rdoPovDownLeft.Checked && !rdoPovDownRight.Checked && !rdoPovLeft.Checked && !rdoPovRight.Checked && !rdoPovUp.Checked && !rdoPovUpLeft.Checked && !rdoPovUpRight.Checked)
                    {
                        MessageBox.Show(this, "A point-of-view (POV) hat control is selected, but no position on the hat has been selected.\nPlease choose a hat position or change the assigned input control.", System.Windows.Forms.Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        valid = false;
                    }
                }

            }
            else if (rdoKeystroke.Checked)
            {
                if (String.IsNullOrEmpty(txtKeystroke.Text) || txtKeystroke.Text == "None")
                {
                    rdoNotAssigned.Checked = true;
                }
            }
            this.SelectedControl= BuildInputControlSelection();
            return valid;
        }
        private void StoreSelectedControlInfo()
        {
            this.SelectedControl = BuildInputControlSelection();
        }
        private InputControlSelection BuildInputControlSelection()
        {
            InputControlSelection toReturn = new InputControlSelection();
            if (rdoJoystick.Checked)
            {
                DIPhysicalDeviceInfo device = (DIPhysicalDeviceInfo)cbJoysticks.SelectedItem;
                DIPhysicalControlInfo control = (DIPhysicalControlInfo)cboJoystickControl.SelectedItem;
                switch (control.ControlType)
                {
                    case ControlType.Axis:
                        toReturn.ControlType = ControlType.Axis;
                        break;
                    case ControlType.Button:
                        toReturn.ControlType = ControlType.Button;
                        break;
                    case ControlType.Pov:
                        toReturn.ControlType = ControlType.Pov;
                        if (rdoPovUp.Checked)
                        {
                            toReturn.PovDirection = PovDirections.Up;
                        }
                        else if (rdoPovUpLeft.Checked)
                        {
                            toReturn.PovDirection = PovDirections.UpLeft;
                        }
                        else if (rdoPovUpRight.Checked)
                        {
                            toReturn.PovDirection = PovDirections.UpRight;
                        }
                        else if (rdoPovRight.Checked)
                        {
                            toReturn.PovDirection = PovDirections.Right;
                        }
                        else if (rdoPovLeft.Checked)
                        {
                            toReturn.PovDirection = PovDirections.Left;
                        }
                        else if (rdoPovDownLeft.Checked)
                        {
                            toReturn.PovDirection = PovDirections.DownLeft;
                        }
                        else if (rdoPovDownRight.Checked)
                        {
                            toReturn.PovDirection = PovDirections.DownRight;
                        }
                        else if (rdoPovDown.Checked)
                        {
                            toReturn.PovDirection = PovDirections.Down;
                        }
                        break;
                    default:
                        break;
                }
                toReturn.DirectInputDevice = device;
                toReturn.DirectInputControl = control;

            }
            else if (rdoKeystroke.Checked)
            {
                toReturn.ControlType = ControlType.Key;
                toReturn.Keys = (Keys)Enum.Parse(typeof(Keys), txtKeystroke.Text);
            }
            return toReturn;
        }
        private void LoadSelectedControlInfo()
        {
            InputControlSelection thisControlSelection = this.SelectedControl;
            this.lblPromptText.Text = this.PromptText;
            switch (thisControlSelection.ControlType)
            {
                case ControlType.Key:
                    txtKeystroke.Text = thisControlSelection.Keys.ToString();
                    rdoKeystroke.Checked = true;
                    break;
                case ControlType.Button:
                    cbJoysticks.SelectedItem = thisControlSelection.DirectInputDevice;
                    cboJoystickControl.SelectedItem = thisControlSelection.DirectInputControl;
                    rdoJoystick.Checked = true;
                    break;
                case ControlType.Pov:
                    ClearAllPovRadioButtons();
                    cbJoysticks.SelectedItem = thisControlSelection.DirectInputDevice;
                    cboJoystickControl.SelectedItem = thisControlSelection.DirectInputControl;
                    switch (thisControlSelection.PovDirection)
                    {
                        case PovDirections.Up:
                            rdoPovUp.Checked = true;
                            break;
                        case PovDirections.UpRight:
                            rdoPovUpRight.Checked = true;
                            break;
                        case PovDirections.Right:
                            rdoPovRight.Checked = true;
                            break;
                        case PovDirections.DownRight:
                            rdoPovDownRight.Checked = true;
                            break;
                        case PovDirections.Down:
                            rdoPovDown.Checked = true;
                            break;
                        case PovDirections.DownLeft:
                            rdoPovDownLeft.Checked = true;
                            break;
                        case PovDirections.Left:
                            rdoPovLeft.Checked = true;
                            break;
                        case PovDirections.UpLeft:
                            rdoPovUpLeft.Checked = true;
                            break;
                        default:
                            break;
                    }
                    rdoJoystick.Checked = true;
                    break;
                case ControlType.Axis:
                    cbJoysticks.SelectedItem = thisControlSelection.DirectInputDevice;
                    cboJoystickControl.SelectedItem = thisControlSelection.DirectInputControl;
                    rdoJoystick.Checked = true;
                    break;
                case ControlType.Unknown:
                    rdoNotAssigned.Checked = true;
                    break;
                default:
                    break;
            }
            EnableDisableControls();
        }
        private void ClearAllPovRadioButtons()
        {
            rdoPovUp.Checked = false;
            rdoPovUpLeft.Checked = false;
            rdoPovUpRight.Checked = false;
            rdoPovRight.Checked = false;
            rdoPovDownLeft.Checked = false;
            rdoPovDownRight.Checked = false;
            rdoPovDown.Checked = false;
            rdoPovLeft.Checked = false;
        }
        private void PopulateJoysticksComboBox()
        {
            cbJoysticks.Items.Clear();
            if (this.Mediator != null)
            {
                foreach (KeyValuePair<Guid, DIDeviceMonitor> pair in this.Mediator.DeviceMonitors)
                {
                    cbJoysticks.Items.Add(pair.Value.DeviceInfo);
                }
            }
            cbJoysticks.DisplayMember = "Alias";
        }
        private void PopulateJoystickControlsComboBox()
        {
            DIPhysicalDeviceInfo thisDevice = this.SelectedControl.DirectInputDevice;
            cboJoystickControl.Items.Clear();
            if (thisDevice != null && thisDevice.Controls !=null)
            {
                foreach (DIPhysicalControlInfo control in thisDevice.Controls)
                {
                    if (control.ControlType == ControlType.Button || control.ControlType == ControlType.Pov)
                    {
                        cboJoystickControl.Items.Add(control);
                    }
                }
            }
            cboJoystickControl.DisplayMember = "Alias";
        }
        private void SelectCurrentJoystickControl()
        {
            DIPhysicalControlInfo curControl = this.SelectedControl.DirectInputControl;
            if (curControl != null)
            {
                cboJoystickControl.SelectedItem = curControl;
                if (cboJoystickControl.SelectedIndex < 0) cboJoystickControl.SelectedIndex = 0;
            }
            else
            {
                cboJoystickControl.SelectedIndex = 0;
            }
        }
        private void SelectCurrentJoystick()
        {
            DIPhysicalDeviceInfo thisDevice = this.SelectedControl.DirectInputDevice;
            if (thisDevice != null)
            {
                cbJoysticks.SelectedItem = thisDevice;
                if (cbJoysticks.SelectedIndex < 0) cbJoysticks.SelectedIndex = 0;
            }
            else
            {
                cbJoysticks.SelectedIndex = 0;
            }
        }
        private void rdoKeystroke_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }
        private void rdoJoystick_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.SelectedControl = null;
            this.Close();
        }
        private void cbJoysticks_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedControl.DirectInputDevice = (DIPhysicalDeviceInfo)cbJoysticks.SelectedItem;
            PopulateJoystickControlsComboBox();
            SelectCurrentJoystickControl();
        }
        private void cboJoystickControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedControl.DirectInputControl = (DIPhysicalControlInfo)cboJoystickControl.SelectedItem;
            EnableDisableControls();
        }
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_mediator != null)
            {
                _mediator.PhysicalControlStateChanged -= _mediatorHandler;
            }
        }
    }
}
