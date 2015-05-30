using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct IntellivibeData
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U1)]
        public byte AAMissileFired; // how many AA missiles fired.

        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.U1)]
        public byte AGMissileFired; // how many maveric/rockets fired

        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.U1)]
        public byte BombDropped; // how many bombs dropped

        [FieldOffset(3)]
        [MarshalAs(UnmanagedType.U1)]
        public byte FlareDropped; // how many flares dropped

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.U1)]
        public byte ChaffDropped; // how many chaff dropped

        [FieldOffset(5)]
        [MarshalAs(UnmanagedType.U1)]
        public byte BulletsFired; // how many bullets shot

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I4)]
        public int CollisionCounter; // Collisions

        [FieldOffset(12)]
        [MarshalAs (UnmanagedType.U1)]
        public bool IsFiringGun; // gun is firing

        [FieldOffset(13)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsEndFlight; // Ending the flight from 3d

        [FieldOffset(14)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsEjecting; // we've ejected

        [FieldOffset(15)]
        [MarshalAs(UnmanagedType.U1)]
        public bool In3D; // In 3D?

        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsPaused; // sim paused?

        [FieldOffset(17)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsFrozen; // sim frozen?

        [FieldOffset(18)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOverG; // are G limits being exceeded?

        [FieldOffset(19)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOnGround; // are we on the ground

        [FieldOffset(20)]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsExitGame; // Did we exit Falcon?

        [FieldOffset(24)]
        [MarshalAs(UnmanagedType.R4)]
        public float Gforce; // what gforce we are feeling

        [FieldOffset(28)]
        [MarshalAs(UnmanagedType.R4)]
        public float eyex; // where the eye is in relationship to the plane

        [FieldOffset(32)]
        [MarshalAs(UnmanagedType.R4)]
        public float eyey; // where the eye is in relationship to the plane

        [FieldOffset(36)]
        [MarshalAs(UnmanagedType.R4)]
        public float eyez; // where the eye is in relationship to the plane

        [FieldOffset(40)]
        [MarshalAs(UnmanagedType.I4)]
        public int lastdamage; // 1 to 8 depending on quadrant. 

        [FieldOffset(44)]
        [MarshalAs(UnmanagedType.R4)]
        public float damageforce; // how big the hit was.

        [FieldOffset(48)]
        [MarshalAs(UnmanagedType.I4)]
        public int whendamage;
    }
}
