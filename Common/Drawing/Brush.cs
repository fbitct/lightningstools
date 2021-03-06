﻿using System;

namespace Common.Drawing
{
    public abstract class Brush : MarshalByRefObject, IDisposable
    {
        protected Brush() { }
        protected Brush (System.Drawing.Brush brush)
        {
            WrappedBrush = brush;
        }
        protected System.Drawing.Brush WrappedBrush { get; set; }
        public abstract object Clone();

        ///// <summary>Converts the specified <see cref="T:System.Drawing.Brush" /> to a <see cref="T:Common.Drawing.Brush" />.</summary>
        ///// <returns>The <see cref="T:Common.Drawing.Brush" /> that results from the conversion.</returns>
        ///// <param name="brush">The <see cref="T:System.Drawing.Brush" /> to be converted.</param>
        ///// <filterpriority>3</filterpriority>
        //public static implicit operator Brush(System.Drawing.Brush brush)
        //{
        //    return new Brush(brush);
        //}

        /// <summary>Converts the specified <see cref="T:Common.Drawing.Brush" /> to a <see cref="T:System.Drawing.Brush" />.</summary>
        /// <returns>The <see cref="T:System.Drawing.Brush" /> that results from the conversion.</returns>
        /// <param name="brush">The <see cref="T:Common.Drawing.Brush" /> to be converted.</param>
        /// <filterpriority>3</filterpriority>
        public static implicit operator System.Drawing.Brush(Brush brush)
        {
            return brush.WrappedBrush;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WrappedBrush.Dispose();
            }
        }


        ~Brush()
        {
            this.Dispose(false);
        }


    }
}
