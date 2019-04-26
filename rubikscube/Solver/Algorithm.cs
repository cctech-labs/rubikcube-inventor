/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RubikSolver
{
	class Algorithm
	{
		public List<LayerMove> Moves;

		public Algorithm(string moves)
		{
			Moves = new List<LayerMove>();
			foreach (string s in moves.Split(char.Parse(" ")))
			{
				Moves.Add(LayerMove.Parse(s));
			}
		}
	}
}
