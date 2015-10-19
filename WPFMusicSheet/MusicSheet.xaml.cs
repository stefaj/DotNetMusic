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
using DotNetMusic.Font;
using AlphaTab.Model;
using AlphaTab;
using System.Collections.ObjectModel;
using AlphaTab.Rendering;
using System.Windows.Interop;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DotNetMusic.WPF
{

    public partial class MusicSheet : UserControl
    {

        public static readonly RoutedEvent RenderFinishedEvent = EventManager.RegisterRoutedEvent("RenderFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MusicSheet));


        Score score;

                public Track Track{get;set;}

   

        public Settings Settings{get; set;}

        private ScoreRenderer _renderer;


        public ObservableCollection<ImageSource> PartialResults { get; set; }

        public MusicSheet()
        {
            InitializeComponent();

            Init();
        }

        void Init()
        {
            SnapsToDevicePixels = true;
            var settings = Settings.Defaults;
            settings.Engine = "gdi";
            settings.Layout.Mode = "horizontal";
            Settings = settings;

            PartialResults = new ObservableCollection<ImageSource>();
            _renderer = new ScoreRenderer(settings, this);

            _renderer.PreRender += () =>
            {
                lock (this)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PartialResults.Clear();
                    }));
                }
            };
            _renderer.PartialRenderFinished += result =>
            {
                lock (this)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DrawResult(result);
                        //AddPartialResult(result);
                    }));
                }
            };
            _renderer.RenderFinished += result =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OnRenderFinished(result);
                }));
            };
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private void AddPartialResult(RenderFinishedEventArgs result)
        {
            lock (this)
            {
                Width = result.TotalWidth;
                Height = result.TotalHeight;
                var bitmap = (Bitmap)result.RenderResult;
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                    PartialResults.Add(Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height)));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        private void DrawResult(RenderFinishedEventArgs result)
        {
            lock (this)
            {
                Width = result.TotalWidth;
                Height = result.TotalHeight;
                var bitmap = (Bitmap)result.RenderResult;
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                  //  ImageSourceConverter c = new ImageSourceConverter();
                 //   var imgSOurce = (c.ConvertFrom(bitmap)) as ImageSource;
                    
                 //   imagi.Source = imgSOurce;
                    var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                    imagi.Source = bitmapSource;
                    PartialResults.Add(bitmapSource);
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        protected virtual void OnRenderFinished(RenderFinishedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(RenderFinishedEvent);
            RaiseEvent(newEventArgs);
        }

        public void SetHighlight(int index, bool isHighlighted)
        {
            var track = score.Tracks[0];
            var voice = track.Bars[0].Voices[0];

            if (index < voice.Beats.Count)
            {
                var beat = voice.Beats[index];
                foreach (var n in beat.Notes)
                {
                    n.IsHighlighted = isHighlighted;
                }
            }            
            
        }

        int highlightIndex = -1;
        public void SetHighlightIndex(int index)
        {
            if (index < 0)
                return;
            if (score.Tracks.Count < 1)
                return;

            if(highlightIndex > -1)
                SetHighlight(highlightIndex, false);
            SetHighlight(index, true);
            highlightIndex = index;
            
            _renderer.Render(score.Tracks[0]);
        }

        public void SetNotes(GeneticMIDI.Representation.Track track)
        {

            var mel = track.GetMelodySequence();


            //return;

            score = new Score();

            Track t = new Track();
            

            MasterBar mb = new MasterBar();
            score.AddMasterBar(mb);
            mb.KeySignature = 2;

            Bar b = new Bar();
            t.AddBar(b);
            score.AddTrack(t);

            Voice v = new Voice();
            b.AddVoice(v);

            t.Name = track.Instrument.ToString().Replace("_", " ");

            //t.IsPercussion = true;
            if(t.IsPercussion)
            {
                
                b.Clef = Clef.Neutral;
            }

            int i = 0;

            int qn_per_bar = 4;

            int durs = 0;
            int avg_octave = mel.CalculateAverageOctave();
            int dist4 = 4 - avg_octave;
            foreach (var n in mel.Notes)
            {
                Beat be = new Beat();
                be.Index = i++;

                
                GeneticMIDI.Representation.Durations dur;
                int remainder;

                n.GetClosestLowerDurationAndRemainder(out dur, out remainder);

                int dots = n.GetNumberOfDots();

                durs += n.Duration;

                /*        if(durs >= qn_per_bar * (int)GeneticMIDI.Representation.Durations.qn)
                        {
                            durs = 0;
                            b = new Bar();
                            t.AddBar(b);
                            v.Bar = b;
                            b.Finish();
                        }*/

                switch (((GeneticMIDI.Representation.Durations)n.Duration))
                {
                    case GeneticMIDI.Representation.Durations.bn:
                        be.Duration = AlphaTab.Model.Duration.Whole;
                        dots = 2;
                        break;
                    case GeneticMIDI.Representation.Durations.en:
                        be.Duration = AlphaTab.Model.Duration.Eighth;
                        break;
                    case GeneticMIDI.Representation.Durations.hn:
                        be.Duration = AlphaTab.Model.Duration.Half;
                        break;
                    case GeneticMIDI.Representation.Durations.qn:
                        be.Duration = AlphaTab.Model.Duration.Quarter;
                        break;
                    case GeneticMIDI.Representation.Durations.sn:
                        be.Duration = AlphaTab.Model.Duration.Sixteenth;
                        break;
                    case GeneticMIDI.Representation.Durations.tn:
                        be.Duration = AlphaTab.Model.Duration.ThirtySecond;
                        break;
                    case GeneticMIDI.Representation.Durations.wn:
                        be.Duration = AlphaTab.Model.Duration.Whole;
                        break;
                    default:
                        break;
                }
                be.Dots = dots;

                Note note = new Note();
                
                
                if (!n.IsRest())
                {
                    note.Tone = n.NotePitch;
                    note.Octave = n.Octave + dist4;

                    be.AddNote(note);
                    be.IsEmpty = false;                       
                }

                if (n.IsRest() && n.Duration < 2)
                {

                }
                else
                    v.AddBeat(be);

                be.RefreshNotes();



            }

            

            v.Bar = b;

            v.Finish();

            b.Finish();

            t.Finish();


            score.Finish();

 
            //TablatureControl

            _renderer.Render(t);
            return;

            /*TablatureControl.Track = t; 
            TablatureControl.InvalidateVisual();
            TablatureControl.InvalidateTrack();        */


            
        }

  
    }
}
