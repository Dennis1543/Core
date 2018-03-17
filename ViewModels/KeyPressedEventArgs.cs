﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace Jamiras.ViewModels
{
    /// <summary>
    /// Parameters associated to a key being pressed.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class KeyPressedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPressedEventArgs"/> class.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        /// <param name="modifiers">Modifier keys that were pressed when the key was pressed.</param>
        public KeyPressedEventArgs(Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Gets the key that was pressed.
        /// </summary>
        public Key Key { get; private set; }

        /// <summary>
        /// Gets the modifier keys that were pressed when the key was pressed.
        /// </summary>
        public ModifierKeys Modifiers { get; private set; }

        /// <summary>
        /// Gets or sets whether the key press has been handled.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>true</c> should prevent any other handlers from processing the event.
        /// </remarks>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the character associated with the key press.
        /// </summary>
        /// <returns>The character generated by the key press, or '\0' if no character is associated to the key press.</returns>
        /// <remarks>
        /// Queries the keyboard state directly instead of using <see cref="ModifierKeys"/>, so may be incorrect when debugging or unit testing.
        /// </remarks>
        public char GetChar()
        {
            // https://stackoverflow.com/questions/5825820/how-to-capture-the-character-on-different-locale-keyboards-in-wpf-c
            // map the Key enum to a virtual key
            int virtualKey = KeyInterop.VirtualKeyFromKey(Key);

            // map the virtual key to a scan code
            const uint MAPVK_VK_TO_VSC = 0;
            uint scanCode = MapVirtualKey((uint)virtualKey, MAPVK_VK_TO_VSC);

            // get the state of keys (shift/caps lock/etc)
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            // convert the scan code and key state to a character
            StringBuilder buffer = new StringBuilder(2);
            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, buffer, buffer.Capacity, 0);

            switch (result)
            {
                case -1: // accent or diacritic
                    return '\0';
                case 0: // key does not map to a character
                    return '\0';
                case 1: // single character
                    if (Char.IsControl(buffer[0]))
                        return '\0';
                    return buffer[0];
                default: // more than one character
                    return buffer[0];
            }
        }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode,byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff, uint wFlags);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
    }
}
