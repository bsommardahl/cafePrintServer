using System;
using System.Drawing;

namespace CafePrintServer
{
    public class LinePrinter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly Brush _brush;
        readonly Font _font;
        readonly StringFormat _format;
        readonly Graphics _graphics;
        readonly int _left;
        readonly int _lineHeight;
        public int Top;

        public LinePrinter(Graphics graphics, Font font, Brush brush, int left, int top, int lineHeight,
                           StringFormat format)
        {
            _graphics = graphics;
            _font = font;
            _brush = brush;
            Top = top;
            _lineHeight = lineHeight;
            _format = format;
            _left = left;
        }

        public void Print(string text, params object[] args)
        {
            try
            {
                if (args.Length > 0)
                    text = string.Format(text, args);

                _graphics.DrawString(text, _font, _brush, _left, Top, _format);
                Top += _lineHeight;
            }
            catch (Exception ex)
            {
                Log.Error("Could not add line to the PrintDocument.", ex);
                throw;
            }
        }
    }
}