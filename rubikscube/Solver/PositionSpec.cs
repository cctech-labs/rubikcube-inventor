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
	struct PositionSpec
	{
		public Cube3D.RubikPosition CubePosition { get; set; }
		public Face3D.FacePosition FacePosition { get; set; }
	}
}
