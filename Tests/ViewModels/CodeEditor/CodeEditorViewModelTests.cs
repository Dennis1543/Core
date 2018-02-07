﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Jamiras.Services;
using Jamiras.ViewModels;
using Jamiras.ViewModels.CodeEditor;
using Moq;
using NUnit.Framework;

namespace Jamiras.Core.Tests.ViewModels.CodeEditor
{
    [TestFixture]
    class CodeEditorViewModelTests
    {
        [SetUp]
        public void Setup()
        {
            //            0         1         2         3         4
            //            01234567890123456789012345678901234567890123456
            var content = "bool test_function(int param1, string param2)\n" + // 1
                          "{\n" +                                             // 2
                          "    // This is an unnecessary comment\n" +         // 3
                          "    if (param1 == 1)\n" +                          // 4
                          "        param2 = 'Hello, world.';\n" +             // 5
                          "    else\n" +                                      // 6
                          "        param2 = NULL;\n" +                        // 7
                          "\n" +                                              // 8
                          "    return true;\n" +                              // 9
                          "}\n";                                              // 10

            var mockTimerService = new Mock<ITimerService>();
            viewModel = new CodeEditorViewModel(mockTimerService.Object);
            viewModel.SetContent(content);
            viewModel.FormatLine += FormatLine;
        }

        CodeEditorViewModel viewModel;

        private void FormatLine(object sender, LineFormatEventArgs e)
        {
            var text = e.Text;
            int i = 0;
            while (i < text.Length)
            {
                if (Char.IsWhiteSpace(text[i]))
                {
                    i++;
                    continue;
                }

                if (Char.IsLetterOrDigit(text[i]))
                {
                    int start = i;
                    do
                    {
                        i++;
                    } while (i < text.Length && (Char.IsLetterOrDigit(text[i]) || text[i] == '_'));

                    e.SetColor(start + 1, i - start, i);
                }
                else
                {
                    e.SetColor(i + 1, 1, 99);
                    i++;
                }
            }
        }

