namespace Common.Drawing
{
    /// <summary>Each property of the <see cref="T:Common.Drawing.SystemPens" /> class is a <see cref="T:Common.Drawing.Pen" /> that is the color of a Windows display element and that has a width of 1 pixel.</summary>
    /// <filterpriority>1</filterpriority>
    public sealed class SystemPens
    {
        public static Pen ActiveBorder { get { return System.Drawing.SystemPens.ActiveBorder; } }
        public static Pen ActiveCaption { get { return System.Drawing.SystemPens.ActiveCaption; } }
        public static Pen ActiveCaptionText { get { return System.Drawing.SystemPens.ActiveCaptionText; } }
        public static Pen AppWorkspace { get { return System.Drawing.SystemPens.AppWorkspace; } }
        public static Pen ButtonFace { get { return System.Drawing.SystemPens.ButtonFace; } }
        public static Pen ButtonHighlight { get { return System.Drawing.SystemPens.ButtonHighlight; } }
        public static Pen ButtonShadow { get { return System.Drawing.SystemPens.ButtonShadow; } }
        public static Pen Control { get { return System.Drawing.SystemPens.Control; } }
        public static Pen ControlText { get { return System.Drawing.SystemPens.ControlText; } }
        public static Pen ControlDark { get { return System.Drawing.SystemPens.ControlDark; } }
        public static Pen ControlDarkDark { get { return System.Drawing.SystemPens.ControlDarkDark; } }
        public static Pen ControlLight { get { return System.Drawing.SystemPens.ControlLight; } }
        public static Pen ControlLightLight { get { return System.Drawing.SystemPens.ControlLightLight; } }
        public static Pen Desktop { get { return System.Drawing.SystemPens.Desktop; } }
        public static Pen GradientActiveCaption { get { return System.Drawing.SystemPens.GradientActiveCaption; } }
        public static Pen GradientInactiveCaption { get { return System.Drawing.SystemPens.GradientInactiveCaption; } }
        public static Pen GrayText { get { return System.Drawing.SystemPens.GrayText; } }
        public static Pen Highlight { get { return System.Drawing.SystemPens.Highlight; } }
        public static Pen HighlightText { get { return System.Drawing.SystemPens.HighlightText; } }
        public static Pen HotTrack { get { return System.Drawing.SystemPens.HotTrack; } }
        public static Pen InactiveBorder { get { return System.Drawing.SystemPens.InactiveBorder; } }
        public static Pen InactiveCaption { get { return System.Drawing.SystemPens.InactiveCaption; } }
        public static Pen InactiveCaptionText { get { return System.Drawing.SystemPens.InactiveCaptionText; } }
        public static Pen Info { get { return System.Drawing.SystemPens.Info; } }
        public static Pen InfoText { get { return System.Drawing.SystemPens.InfoText; } }
        public static Pen Menu { get { return System.Drawing.SystemPens.Menu; } }
        public static Pen MenuBar { get { return System.Drawing.SystemPens.MenuBar; } }
        public static Pen MenuHighlight { get { return System.Drawing.SystemPens.MenuHighlight; } }
        public static Pen MenuText { get { return System.Drawing.SystemPens.MenuText; } }
        public static Pen ScrollBar { get { return System.Drawing.SystemPens.ScrollBar; } }
        public static Pen Window { get { return System.Drawing.SystemPens.Window; } }
        public static Pen WindowFrame { get { return System.Drawing.SystemPens.WindowFrame; } }
        public static Pen WindowText { get { return System.Drawing.SystemPens.WindowText; } }

        private SystemPens(){}
    }
}
