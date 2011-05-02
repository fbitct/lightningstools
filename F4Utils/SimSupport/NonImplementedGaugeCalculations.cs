using System;

namespace F4Utils.SimSupport
{
    //CREDIT: Carolus "Falcas" Burgers

    public static class NonImplementedGaugeCalculations
    {
        public static float NOZ(float RPM, float Alt, float FF)
        {
            float NewNoz = 0;
            var AC_Alt = Math.Abs(Alt);

            if (RPM < 58)
            {
                NewNoz = 0;
            }
            else if (RPM >= 58 && RPM < 62)
            {
                NewNoz = (RPM - 58)*25;
            }
            else if (RPM >= 62 && RPM < 68)
            {
                NewNoz = 100;
            }
            else if (RPM >= 68 && RPM <= 70)
            {
                //Reduce the NOZ to around 92%
                NewNoz = (100 - (RPM - 68)*4);
            }
            else if (RPM > 70 && RPM < 83)
            {
                NewNoz = 92 - (RPM - 70)*7.1F;
            }
            else if (RPM >= 83)
            {
                //Get MilFuelFlow
                var Mil = MilFF(AC_Alt);
                var AB = AbFF(AC_Alt);
                var Spread = AB - Mil;

                if (FF < Mil)
                {
                    NewNoz = 0;
                }
                else
                {
                    if (FF > AB)
                    {
                        NewNoz = 100;
                    }
                    else
                    {
                        NewNoz = ((FF - Mil)/Spread)*100;
                    }
                }
            }

            return NewNoz;
        }

        private static int MilFF(float Alt)
        {
            var MilFF = 0;

            if (Alt >= 0 && Alt < 5000)
            {
                MilFF = 13000;
            }
            else if (Alt >= 5000 && Alt < 10000)
            {
                MilFF = 12700;
            }
            else if (Alt >= 10000 && Alt < 15000)
            {
                MilFF = 12000;
            }
            else if (Alt >= 15000 && Alt < 20000)
            {
                MilFF = 10500;
            }
            else if (Alt >= 20000 && Alt < 25000)
            {
                MilFF = 8600;
            }
            else if (Alt >= 25000 && Alt < 30000)
            {
                MilFF = 6800;
            }
            else if (Alt >= 30000 && Alt < 35000)
            {
                MilFF = 5400;
            }
            else if (Alt >= 35000 && Alt < 40000)
            {
                MilFF = 4400;
            }
            else if (Alt >= 40000 && Alt < 45000)
            {
            }
            else if (Alt >= 45000 && Alt < 50000)
            {
            }
            else if (Alt >= 50000 && Alt < 55000)
            {
            }

            return MilFF;
        }

        private static int AbFF(float Alt)
        {
            var AbFF = 0;

            if (Alt >= 0 && Alt < 5000)
            {
                AbFF = 45000;
            }
            else if (Alt >= 5000 && Alt < 10000)
            {
                AbFF = 55000;
            }
            else if (Alt >= 10000 && Alt < 15000)
            {
                AbFF = 45000;
            }
            else if (Alt >= 15000 && Alt < 20000)
            {
                AbFF = 35000;
            }
            else if (Alt >= 20000 && Alt < 25000)
            {
                AbFF = 29000;
            }
            else if (Alt >= 25000 && Alt < 30000)
            {
                AbFF = 24000;
            }
            else if (Alt >= 30000 && Alt < 35000)
            {
                AbFF = 16000;
            }
            else if (Alt >= 35000 && Alt < 40000)
            {
                AbFF = 10000;
            }
            else if (Alt >= 40000 && Alt < 45000)
            {
            }
            else if (Alt >= 45000 && Alt < 50000)
            {
            }
            else if (Alt >= 50000 && Alt < 55000)
            {
            }

            return AbFF;
        }

        public static float Ftit(float FTIT, float RPM)
        {
            float NewFtit = 0;
            var DampingValue = 5;

            if (RPM > 25 && RPM <= 65)
            {
                NewFtit = (RPM - 5)*10;
            }
            else if (RPM > 60 && RPM < 65)
            {
                NewFtit = RPM*10;
            }
            else if (RPM > 65 && RPM <= 70)
            {
                NewFtit = (RPM - (RPM - 65)*5)*10;
            }
            else if (RPM > 70)
            {
                NewFtit = (RPM - 20)*10;
            }
            else
            {
                NewFtit = 200;
            }

            //Dampen the movement.
            if (FTIT - NewFtit < -10)
            {
                NewFtit = FTIT + DampingValue;
            }
            else if (FTIT - NewFtit > 10)
            {
                NewFtit = FTIT - DampingValue;
            }

            if (NewFtit > 770)
            {
                NewFtit = 770;
            }

            return NewFtit;
        }

