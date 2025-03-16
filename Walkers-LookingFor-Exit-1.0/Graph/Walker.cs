using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using static MathGraph.Graph;

namespace MathGraph
{
    class Walker
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler MessageNotify;
        public delegate void TimerHandler();
        public event TimerHandler TimerNotify;

        public Vector pos;
        int old_node;
        public int current_node, id;

        Random rnd;
        int radius;
        int state;
        Node next_node = null;
        Vector direction = new Vector();
        Dictionary<int, int> visited; // key is node index, value is visited count
        //int dist_path;
        public int exit_point;

        public Walker(Random rand)
        {
            rnd = rand;
            radius = 2;
            pos = new Vector();
            visited = new Dictionary<int, int>();
        }

        public void Update(List<Edge> edges, List<Node> nodes)
        {
            switch (state)
            {
                case 0: // init new way
                    
                    if (current_node == exit_point) // exit found
                    {
                        MessageNotify?.Invoke("\nwalker " + id + ": exit was found!");
                        TimerNotify?.Invoke();
                        return;
                    }

                    (next_node, direction) = GetNewWay(edges, nodes);
                    if (next_node is null) break;

                    //MessageNotify?.Invoke("\nwalker " + id + ": next node is " + next_node.id);
                    old_node = current_node;
                    state = 1;

                    break;

                case 1: // update position
                    pos += direction;
                    var inCheckPoint = isInCheckPoint(next_node);
                    if (inCheckPoint)
                    {
                        current_node = next_node.id;
                        var dist = edges.Where(e => e.from == old_node && e.to == current_node || e.to == old_node && e.from == current_node).Select(d => d.dist).ToList();
                        //dist_path += (int)dist[0];
                        if (!visited.ContainsKey(current_node))
                        {
                            visited.Add(current_node, 1);
                            nodes[current_node].label = 1;
                        }
                        else
                            visited[current_node] += 1;
                        
                        //MessageNotify?.Invoke(" / path is " + dist_path);
                        state = 0;
                    }
                    break;
            }
        }
        private bool isInCheckPoint(Node n)
        {
            if (Math.Abs((n.pos.X - pos.X)) < 0.8 && Math.Abs((n.pos.Y - pos.Y)) < 0.8)
            {
                return true;
            }
            return false;
        }
        private (Node, Vector) GetNewWay(List<Edge> edges, List<Node> nodes)
        {
            var next_node = GetNextNode(edges, nodes);
            if (next_node is null)
                return (null, new Vector(0,0));

            var direction = GetDirection(next_node.pos);

            return (next_node, direction);
        }
        private int FindFreeNextNode(List<int> nearest_nodes)
        {
            int prob_index = 0;
            int limit = nearest_nodes.Count;

            // not visited case
            var nonvisited_nodes = new List<int>();
            foreach(var n in nearest_nodes)
            {
                if (!visited.ContainsKey(n))
                    nonvisited_nodes.Add(n);
            }
            int count = nonvisited_nodes.Count();

            prob_index = rnd.Next(count);
            if (count > 0)
                return nonvisited_nodes[prob_index];

            return -1;
        }
        private Node GetNextNode(List<Edge> edges, List<Node> nodes)
        {
            var nearest_nodes = edges.Where(n => n.from == current_node).Select(n => n.to).ToList();
            var nearest_nodes_ = edges.Where(n => n.to == current_node).Select(n => n.from).ToList();
            nearest_nodes.AddRange(nearest_nodes_);

            // not visited case
            int new_node_index = FindFreeNextNode(nearest_nodes);
            if (new_node_index != -1)
                return nodes[new_node_index];

            // all nodes visited case
            var visited_sorted = visited
                        .Where(v => nearest_nodes.Any(n => v.Key == n))
                        .OrderBy(v => v.Value)
                        .ToList();

            new_node_index = visited_sorted[0].Key;
            return nodes[new_node_index];
        }
        private Vector GetDirection(Vector p)
        {
            Vector dir = p - pos;
            dir.Normalize();
            return dir;
        }

        public void Draw(DrawingContext dc) => dc.DrawEllipse(Brushes.Black, null, (Point)pos, radius, radius);
    }
}
