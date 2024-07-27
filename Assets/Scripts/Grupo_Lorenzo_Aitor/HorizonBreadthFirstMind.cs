using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DirectionOperations;

public class HorizonBreadthFirstMind : AbstractPathMind
{
    private CellInfo _objectiveEndCell;
    private CellInfo _pathEndCell;
    private List<CellInfo> _pathToFollow;
    [SerializeField] private int _targetDepth = 1;

    [SerializeField] private GameObject openListPrefab;       //Representación visual de los nodos a seguir
    [SerializeField] private GameObject closedListPrefab;		//Representación visual de los nodos explorados

    private List<GameObject> pathPrefabs = new List<GameObject>();

    public override void Repath()
    {
        throw new System.NotImplementedException();
    }

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        //Limpiamos lños objetos que represenatn el camino a seguir y als celdas exploradas
        pathPrefabs = DeleteObjectsAndEmptyList(pathPrefabs);
        //Primero fijamos la celda objetivo que vamos a usar como referencia, que será la celda de un enemigo o la meta
        CalculateObjectiveEndCell(boardInfo);
        print("Objective: " + this._objectiveEndCell.CellId);

        //Calculamos el camino que vamos a tomar despues de resolver la busqueda por horizonte
        this._pathToFollow = ReconstructPath(BreadthFirstSearch( currentPos, boardInfo));

