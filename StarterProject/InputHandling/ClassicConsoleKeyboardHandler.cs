﻿namespace StarterProject.InputHandling
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using SadConsole.Consoles;
    using SadConsole.Input;
    using System;

    class ClassicConsoleKeyboardHandler
    {
        // This holds the row that the virtual cursor is starting from when someone is typing.
        public int VirtualCursorLastY;

        // this is a callback for the owner of this keyboard handler. It is called when the user presses ENTER.
        public Action<string> EnterPressedAction = (s) => { int i = s.Length; };

        public bool HandleKeyboard(IConsole console, KeyboardInfo info)
        {
            // Check each key pressed.
            foreach (var key in info.KeysPressed)
            {
                // If the character associated with the key pressed is a printable character, print it
                if (key.Character != '\0')
                    console.VirtualCursor.Print(key.Character.ToString());

                // Special character - BACKSPACE
                else if (key.XnaKey == Keys.Back)
                {
                    // Get the prompt that the console has.
                    string prompt = ((CustomConsoles.DOSConsole)console).Prompt;

                    // If the console has scrolled since the user started typing, adjust the starting row of the virtual cursor by that much.
                    if (console.CellData.TimesShiftedUp != 0)
                    {
                        VirtualCursorLastY -= console.CellData.TimesShiftedUp;
                        console.CellData.TimesShiftedUp = 0;
                    }

                    // Do not let them backspace into the prompt
                    if (console.VirtualCursor.Position.Y != VirtualCursorLastY || console.VirtualCursor.Position.X > prompt.Length)
                        console.VirtualCursor.LeftWrap(1).Print(" ").LeftWrap(1);
                }

                // Special character - ENTER
                else if (key.XnaKey == Keys.Enter)
                {
                    // If the console has scrolled since the user started typing, adjust the starting row of the virtual cursor by that much.
                    if (console.CellData.TimesShiftedUp != 0)
                    {
                        VirtualCursorLastY -= console.CellData.TimesShiftedUp;
                        console.CellData.TimesShiftedUp = 0;
                    }

                    // Get the prompt to exclude it in determining the total length of the string the user has typed.
                    string prompt = ((CustomConsoles.DOSConsole)console).Prompt;
                    int startingIndex = console.CellData.GetIndexFromPoint(new Point(prompt.Length, VirtualCursorLastY));
                    string data = console.CellData.GetString(startingIndex, console.CellData.GetIndexFromPoint(console.VirtualCursor.Position) - startingIndex);

                    // Move the cursor to the next line before we send the string data to the processor
                    console.VirtualCursor.CarriageReturn().LineFeed();

                    // Send the string data
                    EnterPressedAction(data);

                    // After they have processed the string, we will create a new line and display the prompt.
                    console.VirtualCursor.CarriageReturn().LineFeed();
                    console.VirtualCursor.Print(((CustomConsoles.DOSConsole)console).Prompt);
                    VirtualCursorLastY = console.VirtualCursor.Position.Y;

                    // Preparing the next lines could have scrolled the console, reset the counter
                    console.CellData.TimesShiftedUp = 0;
                }
            }

            return true;
        }
    }
}
