using System.Collections;
using System.Collections.Generic;

using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.DirectionOperations;
using Assets.Scripts.SampleMind;

using UnityEngine;

public class AstarMind : AbstractPathMind
{
	bool pathPrepared = false;
	private List<CellInfo> pathToFollow;
	private int _pathToFollowIndex = 0;

	[SerializeField] private GameObject openListPrefab;		//Representación visual de los nodos a seguir
	[SerializeField] private GameObject closedListPrefab;		//Representación visual de los nodos explorados

	public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
	{
		//Nos aseguramos de que sólo se realiza el pathfinding una vez
		if(!pathPrepared)
		{
			this.pathToFollow = AStarPathfinding(boardInfo, currentPos);
			this.pathPrepared = true;
		}
		Locomotion.MoveDirection tempDirection = DirectionOperations.CalculateDirectionToAdjacentCell(currentPos, this.pathToFollow[this._pathToFollowIndex]);
		if(!(_pathToFollowIndex++ < pathToFollow.Count))
        {	
			//Limitamos el índice con el cap más alto, evitando que se salga de rango
			this._pathToFollowIndex++;
		}

		return tempDirection;
	}
	public override void Repath()
	{
		// Función abstracta necesaria por la clase AbstractPathMind
	}


	List<CellInfo> AStarPathfinding(BoardInfo boardInfo, CellInfo currentPos)
	{		
		CellInfo startCellInfo = currentPos;
		CellInfo targetCellInfo = boardInfo.Exit;

		List<CellInfo> openSet = new List<CellInfo>();					//Nodos por los que navegamos
		HashSet<CellInfo> closedSet = new HashSet<CellInfo>();			//Nodos que han sido abiertos para ser explorados (por lo tanto se saben sus valores).
		openSet.Add(startCellInfo);

		while (openSet.Count > 0)
		{
			CellInfo node = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{

				//Cogemos el nodo con menor coste total (f)
				if (openSet[i].fCost <= node.fCost)
				{
					//En el caso de tener dos o más nodos con la misma f, escogemos el que está más cerca del nodo meta.
					if (openSet[i].hCost < node.hCost)
						node = openSet[i];
				}
			}

			openSet.Remove(node);				//Mandamos el nodo que ya ha sido evaluado a la explorada.
			closedSet.Add(node);

			if (node == targetCellInfo)		//En el caso de que estemos en el nodo meta...
			{
				List<CellInfo> path = ReconstructPath(startCellInfo, targetCellInfo);		//Recalculamos toda la ruta recorrida para usarla en locomoción
				return path;
			}

			//Si no estamos en el nodo meta, necesitamos seguir buscando:

			foreach (CellInfo neighbour in node.WalkableNeighbours(boardInfo))
			{
				if(neighbour != null)
				{
					//Si el vecino es un obstáculo o ya ha sido explorado, se salta.
					if (!neighbour.Walkable || closedSet.Contains(neighbour))
					{
						continue;
					}


					Instantiate(closedListPrefab).transform.position = neighbour.GetPosition; //Indicador visual de los nodos explorados


					float newCostToNeighbour = node.gCost + Distance(node, neighbour);		//Comprobamos si la distancia entre 
					print(newCostToNeighbour+" vs "+node.gCost);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = Distance(neighbour, targetCellInfo);
						neighbour.parent = node;

						if (!openSet.Contains(neighbour))
                        {
							openSet.Add(neighbour);
							
						}
					}
				}
				
			}
		}
		return null;
	}

	List<CellInfo> ReconstructPath(CellInfo startCellInfo, CellInfo endCellInfo)
	{
		List<CellInfo> path = new List<CellInfo>();
		CellInfo currentCellInfo = endCellInfo;

		while (currentCellInfo != startCellInfo)
		{
			path.Add(currentCellInfo);

			Instantiate(openListPrefab).transform.position = currentCellInfo.GetPosition;
			currentCellInfo = currentCellInfo.parent;
		}
		path.Reverse();

		return path;

	}

	float Distance(CellInfo a, CellInfo b)
	{
		return Vector2.Distance(a.GetPosition, b.GetPosition);
	}

}