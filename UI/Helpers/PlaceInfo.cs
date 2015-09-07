namespace UI.Helpers
{
    public struct PlaceInfo
    {
        public AttachPosition AttachTo { get; set; }
        public bool SkipParentBorder { get; set; }

        public bool OutsideHorizontal { get; set; }
        public bool OutsideVertical { get; set; }

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public bool AttachLeft
        {
            get
            {
                return (AttachTo == AttachPosition.Left) || (AttachTo == AttachPosition.TopLeft) || (AttachTo == AttachPosition.BottomLeft);
            }
        }

        public bool AttachRight
        {
            get
            {
                return (AttachTo == AttachPosition.Right) || (AttachTo == AttachPosition.TopRight) || (AttachTo == AttachPosition.BottomRight);
            }
        }

        public bool AttachTop
        {
            get
            {
                return (AttachTo == AttachPosition.Top) || (AttachTo == AttachPosition.TopLeft) || (AttachTo == AttachPosition.TopRight);
            }
        }

        public bool AttachBottom
        {
            get
            {
                return (AttachTo == AttachPosition.Bottom) || (AttachTo == AttachPosition.BottomLeft) || (AttachTo == AttachPosition.BottomRight);
            }
        }
    }
}
