using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;
using CursorPosition = (int line, int column);

#pragma warning disable IDE1006 // Naming Styles
namespace BetterWorkshopUploader
{
    public class OpTextArea : UIconfig, ICanBeTyped
    {
        protected bool _mouseDown;
        protected bool _keyboardOn;
        protected float _col;
        protected FLabelAlignment _alignment = FLabelAlignment.Left;
        protected int _linesOffset;
        protected float _cursorAlpha;
        protected float _lastCursorAlpha;
        protected int _cursorChar;
        protected CursorPosition _cursorPos;
        protected float _arrowX;
        protected float _lastArrowX;
        protected float _targetArrowX;

        protected FSprite _cursor;
        protected FSprite _arrowUp;
        protected FSprite _arrowDown;
        protected BumpBehaviour _bumpUp;
        protected BumpBehaviour _bumpDown;

        public DyeableRect rect;
        public List<FLabel> labels = [];

        protected List<string> _valueLines = [];
        protected List<int> _valueLineOffsets = [];

        public Color colorEdge = MenuColorEffect.rgbMediumGrey;
        public Color colorText = MenuColorEffect.rgbMediumGrey;
        public Color colorFill = MenuColorEffect.rgbBlack;

        protected float WrapWidth => size.x - 5f - 30f; // 5f = left padding, 30f = right padding

        public int LinesOffset
        {
            get => _linesOffset;
            set => _linesOffset = Mathf.Clamp(value, 0, _valueLines.Count - labels.Count);
        }

        public Action<char> OnKeyDown { get; set; }

        public OpTextArea(string defaultValue, Vector2 pos, float sizeX, int lines) : base(new Configurable<string>(""), pos, new Vector2(60f, 24f))
        {
            mute = true;
            _size = new Vector2(Mathf.Max(60f, sizeX), 30f + LabelTest.LineHeight(false) * Math.Max(0, lines - 1));
            _value = defaultValue;
            value = _value;
            _mouseDown = false;
            mouseOverStopsScrollwheel = true;

            rect = new DyeableRect(myContainer, Vector2.zero, size, true)
            {
                fillAlpha = 0.5f
            };
            
            for (int i = 0; i < lines; i++)
            {
                var label = new FLabel(LabelTest.GetFont(false), "")
                {
                    color = colorText,
                    alignment = FLabelAlignment.Left,
                    anchorX = 0f,
                    anchorY = 1f
                };
                myContainer.AddChild(label);
                labels.Add(label);
            }

            _cursor = new FSprite("pixel")
            {
                scaleX = 2f,
                scaleY = LabelTest.LineHeight(false),
                color = colorText,
                anchorX = 0f,
                anchorY = 1f
            };
            myContainer.AddChild(_cursor);

            _arrowUp = new FSprite("Menu_Symbol_Arrow", true)
            {
                scale = 1f,
                rotation = 0f,
                anchorX = 0.5f,
                anchorY = 0.5f,
                x = size.x - 15f,
                y = size.y - 10f,
                color = colorText
            };
            _arrowDown = new FSprite("Menu_Symbol_Arrow", true)
            {
                scale = 1f,
                rotation = 180f,
                anchorX = 0.5f,
                anchorY = 0.5f,
                x = size.x - 15f,
                y = 10f,
                color = colorText
            };
            myContainer.AddChild(_arrowUp);
            myContainer.AddChild(_arrowDown);
            _bumpUp = new BumpBehaviour(this);
            _bumpDown = new BumpBehaviour(this);

            OnKeyDown += KeyboardAccept;

            mute = false;
            this.Assign();
            Change();
        }

