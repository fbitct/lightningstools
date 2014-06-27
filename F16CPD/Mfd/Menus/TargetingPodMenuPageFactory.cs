using F16CPD.Mfd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Menus
{
    internal interface ITargetingPodMenuPageFactory
    {
        MfdMenuPage BuildTargetingPodMenuPage();
    }
    class TargetingPodMenuPageFactory:ITargetingPodMenuPageFactory
    {
        private F16CpdMfdManager _mfdManager;
        private IOptionSelectButtonFactory _optionSelectButtonFactory;
        public TargetingPodMenuPageFactory(
            F16CpdMfdManager mfdManager,
            IOptionSelectButtonFactory optionSelectButtonFactory = null
        )
        {
            _mfdManager = mfdManager;
            _optionSelectButtonFactory = optionSelectButtonFactory ?? new OptionSelectButtonFactory();
        }
        public MfdMenuPage BuildTargetingPodMenuPage()
        {
            var thisPage = new MfdMenuPage(_mfdManager);
            var buttons = new List<OptionSelectButton>();

            var testPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToTestPage();
            buttons.Add(testPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            var imagingPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToImagingPage();
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToMessagePage();
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    false);
            tacticalAwarenessDisplayPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTADPage();
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 17, "TGP", true);
            targetingPodPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTargetingPodPage();
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToInstrumentsPage();
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 22, "CAP", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Targeting Pod Page";
            return thisPage;
        }
    }
}
