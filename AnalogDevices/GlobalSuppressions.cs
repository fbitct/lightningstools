// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project. 
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc. 
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File". 
// You do not need to add suppressions to this file manually. 

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.ChannelMonitorSource.#DacChannel")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Scope = "type",
        Target = "AnalogDevices.ChannelAddress")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac", Scope = "type",
        Target = "AnalogDevices.DacChannelDataSource")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac", Scope = "type",
        Target = "AnalogDevices.DacPrecision")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac", Scope = "type",
        Target = "AnalogDevices.DenseDacEvalBoard")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Scope = "type"
        , Target = "AnalogDevices.DenseDacEvalBoard")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#DacPrecision")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#GetDacChannelDataSource(AnalogDevices.ChannelAddress)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#GetDacChannelDataValueA(AnalogDevices.ChannelAddress)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#GetDacChannelGain(AnalogDevices.ChannelAddress)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#GetDacChannelOffset(AnalogDevices.ChannelAddress)")
]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#GetDacChannelDataValueB(AnalogDevices.ChannelAddress)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target =
            "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataSource(AnalogDevices.ChannelAddress,AnalogDevices.DacChannelDataSource)"
        )]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target =
            "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataSource(AnalogDevices.ChannelGroup,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource,AnalogDevices.DacChannelDataSource)"
        )]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#ResumeAllDacOutputs()")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target =
            "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataSourceAllChannels(AnalogDevices.DacChannelDataSource)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataValueA(AnalogDevices.ChannelAddress,System.UInt16)")
]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataValueB(AnalogDevices.ChannelAddress,System.UInt16)")
]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelGain(AnalogDevices.ChannelAddress,System.UInt16)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelOffset(AnalogDevices.ChannelAddress,System.UInt16)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#SuspendAllDacOutputs()")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#UpdateAllDacOutputs()")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dac",
        Scope = "member", Target = "AnalogDevices.DeviceType.#DacEvalBoard")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval",
        Scope = "member", Target = "AnalogDevices.DeviceType.#DacEvalBoard")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ihx", Scope = "type",
        Target = "AnalogDevices.IhxFile")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ihx",
        Scope = "member", Target = "AnalogDevices.IhxFile.#ihxData")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Val",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataValueA(AnalogDevices.ChannelAddress,System.UInt16)")
]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Val",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataValueB(AnalogDevices.ChannelAddress,System.UInt16)")
]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Val",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelGain(AnalogDevices.ChannelAddress,System.UInt16)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Val",
        Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetDacChannelOffset(AnalogDevices.ChannelAddress,System.UInt16)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ihx",
        Scope = "member", Target = "AnalogDevices.DenseDacEvalBoard.#UploadFirmware(AnalogDevices.IhxFile)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ihx",
        Scope = "member", Target = "AnalogDevices.IhxFile.#IhxData")]
[assembly:
    SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args", Scope = "member",
        Target = "AnalogDevices.Test.#Main(System.String[])")]
[assembly:
    SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member",
        Target =
            "AnalogDevices.DenseDacEvalBoard.#SetDacChannelDataSource(AnalogDevices.ChannelAddress,AnalogDevices.DacChannelDataSource)"
        )]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetLDacPinLow()")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "AnalogDevices.DenseDacEvalBoard.#SetLDacPinHigh()")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type",
        Target = "AnalogDevices.Test")]
[assembly: SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member",
        Target = "AnalogDevices.IhxFile.#IhxData")]