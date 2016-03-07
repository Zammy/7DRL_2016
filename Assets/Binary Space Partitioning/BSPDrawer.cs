using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using RogueLib;

namespace BSP
{
    public class BSPDrawer : MonoBehaviour 
    {
        public Transform Canvas;
        public GameObject NodePrefab;

        public float DrawSpeed = 1f;

        public void DrawNode(BinaryNode node)
        {
            StartCoroutine ( this.DrawNodes(new BinaryNode[] { node }, this.Canvas ) );
        }

        public void DrawRoomsAndCorridors(List<Room> rooms, List<Corridor> corridors)
        {
            foreach (var room in rooms)
            {
                var roomObj = (GameObject) Instantiate(NodePrefab);
                roomObj.transform.SetParent(this.Canvas);
                roomObj.GetComponent<Image>().color = Color.black;
                roomObj.name = "Room";

                var roomRectTrans = roomObj.transform as RectTransform;
                roomRectTrans.sizeDelta = new Vector2( room.Width, room.Height );
                roomRectTrans.anchoredPosition = new Vector2( room.GlobalPos.X, room.GlobalPos.Y);
            }

            foreach (var corridor in corridors)
            {
                Point prevPoint = corridor.Points[0];
                for (int i = 1; i < corridor.Points.Count; i++)
                {
                    Point newPoint = corridor.Points[i];
                    var corridorSig = (GameObject) Instantiate(NodePrefab);
                    corridorSig.transform.SetParent(this.Canvas);
                    corridorSig.GetComponent<Image>().color = Color.red;
                    corridorSig.name = "Corridor " + (i-1).ToString();

                    var corridorRectTrans = corridorSig.transform as RectTransform;
                    if (newPoint.X > prevPoint.X || newPoint.Y > prevPoint.Y)
                    {
                        corridorRectTrans.anchoredPosition = new Vector2( prevPoint.X, prevPoint.Y);
                    }
                    else
                    {
                        corridorRectTrans.anchoredPosition = new Vector2( newPoint.X, newPoint.Y);
                    }

                    corridorRectTrans.sizeDelta = new Vector2( Mathf.Max(1, Mathf.Abs( newPoint.X - prevPoint.X ) + 1), 
                                                        Mathf.Max(1, Mathf.Abs( newPoint.Y - prevPoint.Y ) + 1 ));
                    prevPoint = newPoint;
                }
            }
        }

        IEnumerator DrawNodes(BinaryNode[] nodes, Transform parent)
        {
            foreach (var node in nodes)
            {
                var leaves = new List<BinaryNode>();

                foreach(var leaf in node.Children)
                {
                    if (leaf != null)
                    {
                        leaves.Add(leaf);
                    }
                }

                var nodeTrans = Draw (node, parent, leaves.Count == 0);

                yield return new WaitForSeconds(this.DrawSpeed);

                if (leaves.Count != 0)
                {
                    yield return StartCoroutine( this.DrawNodes( leaves.ToArray(), nodeTrans ) );
                }
            }
        }

        Transform Draw(BinaryNode node, Transform parent, bool drawRoom = false)
        {
            var nodeObj = (GameObject) Instantiate(NodePrefab);
            nodeObj.transform.SetParent(parent);

            if (drawRoom)
            {
                var roomObj = (GameObject) Instantiate(NodePrefab);
                roomObj.transform.SetParent(nodeObj.transform);
                roomObj.GetComponent<Image>().color = Color.black;
                roomObj.name = "Room";
                var roomRectTrans = roomObj.transform as RectTransform;
                roomRectTrans.sizeDelta = new Vector2( node.Room.Width, node.Room.Height );
                roomRectTrans.anchoredPosition = new Vector2( node.Room.LocalPos.X, node.Room.LocalPos.Y);
            }

            var color = new Color( Random.Range(0.5f, 1f), Random.Range(0.25f,1f), Random.Range(0.25f, 0.75f));
            nodeObj.GetComponent<Image>().color = color;

            var rectTrans = nodeObj.transform as RectTransform;
            rectTrans.sizeDelta = new Vector2( node.Width, node.Height );
            rectTrans.anchoredPosition = new Vector2( node.LocalPos.X, node.LocalPos.Y);

            return nodeObj.transform;
        }
    }
}