        public static long CabinAlt(float OrigCabinAlt, float z, bool Pressurization)
        {
            //Create a value for the cabin alt, depending on the aircraft alt and
            //the cabin presss warning bit.
            var AircraftAlt = Math.Abs(z);
            double CabinAlt = 0;
            double Temp = 0;
            var DampingValue = 33;

            if (!Pressurization)
            {
                //Cabin press is not maintained,
                //Cabin alt is the same as aircraft alt.

                //Dampen the movement.
                if (OrigCabinAlt - AircraftAlt < -100)
                {
                    CabinAlt = OrigCabinAlt + DampingValue;
                }
                else if (OrigCabinAlt - AircraftAlt > 100)
                {
                    CabinAlt = OrigCabinAlt - DampingValue;
                }
                else
                {
                    CabinAlt = AircraftAlt;
                }
            }
            else
            {
                //Cabin press is maintained,
                //below 8000', Cabin alt is aircraft alt.
                //between 8000' and 23000' Cabin alt remains 8000'
                //above 23000' Cabin alt climbs as scheduled.
                if (AircraftAlt <= 8000)
                {
                    CabinAlt = AircraftAlt;
                }
                else if (AircraftAlt > 8000 && AircraftAlt <= 23000)
                {
                    CabinAlt = 8000;
                }
                else if (AircraftAlt > 23000)
                {
                    //Cabin press is not maintained,
                    //Cabin alt is the same as aircraft alt.

                    Temp = Math.Sqrt(AircraftAlt - 23000)*68 + 8000;

                    //Dampen the movement.
                    if (OrigCabinAlt - Temp < -1000)
                    {
                        CabinAlt = OrigCabinAlt + DampingValue;
                    }
                    else if (OrigCabinAlt - Temp > 1000)
                    {
                        CabinAlt = OrigCabinAlt - DampingValue;
                    }
                    else
                    {
                        CabinAlt = Temp;
                    }
                }
            }

            return Convert.ToInt64(CabinAlt);
        }

        public static int HydA(float RPM,
                               bool MainGen,
                               bool StbyGen,
                               bool EpuGen,
                               bool EPU_On,
                               float EPU_Fuel)
        {
            //HydA is linked to the Engine and
            //the indicator powered by the Emergency AC bus nr 2.
            //Em AC Bus nr 2 gets power from the Essential AC bus or EPU GEN.
            //Essential AC bus gets power from the NonEssential AC bus Nr 2 or the STBY GEN.
            //The NonEssential AC bus gets power from the Main GEN.
            //That means that the power is supplied when the Main GEN comes online.
            //To simulate that let the Hyd A rise when the RPM is between 55 and 65
            float HydA = 0;
            var HydA_High = 2900;
            float RPM_Hyd = 0;
            var Elec = false;

            //First check if we have elec power for the instrument.
            //Elec and Hyd power can come from EPU so check if the EPU is running by EPUon Lightbit and
            //if there is EPU fuel.
            if (EPU_Fuel > 0 && EPU_On && !EpuGen)
            {
                Elec = true;
            }
            else
            {
                if (RPM > 55)
                    if (MainGen)
                    {
                        //True means warning light is on, Gen is off
                        if (StbyGen)
                        {
                            //True means warning light is on, Gen is off
                            Elec = false;
                        }
                        else
                        {
                            Elec = true;
                        }
                    }
                    else
                    {
                        if (StbyGen)
                        {
                            //True means warning light is on, Gen is off
                            Elec = false;
                        }
                        else
                        {
                            Elec = true;
                        }
                    }
                else
                {
                    Elec = false;
                }
            }

            //Now that we know if we have Elec or not.
            //Create a Hyd value.
            if (Elec)
            {
                if (RPM > 55 && RPM < 65)
                {
                    RPM_Hyd = RPM - 55;
                    HydA = (HydA_High/10)*RPM_Hyd;
                }
                else
                {
                    //Hyd press is supplied by the EPU
                    HydA = HydA_High;
                }
            }
            else
            {
                HydA = 0;
            }

            return Convert.ToInt16(HydA);
        }

        public static int HydB(float RPM,
                               bool MainGen,
                               bool StbyGen,
                               bool EpuGen,
                               bool EPU_On,
                               float EPU_Fuel)
        {
            //HydA is linked to the Engine and
            //the indicator powered by the Emergency AC bus nr 2.
            //Em AC Bus nr 2 gets power from the Essential AC bus or EPU GEN.
            //Essential AC bus gets power from the NonEssential AC bus Nr 2 or the STBY GEN.
            //The NonEssential AC bus gets power from the Main GEN.
            //That means that the power is supplied when the Main GEN comes online.
            //To simulate that let the Hyd B rise when the RPM is between 55 and 65
            float HydB = 0;
            var HydB_High = 3100;
            float RPM_Hyd = 0;
            var Elec = false;

            //Check if we have elec power.
            //First check if we have elec power for the instrument.
            //Elec can come from EPU so check if the EPU is running by EPUon Lightbit and
            //if there is EPU fuel.
            if (EPU_Fuel > 0 && EPU_On && !EpuGen)
            {
                Elec = true;
            }
            else
            {
                if (RPM > 55)
                {
                    if (MainGen)
                    {
                        //True means warning light is on, Gen is off
                        if (StbyGen)
                        {
                            //True means warning light is on, Gen is off
                            Elec = false;
                        }
                        else
                        {
                            Elec = true;
                        }
                    }
                    else
                    {
                        if (StbyGen)
                        {
                            //True means warning light is on, Gen is off
                            Elec = false;
                        }
                        else
                        {
                            Elec = true;
                        }
                    }
                }
                else
                {
                    Elec = false;
                }
            }

            if (Elec)
            {
                if (RPM > 55 && RPM < 65)
                {
                    RPM_Hyd = RPM - 55;
                    HydB = (HydB_High/10)*RPM_Hyd;
                }
                else if (RPM > 65)
                {
                    //If the engine is running we have Hyd press.
                    HydB = HydB_High;
                }
                else
                {
                    //Hyd B is not powered by the EPU, so the indicator has
                    //elec power and the Hyd press goes to zero
                    HydB = 0;
                }
            }
            return Convert.ToInt16(HydB);
        }
    }
}