using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using GeneticMIDI.Representation;
using DotNetMusic.Font;

namespace DotNetMusic.WPF
{

    public partial class MusicSheet : UserControl
    {
        
        private List<Note> notes = new List< Note>();

        Brush TEXTBRUSH = null;
        float FONTSIZEEM = 60;
        int PADDINGTOP = 40;
        int LINESPACING = 16;
        Pen BEAMPEN = null;

        int[] lines = new int[5];

        const int prime = 40;

        int highlightIndex = -1;

        public MusicSheet()
        {
            //InitializeComponent();
            
            TEXTBRUSH = new SolidColorBrush(Colors.Black);
            BEAMPEN = new Pen(Brushes.Black, 2.0f);

            InvalidateVisual();

            for (int i = 0; i < 5; i++)
            {
                lines[i] = PADDINGTOP + i * LINESPACING;
            }

        }

        public void SetNotes(Note[] notes)
        {
            this.notes.Clear();
            this.notes.AddRange(notes);
            this.InvalidateVisual();
            this.Width = prime * (notes.Length+3);
        }

        public void SetHighlightIndex(int n)
        {
            this.highlightIndex = n;
            this.InvalidateVisual();
        }

        public void AddMusicalSymbol(GeneticMIDI.Representation.Note symbol)
        {
            if (notes == null) return;
            notes.Add(symbol);
            InvalidateVisual();
        }


        public const string Staff5Lines = "=";
        public const string Staff4Lines = "_";
        public const string GClef = "G";
        public const string FClef = "?";
        public const string CClef = "K";
        public const string Sharp = "X";
        public const string Flat = "b";
        public const string Natural = "k";
        public const string DoubleSharp = "x";
        public const string DoubleFlat = "B";
        public const string WholeNote = "w";
        public const string HalfNote = "h";
        public const string QuarterNote = "q";
        public const string EighthNote = "e";
        public const string SixteenthNote = "s";
        public const string WholeRest = "W";
        public const string HalfRest = "H";
        public const string QuarterRest = "Q";
        public const string EighthRest = "E";
        public const string SixteenthRest = "S";
        public const string WhiteNoteHead = "9";
        public const string BlackNoteHead = "0";
        public const string NoteFlagEighth = "1";
        public const string NoteFlagSixteenth = "2";
        public const string NoteFlag32nd = "3";
        public const string NoteFlag64th = "4";
        public const string NoteFlag128th = "5";
        public const string NoteFlagEighthRev = "!";
        public const string NoteFlagSixteenthRev = "@";
        public const string NoteFlag32ndRev = "#";
        public const string NoteFlag64thRev = "$";
        public const string NoteFlag128thRev = "%";
        public const string Dot = ".";
        public const string CommonTime = "c";
        public const string CutTime = "C";
        public const string RepeatForward = @"\";
        public const string RepeatBackward = @"l";

        private void DrawString(DrawingContext d, string text, Typeface f, Brush b, float xPos, float yPos, float emSize, bool flip = false)
        {
            FormattedText textF = new FormattedText(text, Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight, f, emSize, b);

            var point = new Point(xPos, yPos);
            //flip = true;
            //This function mimics Graphics.DrawString functionality
            //flip = true;
            if(flip)
            {
                //Matrix mat = new Matrix()
                
             //   Transform rt = new MatrixTransform(Constants.m11,Constants.m12,Constants.m21,Constants.m22, Constants.offSetX, Constants.offsetY);
                Transform rt = new MatrixTransform(1, 0, 0, -1, 0, textF.Height);
                point = new Point(xPos, -yPos);
                //point = new Point(-xPos, yPos);
                
                d.PushTransform(rt);
                //point = new Point(-yPos, xPos);
            }
            d.DrawText(textF, point);
            if (flip)
                d.Pop();
        }


        private void DrawString(DrawingContext d, string text, Brush b, float x, float y, float emSize)
        {
            DrawString(d, text, TypeFaces.StaffFont, b, x, y, emSize);
        }

        private bool ContainsSharp(NoteNames note)
        {
            if (note == NoteNames.As || note == NoteNames.Cs || note == NoteNames.Ds || note == NoteNames.Fs || note == NoteNames.GS)
                return true;
            return false;
        }
   