        public override void Change()
        {
            base.Change();

            // Update evaluated lines to display
            ReevaluateLines();
            UpdateCursorPos(true);

            // Update labels
            for (int i = 0; i < labels.Count; i++)
            {
                var label = labels[i];
                label.y = size.y - 5f - LabelTest.LineHeight(false) * i;
                label.alignment = Alignment;
                switch (Alignment)
                {
                    case FLabelAlignment.Left or FLabelAlignment.Custom:
                        label.x = 5f;
                        break;
                    case FLabelAlignment.Center:
                        label.x = 5f + WrapWidth / 2f;
                        break;
                    case FLabelAlignment.Right:
                        label.x = 5f + WrapWidth;
                        break;
                }
            }
        }

        public override void Update()
        {
            bool mouseOverArrow = false;
            bool arrowActivated = false;
            rect.Update();
            _lastArrowX = _arrowX;
            _lastCursorAlpha = _cursorAlpha;

            if (greyedOut)
            {
                _arrowX = _targetArrowX;
                _bumpUp.held = false;
                _bumpDown.held = false;
                _bumpUp.greyedOut = true;
                _bumpDown.greyedOut = true;
                _bumpUp.Focused = false;
                _bumpDown.Focused = false;
            }
            else
            {
                _arrowX = Custom.LerpAndTick(_arrowX, _targetArrowX, 2f, 0.2f);
                _bumpUp.greyedOut = greyedOut;
                _bumpDown.greyedOut = greyedOut;
                _bumpUp.Focused = false;
                _bumpDown.Focused = false;
                if (!MenuMouseMode && Focused)
                {
                    _bumpDown.Focused = true;
                    _bumpUp.Focused = true;
                }
                else if (MenuMouseMode && MousePos.x > size.x - 25f && MousePos.x < size.x - 5f && MousePos.y > 5f)
                {
                    if (MousePos.y < 15f)
                    {
                        _bumpDown.Focused = true;
                        mouseOverArrow = true;
                    }
                    else if (MousePos.y > size.y - 15f)
                    {
                        _bumpUp.Focused = true;
                        mouseOverArrow = true;
                    }
                }
            }

            _col = bumpBehav.col;
            base.Update();
            _bumpUp.Update();
            _bumpDown.Update();
            bumpBehav.col = _col;

            // Disable keyboard input when disabled or unable to accept keyboard input
            if (greyedOut || !MenuMouseMode)
            {
                _KeyboardOn = false;
                return;
            }

            // Update cursor alpha
            if (_KeyboardOn)
            {
                _cursorAlpha -= 0.05f / frameMulti;
                if (_cursorAlpha < -0.5f)
                {
                    _cursorAlpha = 2f;
                }
                ForceMenuMouseMode(true);
            }
            else
            {
                _cursorAlpha = 0;
            }

            // Mouse
            if (Input.GetMouseButtonDown(0))
            {
                held = true;
                if (MouseOver || _KeyboardOn)
                {
                    _mouseDown = true;
                    held = true;
                    if (mouseOverArrow)
                    {
                        if (_bumpUp.Focused)
                        {
                            _bumpUp.held = true;
                            arrowActivated = true;
                        }
                        else if (_bumpDown.Focused)
                        {
                            _bumpDown.held = true;
                            arrowActivated = true;
                        }
                    }
                }
            }
            else
            {
                if (_mouseDown)
                {
                    if (!held && MouseOver && !_KeyboardOn)
                    {
                        if (MousePos.x <= size.x - 30f)
                        {
                            PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                            _KeyboardOn = true;
                            _cursor.isVisible = true;
                            _cursorAlpha = 1f;
                            _cursor.SetPosition(LabelTest.GetWidth(labels[_cursorPos.line - _linesOffset].text.Substring(0, _cursorPos.column), false) + LabelTest.CharMean(false), size.y * 0.5f);
                        }
                    }
                    else if (held && _KeyboardOn && !MouseOver)
                    {
                        _KeyboardOn = false;
                        PlaySound(SoundID.MENU_Checkbox_Uncheck);
                    }
                }
                else
                {
                    _bumpUp.held = false;
                    _bumpDown.held = false;
                    held = false;
                }
                _mouseDown = false;
            }

            // Scroll wheel
            if (MouseOver && Menu.mouseScrollWheelMovement != 0)
            {
                Scroll(Menu.mouseScrollWheelMovement < 0 ? -1 : 1);
                if (Menu.mouseScrollWheelMovement > 0)
                {
                    _bumpDown.held = true;
                }
                else
                {
                    _bumpUp.held = false;
                }
            }

            // Flash
            if (_bumpUp.held || _bumpDown.held)
            {
                bumpBehav.flash = 1f;
            }

            // Keybinds
            if (_KeyboardOn)
            {
                int xDir = bumpBehav.JoystickPressAxis(false);
                int yDir = bumpBehav.JoystickPressAxis(true);
                if (xDir != 0)
                {
                    _cursorChar += xDir;
                    UpdateCursorPos(false);
                }
                else if (yDir != 0)
                {
                    Scroll(-yDir); // opposite of yDir so up direction (1) scrolls up (-1)
                }

                if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Pause))
                {
                    _KeyboardOn = false;
                    PlaySound(SoundID.MENU_Checkbox_Uncheck);
                }
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            rect.addSize = 4f * bumpBehav.AddSize * Vector2.one;

