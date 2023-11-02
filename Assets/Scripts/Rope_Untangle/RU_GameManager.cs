using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Rope_Untangle
{
    [Serializable]
    public class Places
    {
        public int coord;
        public PlaceableCoords place;
    }
    
    public class RU_GameManager : MonoBehaviour
    {
        public static RU_GameManager Instance;
        readonly Dictionary<GameObject, List<GameObject>> _adjacencyList = new Dictionary<GameObject, List<GameObject>>();
        public GameObject[] positions;

        public GameObject[] ropes;
        public List<Places> places;
        public GameObject _midAirRope = null;
        
        [SerializeField] private float timeToMove = 0.5f;
    
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            AddEdge(positions[1],positions[2]);
            AddEdge(positions[2],positions[4]);
            AddEdge(positions[3],positions[4]);
            AddEdge(positions[4],positions[6]);
            AddEdge(positions[5],positions[6]);
            
            for(int i=0;i<ropes.Length;i++)
            {
                //ropes[i].transform.position = positions[i+1].transform.position;
                places[i].place.UpdateStatus(true);
            }
        }

        // Method That is Responsible to Move Rope Up
        public void MoveUp(GameObject curr)
        {
            var currRope = curr.GetComponent<Rope>();
            int n = currRope.currentPos;
            StartCoroutine(Move(curr, positions[n].transform,positions[n+1].transform));
            currRope.currentPos = n + 1;
            currRope.PrintCurrentPos();
            _midAirRope = curr;
            places[n/2].place.UpdateStatus(false);
        }
        
        // Method that is responsible to Move Rope down
        public void MoveDown()
        {
            var currRope = _midAirRope.GetComponent<Rope>();
            int n = currRope.currentPos;
            StartCoroutine(Move(_midAirRope, positions[n].transform,positions[n-1].transform));
            currRope.currentPos = n - 1;
            currRope.PrintCurrentPos();
            places[(n-1)/2].place.UpdateStatus(true);
            _midAirRope = null;
        }

        public void Move(int destiny)
        {
            GameObject rope = _midAirRope;
            var currRope = rope.GetComponent<Rope>();
            int currCoord = currRope.currentPos;
            int endCoords = destiny;

            if (currRope.inAir)
            {
                List<GameObject> path = FindShortestPath(currCoord,endCoords);
                Debug.Log(PathToString(path));
                StartCoroutine(TraversePath(path,rope));
            
                currRope.currentPos = destiny;
                currRope.PrintCurrentPos();
            }
            places[destiny/2].place.UpdateStatus(true);
            _midAirRope = null;
        }

        //Print path
        private string PathToString(List<GameObject> path)
        {
            List<string> nodeNames = new List<string>();
            foreach (var node in path)
            {
                nodeNames.Add(node.name);
            }

            return string.Join(" -> ", nodeNames);
        }

        //Find Path
        private List<GameObject> FindShortestPath(int starting,int ending)
        {
            var start = positions[starting];
            var end = positions[ending];
            
            
            Queue<GameObject> queue = new Queue<GameObject>();
            Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();
            List<GameObject> shortestPath = new List<GameObject>();
            
            queue.Enqueue(start);
            cameFrom[start] = null;
            
            while (queue.Count > 0)
            {
                GameObject curr = queue.Dequeue();

                if (curr == end)
                {
                    GameObject current = end;
                    while (current != null)
                    {
                        shortestPath.Add(current);
                        current = cameFrom[current];
                    }
                    shortestPath.Reverse();
                    return shortestPath;
                }

                if (_adjacencyList.TryGetValue(curr, out var value))
                {
                    foreach (GameObject neighbor in value)
                    {
                        if (!cameFrom.ContainsKey(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            cameFrom[neighbor] = curr;
                        }
                    }
                }
            }
            return shortestPath;
        }

        //move through the path
        IEnumerator TraversePath(List<GameObject> path,GameObject rope)
        {
            int n = path.Count;
            for (int i = 0; i < n - 1; i++)
            {
                yield return StartCoroutine(Move(rope, path[i].transform, path[i + 1].transform));
            }
        }
        
        // Move Coroutine between ode Current to another (Given Node)
        IEnumerator Move(GameObject node,Transform starting, Transform ending)
        {
            float t = 0;
            while (t < 1)
            {
                node.transform.position = Vector3.Lerp(starting.position, ending.position, t);
                t += Time.deltaTime / timeToMove;
                yield return new WaitForEndOfFrame();
            }
            node.transform.position = ending.position;
            yield return new Null();
        }
    
        // Function to add an edge (undirected)
        void AddEdge(GameObject nodeA, GameObject nodeB)
        {
            if (!_adjacencyList.ContainsKey(nodeA))
            {
                _adjacencyList[nodeA] = new List<GameObject>();
            }

            if (!_adjacencyList.ContainsKey(nodeB))
            {
                _adjacencyList[nodeB] = new List<GameObject>();
            }

            _adjacencyList[nodeA].Add(nodeB);
            _adjacencyList[nodeB].Add(nodeA);
        }
        
    }
}
