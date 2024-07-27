using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DirectionOperations;


public class BreadthFirstMind : AbstractPathMind
{
    private bool _isPathCalculated = false;
    private CellInfo _endPoint;
    private List<CellInfo> _pathToFollow;
    private int _pathToFollowIndex = 1;

    [SerializeField] private GameObject openListPrefab;       //Representación visual de los nodos a seguir
    [SerializeField] private GameObject closedListPrefab;		//Representación visual de los nodos explorados

    public override void Repath()
    {
        throw new System.NotImplementedException();
    }

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        //Nos aseguramos de que se calcula el camino una sola vez al comienzo, comprobando si el camino ya se ha calculado
        if (!this._isPathCalculated)
        {
            this._isPathCalculated = true;
            //Guardamos el camino a seguir despues de resolver el grafo y reconstruir el camino en base a él
            this._pathToFollow = ReconstructPath(BreadthFirstSearch(currentPos, boardInfo));
        }

        //Calculamos la direccion que vamos a seguir en funcion de nuestra posicion actual y de la siguiente posicion en la lista
        Locomotion.MoveDirection tempDirection = DirectionOperations.CalculateDirectionToAdjacentCell( currentPos, this._pathToFollow[this._pathToFollowIndex]);
        this._pathToFollowIndex++;

        return tempDirection;
    }

    //Resuelve el grafo con la busqueda en amplitud y devuelve una lista de celdas con referencia a los vecinos desde las que se encontró cada celda
    private List<CellBreadthFirstMindInfo> BreadthFirstSearch(CellInfo startingCell, BoardInfo boardInfo)
    {
        //Crea la cola de elementos que se van a recorrer y mete el punto de inicio en la cola
        var q = new Queue<CellInfo>();
        q.Enqueue(startingCell);

        //Crea una lista que guarda referencia a cada celda que se haya recorrido y al padre celda desde el que se ha legado.
        //Se utiliza para reconstruir el  camino una vez encontrado el objetivo
        //Añadimos nuestra primera celda con null como padre, ya que no ha sido encontrada desde ninguna otra celda
        var parentList = new List<CellBreadthFirstMindInfo>();
        parentList.Add(new CellBreadthFirstMindInfo(startingCell, null));

        //Recorremas la lista de celdas encoladas mientras siga llena
        while (q.Count != 0)
        {
            //Sacamos el elemento de la lista para que no se vuelva a explorar
            var node = q.Dequeue();
            //Recuperamos una aray de todos las celdas vecinas a explorar
            var neighbours = node.WalkableNeighbours(boardInfo);

            Instantiate(closedListPrefab).transform.position = node.GetPosition;

            //Recorremos las celdas vecinas para meterlas en la cola
            for (int i = 0; i < neighbours.Length; i++)
            {
                //Comprobamos si las celdas son recorribles
                if (neighbours[i] != null)
                {
                    //Añadimos cada vecino exlporado a la lista junto con el padre desde el que hemos llegado a él
                    parentList.Add(new CellBreadthFirstMindInfo( neighbours[i], node));

                    //Comprobamos si ha alcanzado la meta
                    if (neighbours[i] == boardInfo.Exit)
                    {
                        Debug.Log("Found goal at coordinates: " + neighbours[i].CellId);
                        Debug.Log("Goal found after iterating " + parentList.Count + " times");

                        //Si se alcanza la meta, guardamos la celda objetivo y devolvemos la lista con las celdas y sus padres

                        this._endPoint = neighbours[i];

                        return parentList;
                    }

                    q.Enqueue(neighbours[i]);
                    neighbours[i].ChangeToNoWalkable();

                }
            }
        }

        //Si se acaban los elemntos de la cola, no se ha encontrado el punto final, por lo tanot se devuelve una lista vacia
        Debug.Log("No goal found");
        return new List<CellBreadthFirstMindInfo>();
    }

    //Construye el camino a seguir en base a la lista con las celdas y los padres desde las que se llegaron a dichas celdas
    private List<CellInfo> ReconstructPath(List<CellBreadthFirstMindInfo> parentCells)
    {
        var path = new List<CellInfo>();

        //Recorremos la lista, emepezando por el final, añadiendo cada elemento a la lista y sustiteyendolo por su padre para recorrer el camino e principio a fin
        for (CellInfo i = this._endPoint; i != null; i = parentCells.Find(CellParent => CellParent.Cell == i).NeighbourFather)
        {
            Instantiate(openListPrefab).transform.position = i.GetPosition;

            path.Add(i);
        }

        path.Reverse();

        return path;
    }

    //Estructura que contiene referencia a una celda y a la celda desde la que se encontro
    private struct CellBreadthFirstMindInfo
    {
        public CellInfo Cell { get; private set; }
        public CellInfo NeighbourFather { get; private set; }

        public CellBreadthFirstMindInfo(CellInfo cell , CellInfo cellParent)
        {
            this.Cell = cell;
            this.NeighbourFather = cellParent;
        }
    }

}
