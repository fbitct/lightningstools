namespace F4Utils.Campaign.F4Structs
{
    public struct OrientationData
    {
        public VU_QUAT quat_;	// quaternion indicating current facing
        public VU_VECT dquat_;	// unit vector expressing quaternion delta
        public float theta_;	// scalar indicating rate of above delta
        /*
        public float yaw_, pitch_, roll_;
        public float dyaw_, dpitch_, droll_;
         */
    }
}