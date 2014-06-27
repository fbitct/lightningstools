using F16CPD.Mfd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Menus
{
    internal interface IBitmapAnnotationMenuPageFactory
    {
        MfdMenuPage BuildBitmapAnnotationMenuPage();
    }
    class BitmapAnnotationMenuPageFactory:IBitmapAnnotationMenuPageFactory
    {
        private F16CpdMfdManager _mfdManager;
        private IOptionSelectButtonFactory _optionSelectButtonFactory;
        public BitmapAnnotationMenuPageFactory(
            F16CpdMfdManager mfdManager,
            IOptionSelectButtonFactory optionSelectButtonFactory= null
        )
        {
            _mfdManager = mfdManager;
            _optionSelectButtonFactory = optionSelectButtonFactory ?? new OptionSelectButtonFactory();
        }

        public MfdMenuPage BuildBitmapAnnotationMenuPage()
        {
            var thisPage = new MfdMenuPage(_mfdManager);
            var buttons = new List<OptionSelectButton>
                              {
                                  _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 2, "TO\n\rINTEL", false),
                                  _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 4, "OWN\n\rFRISCO", false)
                              };

            var testPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToTestPage();
            buttons.Add(testPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7, "^", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7.5f, "16102F\n\r16UKWN0", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 8, @"\/", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 9, "SEND", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 10, "DATA\n\rMODE", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 11, "DEL", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 12, "DEL ALL", false));


            var imagingPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 14, "IMG", true);
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

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 22, "CAP", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 23, "X1", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 24, "TO USB", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 25, "TO MFCD", false));
            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Bitmap Annotation Page";
            return thisPage;
        }


    }
}
