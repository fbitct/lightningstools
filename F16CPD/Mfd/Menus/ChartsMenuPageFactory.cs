using F16CPD.Mfd.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.Mfd.Menus
{
    internal interface IChartsMenuPageFactory
    {
        MfdMenuPage BuildChartsMenuPage();
    }
    class ChartsMenuPageFactory:IChartsMenuPageFactory
    {
        private F16CpdMfdManager _mfdManager;
        private IOptionSelectButtonFactory _optionSelectButtonFactory;
        public ChartsMenuPageFactory(
            F16CpdMfdManager mfdManager,
            IOptionSelectButtonFactory optionSelectButtonFactory= null
        )
        {
            _mfdManager = mfdManager;
            _optionSelectButtonFactory = optionSelectButtonFactory ?? new OptionSelectButtonFactory();
        }

        public MfdMenuPage BuildChartsMenuPage()
        {
            var thisPage = new MfdMenuPage(_mfdManager);
            var buttons = new List<OptionSelectButton>();

            var chartPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 2, "CHARTS", true);
            chartPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToChartsPage();
            buttons.Add(chartPageSelectButton);

            var checklistPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 3, "CHKLST", false);
            checklistPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToChecklistsPage();
            buttons.Add(checklistPageSelectButton);

            var mapPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 4, "MAP", false);
            mapPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToTADPage();
            buttons.Add(mapPageSelectButton);

            var previousChartFileSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 7, "^", false);
            previousChartFileSelectButton.Pressed += (s,e)=>_mfdManager.PrevChartFile();
            buttons.Add(previousChartFileSelectButton);

            var currentChartFileLabel = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 8, "NO CHART\nFILES", false);
            currentChartFileLabel.FunctionName = "CurrentChartFileLabel";
            buttons.Add(currentChartFileLabel);

            var nextChartFileSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 9, @"\/", false);
            nextChartFileSelectButton.Pressed += (s,e)=>_mfdManager.NextChartFile();
            buttons.Add(nextChartFileSelectButton);

            var tacticalAwarenessDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 16, "TAD",
                                                                                    true);
            tacticalAwarenessDisplayPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToTADPage();
            buttons.Add(tacticalAwarenessDisplayPageSelectButton);

            var targetingPodPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 17, "TGP", false);
            targetingPodPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToTargetingPodPage();
            buttons.Add(targetingPodPageSelectButton);

            var headDownDisplayPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 18, "HDD", false);
            headDownDisplayPageSelectButton.Pressed += (s,e)=>_mfdManager.SwitchToInstrumentsPage();
            buttons.Add(headDownDisplayPageSelectButton);


            var nextChartPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 23, @"\/", false);
            nextChartPageSelectButton.Pressed += (s,e)=>_mfdManager.NextChartPage();
            buttons.Add(nextChartPageSelectButton);

            var currentChartPageNumLabel = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 24, "page", false);
            currentChartPageNumLabel.FunctionName = "CurrentChartPageNumLabel";
            buttons.Add(currentChartPageNumLabel);

            var prevChartPageSelectButton = _optionSelectButtonFactory.CreateOptionSelectButton(thisPage, 25, @"^", false);
            prevChartPageSelectButton.Pressed += (s,e)=>_mfdManager.PrevChartPage();
            buttons.Add(prevChartPageSelectButton);

            thisPage.OptionSelectButtons = buttons;
            thisPage.Name = "Charts Page";
            return thisPage;
        }
    }
}