            // Disabled behavior
            if (greyedOut)
            {
                foreach (var label in labels)
                {
                    label.color = bumpBehav.GetColor(colorText);
                }
                _cursor.alpha = 0f;
                rect.colorEdge = bumpBehav.GetColor(colorEdge);
                rect.colorFill = bumpBehav.GetColor(colorFill);
                rect.GrafUpdate(timeStacker);
                return;
            }

            // Update cursor and color intensity
            if (_KeyboardOn)
            {
                _col = Mathf.Min(1f, bumpBehav.col + 0.1f);
            }
            else
            {
                _col = Mathf.Max(0f, bumpBehav.col - 0.033333335f);
            }
			_cursor.color = Color.Lerp(MenuColorEffect.rgbWhite, colorText, bumpBehav.Sin(30f));
            _cursor.alpha = 1f; //Mathf.Clamp01(Mathf.Lerp(_lastCursorAlpha, _cursorAlpha, timeStacker));
            float textFullWidth = LabelTest.GetWidth(_valueLines[_cursorPos.line], false);
            float textWidth = LabelTest.GetWidth(_valueLines[_cursorPos.line].Substring(0, _cursorPos.column), false);
            _cursor.y = size.y - 5f - LabelTest.LineHeight(false) * (_cursorPos.line - _linesOffset);
            _cursor.x = Alignment switch
            {
                FLabelAlignment.Center => 5f + WrapWidth / 2f - textFullWidth / 2f + textWidth,
                FLabelAlignment.Right => 5f + WrapWidth - textFullWidth + textWidth,
                FLabelAlignment.Left or FLabelAlignment.Custom => 5f + textWidth,
                _ => throw new NotImplementedException()
            };
            _cursor.x = Mathf.Clamp(_cursor.x, 5f, 5f + WrapWidth);

            bumpBehav.col = _col;
            rect.fillAlpha = Mathf.Lerp(0.5f, 0.8f, bumpBehav.col);
            rect.colorEdge = bumpBehav.GetColor(colorEdge);
            rect.colorFill = colorFill;
            rect.GrafUpdate(timeStacker);

            foreach (var label in labels)
            {
                label.color = Color.Lerp(colorText, MenuColorEffect.rgbWhite, Mathf.Clamp(bumpBehav.flash, 0f, 1f));
            }
        }

        public void KeyboardAccept(char input)
        {
            if (!_KeyboardOn) return;
            Plugin.Logger.LogDebug("CHAR: " + ((byte)input).ToString());

            if (input == '\b')
            {
                _cursorAlpha = 2.5f;
                bumpBehav.flash = 2.5f;
                if (_cursorChar > 0)
                {
                    value = value.Substring(0, _cursorChar - 1) + value.Substring(_cursorChar);
                    _cursorChar--;
                    PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                    return;
                }
            }
            else
            {
                if (input == '\r')
                {
                    input = '\n';
                }

                value = value.Substring(0, _cursorChar) + input + value.Substring(_cursorChar);
                _cursorChar++;
                _cursorAlpha = 2.5f;
                bumpBehav.flash = 2.5f;
            }
        }

