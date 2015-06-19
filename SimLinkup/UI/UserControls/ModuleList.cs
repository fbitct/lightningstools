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
        private IEnumerable<HardwareSupportModuleBase> _hardwareSupportModules;
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
        public IEnumerable<HardwareSupportModuleBase> HardwareSupportModules
        {
            get { return _hardwareSupportModules; }
            set
            {
                panel.Controls.Clear();
                _hardwareSupportModules = value;
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
    }
}
