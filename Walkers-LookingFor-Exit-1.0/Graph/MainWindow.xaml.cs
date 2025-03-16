using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MathGraph
{
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        DispatcherTimer timer = new DispatcherTimer();
        int width, height;
        public static DrawingVisual visual;
        DrawingContext dc;

        Graph NSW;
        List<Walker> walkers;
        int nodes_count, exit_node;

        public MainWindow()
        {
            InitializeComponent();

            visual = new DrawingVisual();

            width = (int)g.Width;
            height = (int)g.Height;

            timer.Tick += Tick;
            timer.Interval = new TimeSpan(0,0,0,0, 10);
        }

        private void Init()
        {
            timer.Stop();

            NSW = new Graph(rnd, width, height);
            NSW.Notify += msg =>
            {
                rtbConsole.AppendText(msg);
            };
            nodes_count = 100;
            exit_node = 25;
            NSW.ConstructionStatic(nodes_count);

            int walkers_count = 5;
            rtbConsole.Document.Blocks.Clear();
            rtbConsole.AppendText("Looking for an exit in node " + exit_node
                                    + ".\n    - Nodes number is " + nodes_count
                                    + ".\n    - Walkers number is " + walkers_count
                                    + ".\nWalkers go priority and randomly through non visited nodes then go through other.\n");

            walkers = new List<Walker>();
            for(int i = 0; i < walkers_count; ++i)
            {
                var walker = new Walker(rnd);
                walker.id = i;
                walker.pos = NSW.nodes[1].pos;
                walker.current_node = 1;

                walker.MessageNotify += (msg) =>
                {
                    rtbConsole.AppendText(msg);
                };
                walker.TimerNotify += () =>
                {
                    NSW.nodes[exit_node].label = 2; // 2 means red color for found node
                    timer.Stop();
                    Drawing();
                };
                walkers.Add(walker);
            }    
        }

        private void Tick(object sender , EventArgs e)
        {
            foreach (var w in walkers)
                w.Update(NSW.edges, NSW.nodes);

            Drawing();
        }

        private void btnDynamic_Click(object sender, RoutedEventArgs e)
        {
            if (walkers is null) return;
            
            if (!timer.IsEnabled)
                timer.Start();
            else
                timer.Stop();

            foreach (var w in walkers)
                w.exit_point = exit_node;
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                NSW.Draw(dc);
                foreach (var w in walkers)
                    w.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void g_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void btnStatic_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Drawing();
        }
    }
}
