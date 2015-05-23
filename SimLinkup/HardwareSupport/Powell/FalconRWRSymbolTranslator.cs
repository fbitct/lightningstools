using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLinkup.HardwareSupport.Powell
{
    internal interface IFalconRWRSymbolTranslator
    {
        IEnumerable<RWRSymbol> TranslateFalconRWRSymbolToPowellSymbolList(FalconRWRSymbol falconRWRSymbol, bool primarySymbol);
        
    }
    internal class FalconRWRSymbolTranslator : IFalconRWRSymbolTranslator
    {
        public IEnumerable<RWRSymbol> TranslateFalconRWRSymbolToPowellSymbolList(FalconRWRSymbol falconRWRSymbol,bool primarySymbol)
        {
            var powellSymbols = new List<RWRSymbol>();
            var primarySymbolXPosition = (byte)0x00;
            var primarySymbolYPosition = (byte)0x00;
            var lineSpacingYOffset = (byte)0x00;
            var charSpacingXOffset = (byte)0x00;
            var charSpacingHalfXOffset = (byte)0x00;
            switch (falconRWRSymbol.SymbolID)
            {
                case 0:
                    break;
                case 1: //U
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition=primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 2: //advanced interceptor
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 25, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 3: //basic interceptor
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 26, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 4: //M
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 16, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 5: //H
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 15, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 6: //P
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 7: //2
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 8: //3
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 3, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 9: //4
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 4, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 10: //5
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 5, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 11: //6
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 6, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 12: //8
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 8, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 13: //9
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 9, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 14: //10
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 0, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    break;
                case 15: //13
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 3, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    break;
                case 16: //alternating A or S
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 17: //S
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 18: //ship
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 24, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 19: //C
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 12, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 20: //alternating 15 or M
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 5, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 16, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 21: //N
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 17, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 22: //A with . underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 23: //A with .. underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition - charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition + charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 24: //A with ... underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition - charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition + charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 25: //P with . underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 26: //P|
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    break;
                case 27: //U with . underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 28: //U with .. underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition - charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition + charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 29: //U with ... underneath
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition - charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 21, XPosition = (byte)(primarySymbolXPosition + charSpacingHalfXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break; 
                case 30: //C
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 12, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 31: //airborne threat symbol with 1 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 32://airborne threat symbol with 4 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 4, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 33: //airborne threat symbol with 5 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 5, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 34: //airborne threat symbol with 6 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 6, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 35: //airborne threat symbol with 14 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 4, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 36://airborne threat symbol with 15 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 5, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 37://airborne threat symbol with 16 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 6, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 38: //airborne threat symbol with 18 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 8, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 39: //airborne threat symbol with 19 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 9, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 40: //airborne threat symbol with 20 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 0, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 41: //airborne threat symbol with 21 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 42: //airborne threat symbol with 22 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 43: //airborne threat symbol with 23 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 3, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 44: //airborne threat symbol with 25 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 5, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 45: //airborne threat symbol with 27 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 7, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 46: //airborne threat symbol with 29 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 2, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 9, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 47: //airborne threat symbol with 30 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 3, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 0, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 48: //airborne threat symbol with 31 inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 3, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 1, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 49: //airborne threat symbol with P inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 50: //airborne threat symbol with PD inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 13, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 51: //airborne threat symbol with A inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 52: //airborne threat symbol with B inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 11, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 53: //airborne threat symbol with S inside
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = (byte)(primarySymbolYPosition + lineSpacingYOffset) });
                    break;
                case 54: //A|
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    break;
                case 55: //|A|
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition - charSpacingXOffset), YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    break;
                case 56: //||| with A overlaid
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition - charSpacingXOffset), YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = (byte)(primarySymbolXPosition + charSpacingXOffset), YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 33, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 57: // alternating F and S
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 58://alternating F and A
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 59://alternating F and M
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 16, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 60: //alternating F and U
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 61: //alternating F and basic interceptor symbol
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 26, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 62: //alternating S and basic interceptor symbol
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 26, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 63: //alternating A and basic interceptor symbol
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 26, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 64: //alternating M and basic interceptor symbol
                    if (primarySymbol)
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 16, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    else
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 26, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    }
                    break;
                case 65://A
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 10, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 66://B
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 11, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 67://C
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 12, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 68://D
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 13, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 69://E
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 14, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 70://F
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 34, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 71://G
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 35, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 72://H
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 15, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 73://I
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 36, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 74://J
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 37, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 75://K
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 38, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 76://L
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 39, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 77://M
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 16, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 78://N
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 17, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 79://O
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 40, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 80://P
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 18, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 81://Q
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 41, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 82://R
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 42, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 83://S
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 19, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 84://T
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 43, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 85://U
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 86://V
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 44, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 87://W
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 45, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 88://X
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 46, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 89://Y
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 47, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 90://Z
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 48, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 91://LBRACKET [
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 49, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 92://BACKSLASH \
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 50, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 93://RBRAKCET ]
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 51, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 94://HAT
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 23, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 95://UNDERSCORE _
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 22, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 96://BACKTICK `
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 52, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 97://a
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 53, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 98://b
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 54, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;
                case 99://c
                    powellSymbols.Add(new RWRSymbol { SymbolNumber = 55, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                    break;

                default:
                    if (falconRWRSymbol.SymbolID < 0) //show U for Unknwon Threat
                    {
                        powellSymbols.Add(new RWRSymbol { SymbolNumber = 20, XPosition = primarySymbolXPosition, YPosition = primarySymbolYPosition });
                        break;
                    }
                    else if (falconRWRSymbol.SymbolID >= 100)
                    {
                        //subtract 100 from the number, and that's the Number to display as digits
                        var numberToDisplay = falconRWRSymbol.SymbolID - 100;
                        var digits = numberToDisplay.ToString();
                        for (var i = 0; i < digits.Length; i++)
                        {
                            var thisDigit=(byte)Int32.Parse(digits.Substring(i,1));
                            powellSymbols.Add(new RWRSymbol { SymbolNumber = thisDigit, XPosition = (byte)(primarySymbolXPosition + (i * charSpacingXOffset)), YPosition = primarySymbolYPosition });
                        }
                    }
                    
                    break;
            }
            return powellSymbols;
/*
* 
* POWELL SYMBOLS
* 0="0"
1="1"
2="2"
3="3"
4="4"
5="5"
6="6"
7="7"
8="8"
9="9"
10="A"
11="B"
12="C"
13="D"
14="E"
15="H"
16="M"
17="N"
18="P"
19="S"
20="U"
21="."
22="_"
23=HAT
24=BOAT
25=new/modern aircraft symbol (looks like stealth fighter)
26=older aircraft symbol (loks like stealth bomber)
27=circle
28=diamond
29=box
30=box
31=box
 
 * 32=BLINK BIT (old)
 * 
 * NEW SYMBOLS NEEDED
 * 33=Vertical line 
 * 34=F 
 * 35=G
 * 36=I
 * 37=J
 * 38=K
 * 39=L
 * 40=O
 * 41=Q
 * 42=R
 * 43=T
 * 44=V
 * 45=W
 * 46=X
 * 47=Y
 * 48=Z
 * 49=LBRACKET [
 * 50=BACKSLASH \
 * 51=RBRACKET ]
 * 52=BACKTICK `
 * 53=lowercase a 
 * 54=lowercase b
 * 55=lowercase c
*/

        }
    }
}