        //Devolvemos un valor de dirección en función de la celda en la que nos encontramos y el paso siguente en el camino
        return DirectionOperations.CalculateDirectionToAdjacentCell(currentPos, this._pathToFollow[1]);
            
    }

    //Limpia la lista de objetos
    private List<GameObject> DeleteObjectsAndEmptyList( List<GameObject> list)
    {

        foreach (var listElement in list)
        {
            Destroy(listElement);
        }

        list.Clear();

        return list;
    }

    //Setea la celda objetivo final. Si hay enemigos, esocoge el primero de la lista, si no los hay, toma como objetivo la salida
    private void CalculateObjectiveEndCell(BoardInfo boardInfo)
    {
        if (boardInfo.Enemies.Count > 0)
        {
            this._objectiveEndCell = boardInfo.Enemies[0].CurrentPosition();
        }
        else
        {
            this._objectiveEndCell = boardInfo.Exit;
        }
    }

    //Resuelve el grafo con la busqueda en horizonte
    private List<CellHorizonMindInfo> BreadthFirstSearch(CellInfo startingCell, BoardInfo boardInfo)
    {
        //Crea la cola de elementos que se van a recorrer y mete el punto de inicio en la cola
        var q = new Queue<CellInfo>();
        q.Enqueue(startingCell);

        //Crea una lista que guarda información adicional de la celda, como la heuristica o la profundidad
        //Se utiliza para reconstruir el  camino una vez encontrado el objetivo
        //Añadimos nuestra primera celda con null como padre, ya que no ha sido encontrada desde ninguna otra celda
        var AdditionalCellsInfo = new List<CellHorizonMindInfo>();
        AdditionalCellsInfo.Add(new CellHorizonMindInfo(startingCell, null, 0));

        //Recorremos las celdas en cola mientras esta siga llena
        while (q.Count != 0)
        {
            //Sacamos el elemento que vamos a explorar de la lista y guardamos las celdas vecinas
            var node = q.Dequeue();
            var neighbours = node.WalkableNeighbours(boardInfo);

            //Guardamos la informacion adicional de la celda nodo para usarla mad adelante
            var AdditionalNodeInfo = AdditionalCellsInfo.Find(CellMindInfo => CellMindInfo.Cell == node);

            //Recorremos los vecinos de ese nodo
            for (int i = 0; i < neighbours.Length; i++)
            {
                //Nos aseguramos de que ese nodo es transitable y de que no se ha recorrido ya
                if (neighbours[i] != null && !AdditionalCellsInfo.Contains(AdditionalCellsInfo.Find(cellInfo => cellInfo.Cell == neighbours[i])))
                {
                    pathPrefabs.Add(Instantiate(closedListPrefab, neighbours[i].GetPosition, Quaternion.identity));

                    //Si la profundidad del nodo que estabamos explorando es 1 menos que la objetivo, significa que sus vecinos tendrán la prfundidad objetivo
                    //Por lo tanto añadimos sus vecinos con su Heuristica calculada
                    if (AdditionalNodeInfo.DepthValue == this._targetDepth - 1)
                    {
                        AdditionalCellsInfo.Add(new CellHorizonMindInfo(neighbours[i], node, AdditionalNodeInfo.DepthValue + 1, Heuristic(node, startingCell, this._objectiveEndCell)));
                    }
                    //Si la profundidad es menor, significa que sus vecinos aun no seran de la profundidad objetivo, y por lo tanto se les añade a la lista con una Herística muy alta
                    //También se añade este elemento a la cola para que sea explorado hasta llegar a la profundidad objetivo
                    else if (AdditionalNodeInfo.DepthValue < this._targetDepth - 1)
                    {
                        AdditionalCellsInfo.Add(new CellHorizonMindInfo(neighbours[i], node, AdditionalNodeInfo.DepthValue + 1));

                        q.Enqueue(neighbours[i]);
                    }

                    //Comprobamos si el nodo vecino es el objetivo, en ese caso, devolvemos la lista y seteamos como celda objetivo dicha celda
                    if (neighbours[i] == this._objectiveEndCell)
                    {
                        this._pathEndCell = neighbours[i];
                        return AdditionalCellsInfo;
                    }
                }
            }
        }

        //Si la cola se queda sin elementos, significa que hemos llegado a la profundidad objetivo sin encontrar la celda objetivo.
        //Por lo tanto, se evalua la heuristica de todas las celdas y se pone como celda objetivo para el camino a generar
        SetCurrentEndCellToMinHeursiticCellInList(AdditionalCellsInfo);

        return AdditionalCellsInfo;
    }

    //Construye el camino a seguir en base a la lista con las celdas y los padres desde las que se llegaron a dichas celdas
    private List<CellInfo> ReconstructPath(List<CellHorizonMindInfo> parentCells)
    {
        var path = new List<CellInfo>();

        //Recorremos la lista, emepezando por el final, añadiendo cada elemento a la lista y sustiteyendolo por su padre para recorrer el camino e principio a fin
        for (CellInfo i = this._pathEndCell; i != null; i = parentCells.Find(CellParent => CellParent.Cell == i).NeighbourFather)
        {
            pathPrefabs.Add(Instantiate(openListPrefab, new Vector3 (i.GetPosition.x, i.GetPosition.y, -1), Quaternion.identity));

            path.Add(i);
        }

        path.Reverse();

        return path;
    }

    //Setea como final del camino la celda con menos heuristica de la lista
    private void SetCurrentEndCellToMinHeursiticCellInList(List<CellHorizonMindInfo> desiredList)
    {
        var tempMindCellInfo = desiredList[0];
        foreach (var mindCell in desiredList)
        {
            if (mindCell.HeuristicValue < tempMindCellInfo.HeuristicValue)
            {
                tempMindCellInfo = mindCell;
            }

        }

        this._pathEndCell = tempMindCellInfo.Cell;
    }

    //Devuelve la heuristica de una celda como la suma de la distancia de dicha celda al objetivo final y al principio
    private float Heuristic( CellInfo cellToCalculate, CellInfo startingPoint, CellInfo objectiveCell)
    {
        return (cellToCalculate.GetPosition - objectiveCell.GetPosition).magnitude + (cellToCalculate.GetPosition - startingPoint.GetPosition).magnitude;
    }

    //Estructura que añade informacion a la celda
    private struct CellHorizonMindInfo
    {
        public CellInfo Cell { get; private set; }
        public CellInfo NeighbourFather { get; private set; }
        public int DepthValue { get; private set; }
        public float HeuristicValue { get; set; }
        public bool Visited { get; set; }

        public CellHorizonMindInfo(CellInfo cell, CellInfo cellParent, int depthValue)
        {
            this.Cell = cell;
            this.NeighbourFather = cellParent;
            this.HeuristicValue = 10000000.0f;
            this.DepthValue = depthValue;
            this.Visited = true;
        }

        public CellHorizonMindInfo(CellInfo cell, CellInfo cellParent, int depthValue, float heuristicValue)
        {
            this.Cell = cell;
            this.NeighbourFather = cellParent;
            this.HeuristicValue = heuristicValue;
            this.DepthValue = depthValue;
            this.Visited = true;
        }
    }
}