        [Test]
        public void TestInitialization()
        {
            Assert.That(viewModel.AreLineNumbersVisible, Is.True);
            Assert.That(viewModel.VisibleLines, Is.EqualTo(20));
            Assert.That(viewModel.CursorLine, Is.EqualTo(1));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(1));
            Assert.That(viewModel.LineCount, Is.EqualTo(10));
            Assert.That(viewModel.Lines.Count, Is.EqualTo(10));
            Assert.That(viewModel.Lines[0].LineLength, Is.EqualTo(45));
            Assert.That(viewModel.Lines[1].LineLength, Is.EqualTo(1));
            Assert.That(viewModel.Lines[2].LineLength, Is.EqualTo(37));
            Assert.That(viewModel.Lines[3].LineLength, Is.EqualTo(20));
            Assert.That(viewModel.Lines[4].LineLength, Is.EqualTo(33));
            Assert.That(viewModel.Lines[5].LineLength, Is.EqualTo(8));
            Assert.That(viewModel.Lines[6].LineLength, Is.EqualTo(22));
            Assert.That(viewModel.Lines[7].LineLength, Is.EqualTo(0));
            Assert.That(viewModel.Lines[8].LineLength, Is.EqualTo(16));
            Assert.That(viewModel.Lines[9].LineLength, Is.EqualTo(1));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(5, 1, 5, 1)]
        [TestCase(10, 1, 10, 1)]
        [TestCase(15, 1, 10, 1)]
        [TestCase(1, 20, 1, 20)]
        [TestCase(1, 40, 1, 40)]
        [TestCase(1, 60, 1, 46)]
        [TestCase(6, 20, 6, 9)]
        [TestCase(0, 0, 1, 1)]
        [TestCase(Int32.MaxValue, Int32.MaxValue, 10, 2)]
        public void TestMoveCursor(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 1, 1)]
        [TestCase(10, 1, 9, 1)]
        [TestCase(3, 10, 2, 2)]
        [TestCase(9, 16, 8, 1)]
        public void TestKeyUp(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Up, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        public void TestKeyUpRememberColumn()
        {
            viewModel.MoveCursorTo(7, 20, CodeEditorViewModel.MoveCursorFlags.None);

            // there aren't 20 columns in line 6, so cursor should jump back to column 9
            Assert.That(viewModel.HandleKey(Key.Up, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(9));

            // but when the user continues to line 5, it should jump back to column 20
            Assert.That(viewModel.HandleKey(Key.Up, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(5));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(20));
        }

        [Test]
        [TestCase(1, 1, 2, 1)]
        [TestCase(9, 1, 10, 1)]
        [TestCase(10, 1, 10, 1)]
        [TestCase(1, 10, 2, 2)]
        [TestCase(9, 16, 10, 2)]
        public void TestKeyDown(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Down, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        public void TestKeyDownRememberColumn()
        {
            viewModel.MoveCursorTo(5, 20, CodeEditorViewModel.MoveCursorFlags.None);

            // there aren't 20 columns in line 6, so cursor should jump back to column 9
            Assert.That(viewModel.HandleKey(Key.Down, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(9));

            // but when the user continues to line 7, it should jump back to column 20
            Assert.That(viewModel.HandleKey(Key.Down, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(7));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(20));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 2, 1, 1)]
        [TestCase(2, 1, 1, 46)]
        [TestCase(5, 10, 5, 9)]
        public void TestKeyLeft(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Left, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 4, 1, 1)]
        [TestCase(1, 14, 1, 6)]
        [TestCase(1, 24, 1, 20)]
        [TestCase(5, 9, 4, 21)]
        [TestCase(5, 1, 4, 21)]
        public void TestKeyCtrlLeft(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Left, ModifierKeys.Control), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 2)]
        [TestCase(1, 45, 1, 46)]
        [TestCase(1, 46, 2, 1)]
        [TestCase(5, 10, 5, 11)]
        public void TestKeyRight(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Right, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 6)]
        [TestCase(1, 5, 1, 6)]
        [TestCase(1, 6, 1, 19)]
        [TestCase(1, 23, 1, 24)]
        [TestCase(1, 24, 1, 30)]
        [TestCase(5, 1, 5, 9)]
        [TestCase(4, 21, 5, 9)]
        public void TestKeyCtrlRight(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Right, ModifierKeys.Control), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 4, 1)]
        [TestCase(1, 30, 4, 21)]
        [TestCase(7, 10, 10, 2)]
        [TestCase(9, 10, 10, 2)]
        [TestCase(10, 1, 10, 1)]
        public void TestKeyPageDown(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.VisibleLines = 4;
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.PageDown, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 1, 1)]
        [TestCase(4, 10, 1, 10)]
        [TestCase(5, 10, 2, 2)]
        [TestCase(10, 1, 7, 1)]
        public void TestKeyPageUp(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.VisibleLines = 4;
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.PageUp, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 9, 1, 1)]
        [TestCase(1, 42, 1, 1)]
        [TestCase(1, 46, 1, 1)]
        [TestCase(8, 1, 8, 1)]
        [TestCase(10, 1, 10, 1)]
        public void TestKeyHome(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Home, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 9, 1, 1)]
        [TestCase(1, 46, 1, 1)]
        [TestCase(8, 1, 1, 1)]
        [TestCase(10, 1, 1, 1)]
        public void TestKeyCtrlHome(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Home, ModifierKeys.Control), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 1, 46)]
        [TestCase(1, 9, 1, 46)]
        [TestCase(1, 42, 1, 46)]
        [TestCase(1, 46, 1, 46)]
        [TestCase(8, 1, 8, 1)]
        [TestCase(10, 1, 10, 2)]
        public void TestKeyEnd(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.End, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        [TestCase(1, 1, 10, 2)]
        [TestCase(1, 9, 10, 2)]
        [TestCase(1, 46, 10, 2)]
        [TestCase(8, 1, 10, 2)]
        [TestCase(10, 1, 10, 2)]
        public void TestKeyCtrlEnd(int line, int column, int expectedLine, int expectedColumn)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.End, ModifierKeys.Control), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(expectedLine));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
        }

        [Test]
        public void TestKeyBackspaceMidLine()
        {
            viewModel.MoveCursorTo(6, 7, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Back, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(6));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    ese"));
            Assert.That(viewModel.LineCount, Is.EqualTo(10));
        }

        [Test]
        public void TestKeyBackspaceBeginLine()
        {
            viewModel.MoveCursorTo(7, 1, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Back, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(9));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    else        param2 = NULL;"));
            Assert.That(viewModel.LineCount, Is.EqualTo(9));
        }

        [Test]
        public void TestKeyBackspaceBeginDocument()
        {
            viewModel.MoveCursorTo(1, 1, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Back, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(1));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(1));
            Assert.That(viewModel.Lines[5].PendingText, Is.Null);
            Assert.That(viewModel.LineCount, Is.EqualTo(10));
        }

        [Test]
        public void TestKeyDeleteMidLine()
        {
            viewModel.MoveCursorTo(6, 7, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Delete, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(7));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    ele"));
            Assert.That(viewModel.LineCount, Is.EqualTo(10));
        }

        [Test]
        public void TestKeyDeleteEndLine()
        {
            viewModel.MoveCursorTo(6, 9, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Delete, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(9));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    else        param2 = NULL;"));
            Assert.That(viewModel.LineCount, Is.EqualTo(9));
        }

        [Test]
        public void TestKeyDeleteEndDocument()
        {
            viewModel.MoveCursorTo(10, 2, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Delete, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(10));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(2));
            Assert.That(viewModel.Lines[5].PendingText, Is.Null);
            Assert.That(viewModel.LineCount, Is.EqualTo(10));
        }

        [Test]
        public void TestKeyEnterMidLine()
        {
            viewModel.MoveCursorTo(6, 7, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Enter, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(7));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(1));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    el"));
            Assert.That(viewModel.Lines[6].PendingText, Is.EqualTo("se"));
            Assert.That(viewModel.Lines[7].Text, Is.EqualTo("        param2 = NULL;"));
            Assert.That(viewModel.Lines[5].Line, Is.EqualTo(6));
            Assert.That(viewModel.Lines[6].Line, Is.EqualTo(7));
            Assert.That(viewModel.Lines[7].Line, Is.EqualTo(8));
            Assert.That(viewModel.LineCount, Is.EqualTo(11));
        }

        [Test]
        public void TestKeyEnterBeginLine()
        {
            viewModel.MoveCursorTo(6, 1, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Enter, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(7));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(1));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo(""));
            Assert.That(viewModel.Lines[6].PendingText, Is.EqualTo("    else"));
            Assert.That(viewModel.Lines[7].Text, Is.EqualTo("        param2 = NULL;"));
            Assert.That(viewModel.Lines[5].Line, Is.EqualTo(6));
            Assert.That(viewModel.Lines[6].Line, Is.EqualTo(7));
            Assert.That(viewModel.Lines[7].Line, Is.EqualTo(8));
            Assert.That(viewModel.LineCount, Is.EqualTo(11));
        }

        [Test]
        public void TestKeyEnterEndLine()
        {
            viewModel.MoveCursorTo(6, 9, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Enter, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(7));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(1));
            Assert.That(viewModel.Lines[5].Text, Is.EqualTo("    else"));
            Assert.That(viewModel.Lines[5].PendingText, Is.Null);
            Assert.That(viewModel.Lines[6].PendingText, Is.EqualTo(""));
            Assert.That(viewModel.Lines[7].Text, Is.EqualTo("        param2 = NULL;"));
            Assert.That(viewModel.Lines[5].Line, Is.EqualTo(6));
            Assert.That(viewModel.Lines[6].Line, Is.EqualTo(7));
            Assert.That(viewModel.Lines[7].Line, Is.EqualTo(8));
            Assert.That(viewModel.LineCount, Is.EqualTo(11));
        }

        [Test]
        [TestCase(1, 5, "        else")]
        [TestCase(3, 5, "      else")]
        [TestCase(5, 9, "        else")]
        [TestCase(6, 9, "    e   lse")]
        [TestCase(8, 9, "    els e")]
        [TestCase(9, 13, "    else    ")]
        public void TestKeyTab(int column, int expectedColumn, string expectedText)
        {
            viewModel.MoveCursorTo(6, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Tab, ModifierKeys.None), Is.True);
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo(expectedText));
        }

        [Test]
        public void TestHandleCharacter()
        {
            viewModel.MoveCursorTo(6, 7, CodeEditorViewModel.MoveCursorFlags.None);

            // can't call HandleKey(Key.I) because the key=>char mapping relies on the keyboard state
            viewModel.HandleCharacter('i');
            Assert.That(viewModel.CursorLine, Is.EqualTo(6));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(8));
            Assert.That(viewModel.Lines[5].PendingText, Is.EqualTo("    elise"));
        }

        [Test]
        [TestCase(1, 1, 5, "bool")]
        [TestCase(1, 3, 5, "bool")]
        [TestCase(1, 4, 5, "bool")]
        [TestCase(1, 5, 6, " ")]
        [TestCase(1, 6, 19, "test_function")]
        public void TestHightlightWordAt(int line, int column, int expectedColumn, string expectedText)
        {
            viewModel.HighlightWordAt(line, column);
            Assert.That(viewModel.CursorLine, Is.EqualTo(line));
            Assert.That(viewModel.CursorColumn, Is.EqualTo(expectedColumn));
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(1, 11, "f")]
        [TestCase(2, 2, "\r\n")]
        public void TestHighlightKeyRight(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Right, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(1, 11, "_")]
        [TestCase(3, 1, "\r\n")]
        public void TestHighlightKeyLeft(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Left, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(4, 12, " is an unnecessary comment\r\n    if (par")]
        [TestCase(9, 7, "\r\n    re")]
        public void TestHighlightKeyUp(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Up, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(4, 12, "am1 == 1)\r\n        par")]
        [TestCase(9, 7, "turn true;\r\n}")]
        public void TestHighlightKeyDown(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Down, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(4, 12, "    if (par")]
        [TestCase(9, 7, "    re")]
        [TestCase(6, 1, "")]
        public void TestHighlightKeyHome(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.Home, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        [TestCase(4, 12, "am1 == 1)")]
        [TestCase(9, 7, "turn true;")]
        [TestCase(6, 9, "")]
        public void TestHighlightKeyEnd(int line, int column, string expectedText)
        {
            viewModel.MoveCursorTo(line, column, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.End, ModifierKeys.Shift), Is.True);
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(expectedText));
        }

        [Test]
        public void TestKeyCtrlA()
        {
            viewModel.MoveCursorTo(5, 20, CodeEditorViewModel.MoveCursorFlags.None);
            Assert.That(viewModel.HandleKey(Key.A, ModifierKeys.Control), Is.True);
            Assert.That(viewModel.GetSelectedText().Length, Is.EqualTo(201));

            // trim Content() since SelectedText doesn't contain the trailing newline
            Assert.That(viewModel.GetSelectedText(), Is.EqualTo(viewModel.GetContent().TrimEnd()));
        }
    }
}
