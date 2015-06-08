﻿namespace F4Utils.SimSupport
{
    public enum F4SimOutputs
    {
        MAP__GROUND_POSITION__FEET_NORTH_OF_MAP_ORIGIN,
        MAP__GROUND_POSITION__FEET_EAST_OF_MAP_ORIGIN,
        MAP__GROUND_SPEED_VECTOR__NORTH_COMPONENT_FPS,
        MAP__GROUND_SPEED_VECTOR__EAST_COMPONENT_FPS,
        MAP__GROUND_SPEED_KNOTS,
        TRUE_ALTITUDE__MSL,
        ALTIMETER__INDICATED_ALTITUDE__MSL,
        ALTIMETER__BAROMETRIC_PRESSURE_INCHES_HG,
        VVI__VERTICAL_VELOCITY_FPM,
        FLIGHT_DYNAMICS__SIDESLIP_ANGLE_DEGREES,
        FLIGHT_DYNAMICS__CLIMBDIVE_ANGLE_DEGREES,
        FLIGHT_DYNAMICS__OWNSHIP_NORMAL_GS,
        AIRSPEED_MACH_INDICATOR__MACH_NUMBER,
        AIRSPEED_MACH_INDICATOR__INDICATED_AIRSPEED_KNOTS,
        AIRSPEED_MACH_INDICATOR__TRUE_AIRSPEED_KNOTS,
        HUD__WIND_DELTA_TO_FLIGHT_PATH_MARKER_DEGREES,
        NOZ_POS1__NOZZLE_PERCENT_OPEN,
        NOZ_POS2__NOZZLE_PERCENT_OPEN,
        HYD_PRESSURE_A__PSI,
        HYD_PRESSURE_B__PSI,
        FUEL_QTY__INTERNAL_FUEL_POUNDS,
        FUEL_QTY__EXTERNAL_FUEL_POUNDS,
        FUEL_FLOW__FUEL_FLOW_POUNDS_PER_HOUR,
        RPM1__RPM_PERCENT,
        RPM2__RPM_PERCENT,
        FTIT1__FTIT_TEMP_DEG_CELCIUS,
        FTIT2__FTIT_TEMP_DEG_CELCIUS,
        SPEED_BRAKE__POSITION,
        EPU_FUEL__EPU_FUEL_PERCENT,
        OIL_PRESS1__OIL_PRESS_PERCENT,
        OIL_PRESS2__OIL_PRESS_PERCENT,
        CABIN_PRESS__CABIN_PRESS_FEET_MSL,
        COMPASS__MAGNETIC_HEADING_DEGREES,
        GEAR_PANEL__GEAR_POSITION,
        GEAR_PANEL__NOSE_GEAR_DOWN_LIGHT,
        GEAR_PANEL__LEFT_GEAR_DOWN_LIGHT,
        GEAR_PANEL__RIGHT_GEAR_DOWN_LIGHT,
        GEAR_PANEL__NOSE_GEAR_POSITION,
        GEAR_PANEL__LEFT_GEAR_POSITION,
        GEAR_PANEL__RIGHT_GEAR_POSITION,
        GEAR_PANEL__GEAR_HANDLE_LIGHT,
        GEAR_PANEL__PARKING_BRAKE_ENGAGED_FLAG,
        ADI__PITCH_DEGREES,
        ADI__ROLL_DEGREES,
        ADI__ILS_SHOW_COMMAND_BARS,
        ADI__ILS_HORIZONTAL_BAR_POSITION,
        ADI__ILS_VERTICAL_BAR_POSITION,
        ADI__RATE_OF_TURN_INDICATOR_POSITION,
        ADI__INCLINOMETER_POSITION,
        ADI__OFF_FLAG,
        ADI__AUX_FLAG,
        ADI__GS_FLAG,
        ADI__LOC_FLAG,
        STBY_ADI__PITCH_DEGREES,
        STBY_ADI__ROLL_DEGREES,
        STBY_ADI__OFF_FLAG,
        VVI__OFF_FLAG,
        AOA_INDICATOR__AOA_DEGREES,
        AOA_INDICATOR__OFF_FLAG,
        HSI__COURSE_DEVIATION_INVALID_FLAG,
        HSI__DISTANCE_INVALID_FLAG,
        HSI__DESIRED_COURSE_DEGREES,
        HSI__COURSE_DEVIATION_DEGREES,
        HSI__COURSE_DEVIATION_LIMIT_DEGREES,
        HSI__DISTANCE_TO_BEACON_NAUTICAL_MILES,
        HSI__BEARING_TO_BEACON_DEGREES,
        HSI__CURRENT_HEADING_DEGREES,
        HSI__DESIRED_HEADING_DEGREES,
        HSI__LOCALIZER_COURSE_DEGREES,
        HSI__TO_FLAG,
        HSI__FROM_FLAG,
        HSI__OFF_FLAG,
        HSI__HSI_MODE,
        TRIM__PITCH_TRIM,
        TRIM__ROLL_TRIM,
        TRIM__YAW_TRIM,
        DED__LINES,
        DED__INVERT_LINES,
        PFL__LINES,
        PFL__INVERT_LINES,
        UFC__TACAN_CHANNEL,
        AUX_COMM__TACAN_CHANNEL,
        AUX_COMM__TACAN_BAND_IS_X,
        AUX_COMM__TACAN_MODE_IS_AA,
        UFC__TACAN_BAND_IS_X,
        UFC__TACAN_MODE_IS_AA,
        FUEL_QTY__FOREWARD_QTY_LBS,
        FUEL_QTY__AFT_QTY_LBS,
        FUEL_QTY__TOTAL_FUEL_LBS,
        LMFD__OSB_LABEL_LINES1,
        LMFD__OSB_LABEL_LINES2,
        LMFD__OSB_INVERTED_FLAGS,
        RMFD__OSB_LABEL_LINES1,
        RMFD__OSB_LABEL_LINES2,
        RMFD__OSB_INVERTED_FLAGS,
        MASTER_CAUTION_LIGHT,
        LEFT_EYEBROW_LIGHTS__TFFAIL,
        LEFT_EYEBROW_LIGHTS__ALTLOW,
        LEFT_EYEBROW_LIGHTS__OBSWRN,
        RIGHT_EYEBROW_LIGHTS__ENGFIRE,
        RIGHT_EYEBROW_LIGHTS__ENGINE,
        RIGHT_EYEBROW_LIGHTS__HYDOIL,
        RIGHT_EYEBROW_LIGHTS__DUALFC,
        RIGHT_EYEBROW_LIGHTS__FLCS,
        RIGHT_EYEBROW_LIGHTS__CANOPY,
        RIGHT_EYEBROW_LIGHTS__TO_LDG_CONFIG,
        RIGHT_EYEBROW_LIGHTS__OXY_LOW,
        CAUTION_PANEL__FLCS_FAULT,
        CAUTION_PANEL__LE_FLAPS,
        CAUTION_PANEL__ELEC_SYS,
        CAUTION_PANEL__ENGINE_FAULT,
        CAUTION_PANEL__SEC,
        CAUTION_PANEL__FWD_FUEL_LOW,
        CAUTION_PANEL__AFT_FUEL_LOW,
        CAUTION_PANEL__OVERHEAT,
        CAUTION_PANEL__BUC,
        CAUTION_PANEL__FUEL_OIL_HOT,
        CAUTION_PANEL__SEAT_NOT_ARMED,
        CAUTION_PANEL__AVIONICS_FAULT,
        CAUTION_PANEL__RADAR_ALT,
        CAUTION_PANEL__EQUIP_HOT,
        CAUTION_PANEL__ECM,
        CAUTION_PANEL__STORES_CONFIG,
        CAUTION_PANEL__ANTI_SKID,
        CAUTION_PANEL__HOOK,
        CAUTION_PANEL__NWS_FAIL,
        CAUTION_PANEL__CABIN_PRESS,
        CAUTION_PANEL__OXY_LOW,
        CAUTION_PANEL__PROBE_HEAT,
        CAUTION_PANEL__FUEL_LOW,
        CAUTION_PANEL__IFF,
        CAUTION_PANEL__C_ADC,
        AOA_INDEXER__AOA_TOO_HIGH,
        AOA_INDEXER__AOA_IDEAL,
        AOA_INDEXER__AOA_TOO_LOW,
        NWS_INDEXER__RDY,
        NWS_INDEXER__AR_NWS,
        NWS_INDEXER__DISC,
        TWP__HANDOFF,
        TWP__MISSILE_LAUNCH,
        TWP__PRIORITY_MODE_OPEN,
        TWP__UNKNOWN,
        TWP__NAVAL,
        TWP__TARGET_SEP,
        TWA__SEARCH,
        TWA__ACTIVITY_POWER,
        TWA__LOW_ALTITUDE,
        TWA__SYSTEM_POWER,
        ECM__POWER,
        ECM__FAIL,
        MISC__ADV_MODE_ACTIVE,
        MISC__ADV_MODE_STBY,
        MISC__AUTOPILOT_ENGAGED,
        CHAFF_FLARE__CHAFF_COUNT,
        CHAFF_FLARE__FLARE_COUNT,
        CMDS__GO,
        CMDS__NOGO,
        CMDS__AUTO_DEGR,
        CMDS__DISPENSE_RDY,
        CMDS__CHAFF_LO,
        CMDS__FLARE_LO,
        ELEC__FLCS_PMG,
        ELEC__MAIN_GEN,
        ELEC__STBY_GEN,
        ELEC__EPU_GEN,
        ELEC__EPU_PMG,
        ELEC__TO_FLCS,
        ELEC__FLCS_RLY,
        ELEC__BATT_FAIL,
        TEST__ABCD,
        EPU__HYDRAZN,
        EPU__AIR,
        EPU__RUN,
        AVTR__AVTR,
        JFS__RUN,
        MARKER_BEACON__OUTER_MARKER,
        MARKER_BEACON__MIDDLE_MARKER,
        AIRCRAFT__ONGROUND,
        AIRCRAFT__ELEC_POWER_OFF,
        AIRCRAFT__MAIN_POWER,
        AIRCRAFT__ENGINE_2_FIRE,
        SIM__SHOOT_CUE,
        SIM__LOCK_CUE,
        SIM__BMS_PLAYER_IS_FLYING,
        SIM__SHARED_MEM_VERSION,
        PILOT__HEADX_OFFSET,
        PILOT__HEADY_OFFSET,
        PILOT__HEADZ_OFFSET,
        PILOT__HEAD_PITCH_DEGREES,
        PILOT__HEAD_ROLL_DEGREES,
        PILOT__HEAD_YAW_DEGREES,
        FLIGHT_CONTROL__RUN,
        FLIGHT_CONTROL__FAIL,
        RWR__OBJECT_COUNT,
        RWR__SYMBOL_ID,
        RWR__BEARING_DEGREES,
        RWR__MISSILE_ACTIVITY_FLAG,
        RWR__MISSILE_LAUNCH_FLAG,
        RWR__SELECTED_FLAG,
        RWR__LETHALITY,
        RWR__NEWDETECTION_FLAG,
    }
}