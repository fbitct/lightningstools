using F16CPD.Mfd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Menus
{
    internal interface IControlMapMenuPageFactory
    {
        MfdMenuPage BuildControlMapMenuPage();
    }
    class ControlMapMenuPageFactory:IControlMapMenuPageFactory
    {
        private F16CpdMfdManager _mfdManager;
        private IOptionSelectButtonFactory _optionSelectButtonFactory;
        public ControlMapMenuPageFactory(
            F16CpdMfdManager mfdManager,
            IOptionSelectButtonFactory optionSelectButtonFactory = null
        )
        {
            _mfdManager = mfdManager;
            _optionSelectButtonFactory = optionSelectButtonFactory ?? new OptionSelectButtonFactory();
        }
        public MfdMenuPage BuildControlMapMenuPage()
        {
            var thisPage = new MfdMenuPage(_mfdManager);
            var buttons = new List<OptionSelectButton>
                              {
                                  _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 1, "CNTL", true),
                                  _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 2, "PROF", false)
                              };

            var controlMapPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 3, "MAP", true);
            controlMapPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToControlMapPage();
            buttons.Add(controlMapPageSelectButton);

            var controlOverlayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 4, "OVR", false);
            controlOverlayPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToControlOverlayPage();
            buttons.Add(controlOverlayPageSelectButton);

            var testPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTestPage();
            buttons.Add(testPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7, "DEL\n\rCIB", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 8, "DEL\n\rMAP", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 9, "DEL\n\rDRW", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 10, "DEL\n\rSHP", false));

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

            var targetingPodPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTargetingPodPage();
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToInstrumentsPage();
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 21, "LOAD\n\rECHM", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 22, "LOAD\n\rSHP", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 23, "LOAD\n\rDRW", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 24, "LOAD\n\rMAP", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 25, "LOAD\n\rCIB", false));

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Control Map Page";
            return thisPage;
        }
    }
}
