namespace F4Utils.Campaign.F4Structs
{
    public enum Dirty_Class
    {
        DIRTY_FALCON_ENTITY = 0x00000001,
        DIRTY_CAMPAIGN_BASE = 0x00000002,
        DIRTY_OBJECTIVE = 0x00000004,
        DIRTY_UNIT = 0x00000008,
        DIRTY_PACKAGE = 0x00000010,
        DIRTY_SQUADRON = 0x00000020,
        DIRTY_FLIGHT = 0x00000040,
        DIRTY_GROUND_UNIT = 0x00000080,
        DIRTY_BATTALION = 0x00000100,
        DIRTY_TEAM = 0x00000200,
        DIRTY_SIM_BASE = 0x00000400,
    };
}