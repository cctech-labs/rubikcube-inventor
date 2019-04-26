/*  ******************************************************************************
    Copyright (c) 2019 Centre for Computational Technologies Pvt. Ltd.(CCTech) .
    All Rights Reserved. Licensed under the MIT License .  
    See License.txt in the project root for license information .
******************************************************************************  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RubikSolver
{
	struct RenderInfo
	{
		public Point MousePosition { get; set; }
		public PositionSpec SelectedPos { get; set; }
		public IEnumerable<Face3D> FacesProjected { get; set; }
	}
}
