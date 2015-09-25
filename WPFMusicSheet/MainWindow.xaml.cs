using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicSheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GeneticMIDI.Representation.Composition comp = GeneticMIDI.Representation.Composition.LoadFromMIDI(@"C:\Users\1gn1t0r\Documents\git\GeneticMIDI\GeneticMIDI\bin\Debug\test\harry.mid");
            var seq = comp.GetLongestTrack().GetMainSequence() as GeneticMIDI.Representation.MelodySequence;
            int i = 0;
            foreach(var note in seq.ToArray())
            {
                if (i++ > 40)
                    break;
              
                Console.WriteLine(note.GetNotePitchString());
              //  viewer.AddMusicalSymbol(note);
            }
           // viewer.AddMusicalSymbol(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            viewer.InvalidateVisual();
        }
    }
}
