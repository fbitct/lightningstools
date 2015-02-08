namespace F4Utils.Campaign.F4Structs
{
    public enum Dirty_Flight
    {
        DIRTY_LAST_DIRECTION = 0x00000001,
        DIRTY_PACKAGE_ID = 0x00000002,
        DIRTY_MISSION = 0x00000004,
        DIRTY_PLANE_STATS = 0x00000008,
        DIRTY_PILOTS = 0x00000010,
        DIRTY_EVAL_FLAGS = 0x00000020,
        DIRTY_ASSIGNED_TARGET = 0x00000040,
        DIRTY_STORES = 0x00000080,
        DIRTY_DIVERT_INFO = 0x00000100,
    };
}