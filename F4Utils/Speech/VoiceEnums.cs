﻿using System;
using System.Collections.Generic;
using System.Text;

namespace F4Utils.Speech
{
    public enum RadioComm
    {
        MergePlot = 0,
        AAA = 1,
        Abort= 2,
        AirBDA= 3,
        AirCoverSent= 4,
        AirDropApproach= 5,
        AirDropOne= 6,
        AirManDownA= 7,
        AirManDownB = 8,
        AirManDownD= 9,
        AirManDownE = 10,
        AirManDownF= 11,
        Airspeed= 12,
        AirTargetBRA= 13,
        AltLanding = 14,
        Approach= 15,
        SAMUp= 16,
        AttackTargetsAtMyNose= 17,
        AttackBrief= 18,
        AttackingA= 19,
        AttackMyTarget= 20,
        AttackSecondary= 21,
        AWACSDivert = 22,
        AWACSOff = 23,
        AWACSOn= 24,
        BanditUpdate= 25,
        BaseUnderAttack= 26,
        OutsideAirspeed = 27,
        BogieDope= 28,
        BoomCommands= 29,
        Break= 30,
        BreakAway= 31,
        BuddySpike = 32,
        BVRThreatWarn= 33,
        ChangeAlt= 34,
        CheckSix= 35,
        ClearedEmergLand = 36,
        Missed= 37,
        ClearedLand= 38,
        ClearedOnRunway= 39,
        ClearSix= 40,
        ClearToContact= 41,
        CloseThreat= 42,
        CloseUp= 43,
        Contact = 44,
        Cover= 45,
        DamageReport= 46,
        Disconnect= 47,
        DisruptingTraffic= 48,
        DivertDirective = 49,
        DivertField= 50,
        DoneFueling= 51,
        ECMOff = 52,
        ECMOn= 53,
        EndCAPARMS = 54,
        EndCAPFUEL = 55,
        EndDivertDirective= 56,
        EnemyLaunch= 57,
        EnemyCrash= 58,
        EngagedDirective= 59,
        EngageGroundTarget= 60,
        EngagingA= 61,
        EngagingB = 62,
        EngagingC = 63,
        EngDefensiveA= 64,
        EngDefensiveB = 65,
        EngDefensiveC = 66,
        Execute = 67,
        ExecuteResponse= 68,
        FACContact= 69,
        NoTargets = 70,
        FACReady = 71,
        FACSit = 72,
        Firing = 73,
        FlightIn= 74,
        FlightOff = 75,
        FlightRTB = 76,
        FlightWarning= 77,
        FormResponseA = 78,
        FormResponseB = 79,
        FriendlyFire= 80,
        FuelCheckRsp = 81,
        FuelCheck= 82,
        GeneralResponseC= 83,
        GetOffRunwayA= 84,
        GetOffRunwayB = 85,
        GndTargetBr= 86,
        GoFormation = 87,
        GotLink= 88,
        GroundFire= 89,
        HelpDamaged = 90,
        HelpNow = 91,
        HOLDATCP = 92,
        HOLDFIRE = 93,
        HOLDPATTERN = 94,
        HOLDSHORT = 95,
        HURRYUP = 96,
        IMADOT = 97,
        INBOUND = 98,
        INPOSITION = 99,
        INTERCEPTCOURSE = 100,
        COPY = 101,
        KICKOUT = 102,
        LANDCLEARANCE = 103,
        LANDCLEAREMERGENCY = 104,
        DECOYS = 105,
        LATE = 106,
        LEAKERS = 107,
        LEFT = 108,
        LIFTOFF = 109,
        LOOKFORAIR = 110,
        LOOKGROUNDTARGETS = 111,
        LOOKINGFORBOGIES = 112,
        LOOKINGFORGROUND = 113,
        LZCLEAN = 114,
        MARK = 115,
        MONITORAIRTARGET = 116,
        MOVERBDA = 117,
        NAV = 118,
        SCUDLAUNCH = 119,
        NEARESTTARGET = 120,
        NEARESTTHREATRSP = 121,
        NEEDHELP = 122,
        NOAIRCOVER = 123,
        NOBOGIEDOPE = 124,
        NOJOY = 125,
        NOTCHING = 126,
        ONMYWAY = 127,
        ONRUNWAY = 128,
        PACKATJOIN = 129,
        REQUESTTASK = 130,
        PACKBDAQUERY = 131,
        PACKCALLRESPONSE = 132,
        PACKDEPARTING = 133,
        PACKFUELED = 134,
        PACKJOINED = 135,
        PACKROLECALL = 136,
        PACKTOTCHANGE = 137,
        PICTUREBULL = 138,
        PICTURECLEAR = 139,
        PICTUREQUERY = 140,
        PILOTCAPTURED = 141,
        PILOTDEAD = 142,
        PILOTHIT = 143,
        PILOTHITA = 144,
        NOTASKING = 145,
        PILOTHITC = 146,
        PILOTHITD = 147,
        PILOTRECOVERED = 148,
        NOBDA = 149,
        PLAYERJOINS = 150,
        POSITIONRESPONSEA = 151,
        POSITIONRESPONSEB = 152,
        POSTESCORT = 153,
        JUDY = 154,
        POSTSEAD = 155,
        POSTSTRIKE = 156,
        PRECONTACT = 157,
        PRESSON = 158,
        PRIMARYDESTROYED = 159,
        PRIMARYHIT = 160,
        RADARALTRESPONSE = 161,
        RADARCONTACT = 162,
        GUNS = 163,
        RADAROFF = 164,
        RADARON = 165,
        RADARSCAN = 166,
        RADARTRACKUPDATE = 167,
        RADARUPDATE = 168,
        READYTOFUEL = 169,
        REARMED = 170,
        REATTACKQUERY = 171,
        RECONED = 172,
        REJOIN = 173,
        REQUESTFUEL = 174,
        REQUESTTOENGAGE = 175,
        REQUESTTOFIRE = 176,
        RESUME = 177,
        RIGHT = 178,
        ROGER = 179,
        RTB = 180,
        SAM = 181,
        SAMACTIVITY = 182,
        SANITIZELZ = 183,
        SARENROUTE = 184,
        SENDCHOPPERS = 185,
        SENDDAMAGEREPORT = 186,
        SENDINGSEAD = 187,
        SENDSEADLINK = 188,
        SHOOTER = 189,
        SLOW = 190,
        SORTRESPONSE = 191,
        SPIKE = 192,
        STABALIZE = 193,
        STACKHIGH = 194,
        STACKLEVEL = 195,
        STACKLOW = 196,
        STATICBDA = 197,
        STATUS = 198,
        STRANGER = 199,
        STRIPING = 200,
        SUCCESS = 201,
        SWEEP = 202,
        TACANOFF = 203,
        TACANON = 204,
        TAKEOFFCLEARANCE = 205,
        TAKINGOFF = 206,
        TALLYAIR = 207,
        HARTSOPEN = 208,
        TALLYGROUND = 209,
        TALLYTARGET = 210,
        TANKERTURN = 211,
        TARGETDETECT = 212,
        TARGETLOCKED = 213,
        TAXI = 214,
        SCRAMBLE = 215,
        TIMESUP = 216,
        TOUCHDOWN = 217,
        TURN = 218,
        UNABLE = 219,
        USEALTFIELD = 220,
        VAMOOSE = 221,
        VECTORALTERNATE = 222,
        VECTORHOME = 223,
        VECTORHOMEPLATE = 224,
        VECTORTOFLIGHT = 225,
        VECTORTOFLIGHTREQ = 226,
        VECTORTOPACKAGE = 227,
        VECTORTOTARGET = 228,
        VECTORTOTHREAT = 229,
        WEAPONCHANGE = 230,
        WEAPONSCHECKRSP = 231,
        WEAPONSCHECK = 232,
        WEAPONSFREE = 233,
        WHEREAREYOU = 234,
        WINGCRASH = 235,
        PICTUREBRA = 236,
        RELIEVED = 237,
        CAPNOTOVER = 238,
        DISMISSED = 239,
        CASMISSION = 240,
        GOCASMISSION = 241,
        RECEIVERCALLSIGN = 242,
        SENDERCALLSIGN = 243,
        FLIGHTCALLSIGN = 244,
        SHORTCALLSIGN = 245,
        WEAPONSLOW = 246,
        FIREAMRAAM = 247,
        PLAYERAAKILL = 248,
        TRESPASS = 249,
        LASTWORDS = 250,
        SHOTWINGMAN = 251,
        SLAPSHOT = 252,
        SNIPER = 253,
        HARTSTARGET = 254,
        CHECKIN = 255,
        HTSCLEAR = 256,
        TAGSAM = 257,
        FENCECHECK = 258,
        TUMBLEWEED = 259,
        NOTANKER = 260,
        REPEAT = 261,
        SENDTHREATS = 262,
        SANDBULL = 263,
        SANDBRA = 264,
        SANDBRA2 = 265,
        AIRTARGETBULL = 266,
        SAMUPBULL = 267,
        ATTACKTARGETBULL = 268,
        FACATTACKBULL = 269,
        BVRTHRTWARNBULL = 270,
        ENGAGEDIRECTIVEBULL = 271,
        ENGAGEGNDBULL = 272,
        GNDTARGETBULL = 273,
        NEARESTTHREATBULL = 274,
        POSITBULL = 275,
        RADARCONTACTBULL = 276,
        HARTSOPENBULL = 277,
        TALLYGNDBULL = 278,
        SCRAMBLEBULL = 279,
        VECTORALTBULL = 280,
        TAGSAMBULL = 281,
        SCUDLAUNCHBULL = 282,
        BDA = 283,
        CLEARTOTAXI = 284,
        CVA = 285,
        ContinueInbound1 = 286,
        ContinueInbound2= 287,
        ContinueInbound3 = 288,
        ATCAltitude= 289,
        ATCVectors= 290,
        ATCOrbit1 = 291,
        ATCOrbit2 = 292,
        VectorToTanker= 293,
        TurnToFinal = 294,
        ATCSCOLD1 = 295,
        ATCSCOLDTRAFFIC = 296,
        ATSCOLDVECTOR = 297,
        ATCTRAFFICWARNING = 298,
        ATCTRAFFICWARNING2 = 299,
        ATCFOLLOWTRAFIC = 300,
        ATCLANDSEQUENCE = 301,
        ATCGOAROUND = 302,
        ATCGOAROUND2 = 303,
        CLEAREDDEPARTURE = 304,
        TAXICLEAR = 305,
        POSITIONANDHOLD = 306,
        EXPEDITEDEPARTURE = 307,
        ATCCANCELMISSION = 308,
        TOWERSCOLD1 = 309,
        TAXISEQUENCE = 310,
        TOWERSCOLD2 = 311,
        TOWERSCOLD3 = 312,
        RAYGUN = 313,
        DECLARE = 314,
        LOSTPADLOCK = 315,
        ONMYTAIL = 316,
        REQUESTTAKEOFFCLEARANCE = 317,
        CUTS = 318,
        REQUESTVECTORTOTANKER = 319,
        TARGETGROUPBRA = 320,
        TARGETGROUPBULL = 321,
        READYFORDERARTURE = 322,
        GENERALID = 323,
        ATCDIVERT = 324,
        DIVERTSTRIPBRA = 325,
        DIVERTSTRIPBULL = 326,
        ORBITSPACING = 327,
        PRESENTHEADING = 328,
        FLYHEADING = 329,
        GIVEWAY = 330,
        DEPARTHEADING = 331,
        FLYRUNWAYHEADING = 332,
        STOPTURN = 333,
        RESUMEOWNNAV = 334,
        LANDINGCHECK = 335,
        RUNWAYTROUBLE = 336,
        FUELCRITICAL = 337,
        ACCIDENT = 338,
        LastComm = 339
    };
    public enum RadioEval
    {
        Null= 0,
        AAAType= 1,
        Landing= 2,
        Airspeed= 3,
        AirTargtHit= 4,
        Alphabet= 5,
        AllClear= 6,
        AltCommand= 7,
        Angels= 8,
        ATCHello= 9,
        AttackFormation= 10,
        AttackRoles= 11,
        Bearing= 12,
        BoomCommands= 13,
        Break= 14,
        Callsign= 15,
        Clock= 16,
        Column= 17,
        Compass= 18,
        Compass2= 19,
        Crash= 20,
        Damage= 21,
        EscortBye= 22,
        Execute= 23,
        FACSituation= 24,
        Fire= 25,
        FlightSize= 26,
        FlightPosition= 27,
        FlightStatus = 28,
        FormResponseA= 29,
        FormResponseB= 30,
        RangeLast= 31,
        GoFormation= 32,
        GroundVehicleHit= 33,
        GroupAction= 34,
        Inbound= 35,
        LetsRock= 36,
        LetsSweep= 37,
        Looking = 38,
        Mark = 39,
        Minute= 40,
        NOBDA = 41,
        Numbers= 42,
        Picture= 43,
        Range = 44,
        RelAlt= 45,
        Roger = 46,
        Runways= 47,
        BanditClock= 48,
        ScanCommand = 49,
        SEADBye = 50,
        SelectWeapon= 51,
        Sort= 52,
        BanditAlt= 53,
        SpikeType= 54,
        StrikeBye= 55,
        StructureHit= 56,
        System= 57,
        SAM= 58,
        TrackingUpdate= 59,
        Unable= 60,
        Vamoose= 61,
        Vehicle= 62,
        BearingLast= 63,
        WeaponMiss = 64,
        FlightNumber= 65,
        FriendlyKilled = 66,
        Target= 67,
        Thousands= 68,
        Split= 69,
        WingActs= 70,
        CallNum = 71,
        CallToDigTwo= 72,
        CallNum2 = 73,
        CallFromDigTwo = 74,
        ShortCall= 75,
        Plus= 76,
        Aircraft= 77,
        Winchester= 78,
        AMRAAM= 79,
        PlayerKill= 80,
        Airbases = 81,
        LastWords= 82,
        ShootWing = 83,
        GroundFriends= 84,
        WingCrash = 85,
        CallSign2 = 86,
        Approach = 87,
        LostPadlock= 88,
        OutsideAirspace= 89,
        GreetingFirst = 90,
        ContinueInbound1= 91,
        ATCMissionResults= 92,
        ContinueInbound2 = 93,
        ClimbAndHold= 94,
        MaintainAirspeed= 95,
        ReduceAirspeed = 96,
        IncreaseAirspeed= 97,
        TurnRightOrLeftHeading= 98,
        VectorType= 99,
        TurnToFinal = 100,
        ApproachSpeed= 101,
        Scold1= 102,
        LandSequence= 103,
        GoAround= 104,
        ContactApproach= 105,
        HoldShort= 106,
        TowerScold1= 107,
        EmergencyClearance= 108,
        EmergencyEquipment= 109,
        ATCCancelMission= 110,
        DepartureSequence= 111,
        TowerScold2= 112,
        ExpediteDeparture= 113,
        TowerScold3= 114,
        OnMyTail= 115,
        GeneralID= 116,
        Channel= 117,
        StripType= 118,
        RunwayTrouble= 119,
        FuelCritical= 120,
        LastEval = 121
    };

    //radio channel filters
    public enum RadioChannels
    {
        Off = 0,
        Flight,
        Package,
        FromPackage,
        Proximity,
        Team,
        All,
        Tower
    };
    public enum RadioPriorities
    {
        LifeThreatening = 0,
        GoDefensive,
        GoOffensive,
        InterFlightInfo,
        SectionInfo,
        Default
    };

    public enum ConversationPriority
    {
        Attack=0,
        Normal,
        Medium,
    };
    public enum Warp
    {
	    None=0,
	    Package,
	    Flight,
	    Plane
    };

}
