using F16CPD.Mfd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Menus
{
    internal interface ITADMenuPageFactory
    {
        MfdMenuPage BuildTADMenuPage();
    }
    class TADMenuPageFactory:ITADMenuPageFactory
    {
        private F16CpdMfdManager _mfdManager;
        private IOptionSelectButtonFactory _optionSelectButtonFactory;
        public TADMenuPageFactory(
            F16CpdMfdManager mfdManager,
            IOptionSelectButtonFactory optionSelectButtonFactory = null
        )
        {
            _mfdManager = mfdManager;
            _optionSelectButtonFactory = optionSelectButtonFactory ?? new OptionSelectButtonFactory();
        }
        public MfdMenuPage BuildTADMenuPage()
        {
            var thisPage = new MfdMenuPage(_mfdManager);
            var buttons = new List<OptionSelectButton>();

            var controlMapPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 1, "CNTL", false);
            controlMapPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToControlMapPage();
            buttons.Add(controlMapPageSelectButton);

            var chartPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 2, "CHARTS", false);
            chartPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToChartsPage();
            buttons.Add(chartPageSelectButton);

            var checklistPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToChecklistsPage();
            buttons.Add(checklistPageSelectButton);

            var mapOnOffButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 4, "MAP", true);
            mapOnOffButton.Pressed += (s,e)=>_mfdManager.SwitchToTADPage();
            buttons.Add(mapOnOffButton);

            var testPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 5, "TEST", false);
            testPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTestPage();
            buttons.Add(testPageSelectButton);

            var scaleIncreaseButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7, "^", false);
            scaleIncreaseButton.Pressed += (s,e)=>_mfdManager.IncreaseMapScale();
            buttons.Add(scaleIncreaseButton);

            var scaleLabel = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7.5f,
                                                      "CADRG\r\n" +
                                                      _mfdManager.GetCADRGScaleTextForMapScale(_mfdManager.MapScale), false);
            scaleLabel.FunctionName = "MapScaleLabel";
            buttons.Add(scaleLabel);

            var scaleDecreaseButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 8, @"\/", false);
            scaleDecreaseButton.Pressed += (s, e) => _mfdManager.DecreaseMapScale();
            buttons.Add(scaleDecreaseButton);
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 9, "CNTR\r\nOWN", false));
            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 10, "CLR MSG", false));

            var imagingPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 14, "IMG", false);
            imagingPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToImagingPage();
            buttons.Add(imagingPageSelectButton);

            var messagingPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 15, "MSG", false);
            messagingPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToMessagePage();
            buttons.Add(messagingPageSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTADPage();
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToTargetingPodPage();
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += (s, e) => _mfdManager.SwitchToInstrumentsPage();
            buttons.Add(headDownDisplayPageSelectButton);

            buttons.Add(_optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 22, "CAP", false));

            var mapRangeIncrease = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 25, @"^", false);
            mapRangeIncrease.Pressed += (s,e)=>_mfdManager.IncreaseMapRange();
            buttons.Add(mapRangeIncrease);

            var mapRangeDecrease = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 24, @"\/", false);
            mapRangeDecrease.Pressed += (s,e)=>_mfdManager.DecreaseMapRange();
            buttons.Add(mapRangeDecrease);

            var mapRangeLabel = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 24.5f,
                                                         _mfdManager.MapRangeRingsDiameterInNauticalMiles.ToString(),
                                                         false);
            mapRangeLabel.FunctionName = "MapRangeLabel";
            buttons.Add(mapRangeLabel);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "TAD Page";
            return thisPage;
        }
       

    }
}
