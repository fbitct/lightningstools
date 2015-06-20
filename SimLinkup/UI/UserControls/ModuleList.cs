using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.SimSupport;
using Common.HardwareSupport;

namespace SimLinkup.UI.UserControls
{
    public partial class ModuleList : UserControl
    {
        private IEnumerable<SimSupportModule> _simSupportModules;
        private IEnumerable<IHardwareSupportModule> _hardwareSupportModules;
        public ModuleList()
        {
            InitializeComponent();
        }
        public IEnumerable<SimSupportModule> SimSupportModules
        {
            get { return _simSupportModules; }
            set
            {
                panel.Controls.Clear();
                _simSupportModules = value;
                if (_simSupportModules == null)
                {
                    return;
                }
                foreach (var ssm in _simSupportModules)
                {
                    var visualizer = new ModuleBasicVisualizer();
                    visualizer.ModuleName = ssm.FriendlyName;
                    visualizer.ShowSignals += (s, e) =>
                    {

                    };
                    panel.Controls.Add(visualizer);
                }
            }
        }
        public IEnumerable<IHardwareSupportModule> HardwareSupportModules
        {
            get { return _hardwareSupportModules; }
            set
            {
                panel.Controls.Clear();
                _hardwareSupportModules = value;
                if (_hardwareSupportModules == null)
                {
                    return;
                }
                foreach (var ssm in _hardwareSupportModules)
                {
                    var visualizer = new ModuleBasicVisualizer();
                    visualizer.ModuleName = ssm.FriendlyName;
                    visualizer.ShowSignals += (s, e) =>
                    {

                    };
                    panel.Controls.Add(visualizer);
                }
            }
        }
    }
}