        protected void ReevaluateLines()
        {
            if (string.IsNullOrEmpty(value))
            {
                _valueLines = [value ?? ""];
                _valueLineOffsets = [0];
                return;
            }

            List<string> output = [];
            List<int> offsets = [];
            string[] split = value.Split('\n');
            float maxWidth = WrapWidth;
            float spaceWidth = LabelTest.GetWidth(" ", false);

            StringBuilder sb = new();
            int loc = 0;
            foreach (string line in split)
            {
                offsets.Add(loc);
                if (line.Length == 0)
                {
                    output.Add("");
                    loc++;
                    continue;
                }
                sb.Clear();
                string[] words = line.Split(' ');
                float width = 0f;
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];
                    if (word.Length == 0)
                    {
                        sb.Append(' ');
                        width += spaceWidth;
                        continue;
                    }

                    float wordWidth = LabelTest.GetWidth(word, false);
                    if (wordWidth > maxWidth)
                    {
                        // Word is too big for line
                        if (width > 0f)
                        {
                            // Create a line for what we already have if line is not empty
                            output.Add(sb.ToString());
                            loc += output[output.Count - 1].Length;
                            offsets.Add(loc);
                            sb.Clear();
                            width = 0f;
                        }

                        // Split into sections that take up entire lines
                        do
                        {
                            int sliceAt = GoodSplitPoint(word);
                            output.Add(word.Substring(0, sliceAt));
                            loc += sliceAt;
                            offsets.Add(loc);
                            word = word.Substring(sliceAt);
                            wordWidth = LabelTest.GetWidth(word, false);
                        }
                        while (wordWidth > maxWidth);

                        // Final bit of the word that *does* fit in a line
                        sb.Append(word);
                    }
                    else if (width > 0f && width + spaceWidth + wordWidth > maxWidth)
                    {
                        // Not first word in line and it extends past boundary
                        sb.Append(' '); // we do need this or the total string length will be wrong
                        output.Add(sb.ToString());
                        loc += output[output.Count - 1].Length;
                        offsets.Add(loc);
                        sb.Clear();
                        width = 0f;
                        sb.Append(word);
                    }
                    else
                    {
                        // Word fits. Append a space if it isn't first in the line and then the word.
                        if (width > 0f)
                        {
                            sb.Append(' ');
                            width += spaceWidth;
                        }
                        sb.Append(word);
                    }
                    width += wordWidth;
                }
                output.Add(sb.ToString());
                loc += output[output.Count - 1].Length + 1; // + 1 accounts for newline character
            }

            if (output.Count == 0)
            {
                output.Add("");
                offsets.Add(0);
            }

            _valueLines = output;
            _valueLineOffsets = offsets;

            int GoodSplitPoint(string word)
            {
                int m = Math.Min((int)(WrapWidth / LabelTest.CharMean(false)), word.Length);
                float w = LabelTest.GetWidth(word.Substring(0, m));
                if (w > WrapWidth)
                {
                    do
                    {
                        m--;
                        w = LabelTest.GetWidth(word.Substring(0, m));
                    }
                    while (w > WrapWidth && m > 0);
                    return m;
                }
                else
                {
                    do
                    {
                        m++;
                        w = LabelTest.GetWidth(word.Substring(0, m));
                    }
                    while (w <= WrapWidth && m <= word.Length);
                    if (w > WrapWidth)
                    {
                        return m - 1;
                    }
                    else
                    {
                        return m;
                    }
                }
            }
        }

        protected void UpdateCursorPos(bool forceTextUpdate)
        {
            int lastLinesOffset = LinesOffset;
            _cursorChar = Mathf.Clamp(_cursorChar, 0, Math.Max(0, value.Length));

            // Update cursor position
            int cursorLine = Math.Max(0, _valueLineOffsets.FindLastIndex(x => x <= _cursorChar));
            Plugin.Logger.LogDebug($"Cursor char: {_cursorChar}");
            int cursorLineCol = _cursorChar - _valueLineOffsets[cursorLine];
            _cursorPos = (cursorLine, cursorLineCol);
            Plugin.Logger.LogDebug($"New cursor pos: {_cursorPos.line}:{_cursorPos.column}");

            // Update which lines are visible in the first place
            if (cursorLine < LinesOffset)
            {
                LinesOffset = cursorLine;
            }
            else if (cursorLine >= LinesOffset + labels.Count)
            {
                LinesOffset = cursorLine - labels.Count + 1;
            }
            Plugin.Logger.LogDebug("New offset: " + LinesOffset);

            // Update text
            if (lastLinesOffset != LinesOffset || forceTextUpdate)
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    var label = labels[i];
                    int line = LinesOffset + i;
                    if (line < _valueLines.Count)
                    {
                        label.text = _valueLines[line];
                    }
                    else
                    {
                        label.text = "";
                    }
                }
            }
        }

        public void Scroll(int lines)
        {
            if (lines == 0) return;

            if (lines < 0)
            {
                // Up a line
                if (_cursorPos.line + lines < 0)
                {
                    _cursorChar = 0;
                }
                else
                {
                    float width = LabelTest.GetWidth(_valueLines[_cursorPos.line].Substring(0, _cursorPos.column), false);
                    _cursorPos.line += lines;
                    LinesOffset += lines;
                    _cursorChar = _valueLineOffsets[_cursorPos.line] + NearestCharIndex(width, _valueLines[_cursorPos.line]);
                }
            }
            else
            {
                // Down a line
                if (_cursorPos.line + lines > _valueLines.Count - 1)
                {
                    _cursorChar = value.Length;
                }
                else
                {
                    float width = LabelTest.GetWidth(_valueLines[_cursorPos.line].Substring(0, _cursorPos.column), false);
                    _cursorPos.line += lines;
                    LinesOffset += lines;
                    _cursorChar = _valueLineOffsets[_cursorPos.line] + NearestCharIndex(width, _valueLines[_cursorPos.line]);
                }
            }

            UpdateCursorPos(true);

            static int NearestCharIndex(float at, string text)
            {
                // specific cases
                if (at == 0f) return 0;
                if (LabelTest.GetWidth(text) < at) return text.Length;

                int m = Math.Min((int)(at / LabelTest.CharMean(false)), text.Length);
                float w = LabelTest.GetWidth(text.Substring(0, m));
                if (w > at)
                {
                    do
                    {
                        m--;
                        w = LabelTest.GetWidth(text.Substring(0, m));
                    }
                    while (w > at && m > 0);
                    return m;
                }
                else
                {
                    do
                    {
                        m++;
                        w = LabelTest.GetWidth(text.Substring(0, m));
                    }
                    while (w <= at && m <= text.Length);
                    if (w > at)
                    {
                        return m - 1;
                    }
                    else
                    {
                        return m;
                    }
                }
            }
        }

        public void ScrollToTop()
        {
            _cursorChar = 0;
            UpdateCursorPos(true);
        }

        public void ScrollToBottom()
        {
            _cursorChar = Math.Max(0, value.Length);
            UpdateCursorPos(true);
        }

        public override string value
        {
            get => base.value;
            set
            {
                value ??= "";
                if (this.value == value) return;
                base.value = value;
                Change();
            }
        }

        public FLabelAlignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment != value)
                {
                    _alignment = value;
                    Change();
                }
            }
        }
        protected bool _KeyboardOn
        {
            get => _keyboardOn;
            set
            {
                if (_keyboardOn != value)
                {
                    held = value;
                    if (_keyboardOn && !value)
                    {
                        _cursor.isVisible = false;
                    }
                    _keyboardOn = value;
                    Change();
                }
            }
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