        private void DrawNote(DrawingContext drawingContext,NoteNames note, Durations duration, float x, float y, int alter = 0, bool highlight = false)
        {
            string flag = "";
            string character = "";
            int index = 0;

            if ((int)duration < (int)Durations.hn)
                character = "0";
            else if (duration == Durations.hn)
                character = "9";
            if (duration == Durations.wn)
                character = "w";

            
            if (duration == Durations.en)
                flag = "1";
            else if (duration == Durations.sn)
                flag = "2";
            else if ((int)duration <= (int)Durations.tn)
                flag = "3";

            if(note == NoteNames.Rest)
            {
                if(duration == Durations.wn)
                    character = "W";
                if(duration == Durations.hn)
                    character = "H";
                if (duration == Durations.qn)
                    character = "Q";
                if(duration == Durations.en)
                    character = "E";
                if(duration == Durations.sn)
                    character = "S";
                index = 3;
            }

            
            #region index_switch
            switch (note)
            {
                case NoteNames.A:
                    index = 1;
                    break;
                case NoteNames.As:
                    index = 1;
                    break;
                case NoteNames.B:
                    index = 2;
                    break;
                case NoteNames.C:
                    index = 3;
                    break;
                case NoteNames.Cs:
                    index = 3;
                    break;
                case NoteNames.D:
                    index = 4;
                    break;
                case NoteNames.Ds:
                    index = 4;
                    break;
                case NoteNames.E:
                    index = 5;
                    break;
                case NoteNames.F:
                    index = 6;
                    break;
                case NoteNames.Fs:
                    index = 6;
                    break;
                case NoteNames.G:
                    index = 0;
                    break;
                case NoteNames.GS:
                    index = 0;
                    break;
                default:
                    break;
            }
            #endregion


            // Change note color if highlighted
            
            TEXTBRUSH = new SolidColorBrush(Colors.Black);
            BEAMPEN = new Pen(Brushes.Black, 2.0f);
            if (highlight)
            {
                TEXTBRUSH = new SolidColorBrush(Colors.Red);
                BEAMPEN = new Pen(Brushes.Red, 2.0f);
            }


            index += alter;

            float y_pos = y - index * (LINESPACING / 2);
            
            //Horizontal bar under note
            if(index > 7 || index < -1)
            {
                Pen pen = new Pen(Brushes.Black, 1.0f);
                drawingContext.DrawLine(pen, new Point(x - 6, y_pos + LINESPACING * 3 + LINESPACING / 2), new Point(x + 34, y_pos + LINESPACING * 3 + LINESPACING / 2));
            }

            y -= 6;
            if (ContainsSharp(note)) // Sharp adjustment
                index -= 1;

            int emMod = 0;
            if (note == NoteNames.Rest)
            {
                y_pos = y - LINESPACING;
                emMod = 2;
                y_pos -= 8;
            }
            DrawString(drawingContext, character, TypeFaces.StaffFont,TEXTBRUSH, x, y_pos, FONTSIZEEM+emMod);

            bool flip = index > 1;

            float length = LINESPACING * 3;
            if (duration != Durations.wn && note != NoteNames.Rest)
            {
                float lineX = 0;
                float lineY = 0;
                
                float flagX = 0;
                float flagY = 0;
                if (!flip)
                {
                    lineX = x + FONTSIZEEM / 2 - 9;
                    lineY = y_pos + 6;
                    flagX = lineX;
                    flagY = lineY - length;
                }
                else
                {
                    lineX = x + 7;
                    lineY = y_pos + length + 6;
                    flagX = lineX;
                    flagY = lineY;
                }

                // Draw the sharp accidental
                if(ContainsSharp(note))
                {
                    DrawString(drawingContext, "X", TypeFaces.StaffFont, TEXTBRUSH, x-10, y_pos+5, FONTSIZEEM - 10);
                }
                
                drawingContext.DrawLine(BEAMPEN, new Point(lineX, lineY), new Point(lineX, lineY + length));
                if (flag != "")
                    DrawString(drawingContext, flag, TypeFaces.StaffFont, TEXTBRUSH, flagX, flagY, FONTSIZEEM, flip);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);

            Pen pen = new Pen(Brushes.Black, 1.0f);
            Pen beamPen = new Pen(Brushes.Black, 2.0f);
            
           
            List<float> previousStemEndPositionsY = new List<float>();

            Point startPoint = new Point(0, PADDINGTOP);
            Point endPoint = new Point(Width, PADDINGTOP);

            for (int i = 0; i < 5; i++)
            {
                drawingContext.DrawLine(pen, startPoint, endPoint);
                startPoint.Y += LINESPACING;
                endPoint.Y += LINESPACING;

            }

            // Draw clef
            DrawString(drawingContext, "g", TypeFaces.StaffFont, TEXTBRUSH, 0, PADDINGTOP - 11, 5*LINESPACING);

            //DrawString(drawingContext, currentClef.MusicalCharacter, TypeFaces.StaffFont, textBrush, 0, 0, 50);

            if (notes.Count < 1)
            {
                
                int j = 2;
                DrawNote(drawingContext, NoteNames.G, Durations.wn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.A, Durations.hn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.B, Durations.qn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.Cs, Durations.qn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.D, Durations.en, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.E, Durations.sn, (j++) * prime, PADDINGTOP);

                DrawNote(drawingContext, NoteNames.Rest, Durations.wn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.Rest, Durations.hn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.Rest, Durations.qn, (j++) * prime, PADDINGTOP);
                DrawNote(drawingContext, NoteNames.Rest, Durations.en, (j++) * prime, PADDINGTOP);

                DrawNote(drawingContext, NoteNames.F, Durations.tn, (j++) * 40, PADDINGTOP);
                //   DrawString(drawingContext, "q", TypeFaces.StaffFont, TEXTBRUSH, i*20, 0, 20);
                this.Width = 20 * prime;
            }

            else
            {
                for(int i = 0; i < notes.Count; i++)
                {
                    var note = notes[i];
                    NoteNames noteName = note.GetNoteName();
                    Durations dur = note.GetStandardNoteDuration();
                    DrawNote(drawingContext, noteName, dur, (i+2) * prime, PADDINGTOP,0,highlightIndex == i);
                   // DrawNote(drawingContext, noteName, )
                }
            }
        }

  
    }
}
