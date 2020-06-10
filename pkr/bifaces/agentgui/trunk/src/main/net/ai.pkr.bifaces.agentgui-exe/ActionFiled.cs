/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace ai.pkr.bifaces.agentgui_exe
{
    public class ActionFiled: RichTextBox
    {
        bool _defaultColorKnown = false;
        Color _defaultColor = Color.White;

        void StoreDefaultColor()
        {
            if (!_defaultColorKnown)
            {
                _defaultColor = BackColor;
                _defaultColorKnown = true;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            BackColor = _defaultColor;
        }


        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            BackColor = Color.DarkGray;
        }
    }
}
