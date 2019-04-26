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
	struct RotationInfo
	{
		public bool Rotating { get; set; }
		public int Milliseconds { get; set; } //in ms
		public bool Direction { get; set; }
		public Cube3D.RubikPosition Layer { get; set; }
		public int Target { get; set; }
	}
}
