using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataStructures;

namespace Assets.Scripts.DirectionOperations
{
    public class DirectionOperations
    {
        //Devuelve la direccion acorde en función de la posicion actual y la posicion adjacente a la que hayq ue moverse
        public static Locomotion.MoveDirection CalculateDirectionToAdjacentCell(CellInfo currentPosition, CellInfo adjacentPosition)
        {
            Vector2 vectorDifference = adjacentPosition.GetPosition - currentPosition.GetPosition;

            if (vectorDifference.x == 1)
            {
                return Locomotion.MoveDirection.Right;
            }
            else if (vectorDifference.x == -1)
            {
                return Locomotion.MoveDirection.Left;
            }
            if (vectorDifference.y == 1)
            {
                return Locomotion.MoveDirection.Up;
            }
            else
            {
                return Locomotion.MoveDirection.Down;
            }
        }
    }
}
